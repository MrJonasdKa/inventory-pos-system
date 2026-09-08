-- =========================================================
-- inventory-pos-system — Database Schema (MariaDB)
-- =========================================================
-- Run as root (or a user with CREATE privileges):
--   mariadb -u root -p < schema.sql
--
-- This script creates the database, all tables, and a
-- dedicated least-privilege application user. Root is NOT
-- used by the app itself — the app connects as
-- `inventory_app`, scoped only to this database.
-- =========================================================

CREATE DATABASE IF NOT EXISTS inventory_pos
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE inventory_pos;

-- ---------------------------------------------------------
-- Roles & Users
-- ---------------------------------------------------------

CREATE TABLE roles (
    id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(50) NOT NULL UNIQUE,       -- admin, cashier, inventory_manager
    description VARCHAR(255) NULL
) ENGINE=InnoDB;

CREATE TABLE users (
    id            INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    username      VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,           -- BCrypt hash, never plaintext
    full_name     VARCHAR(100) NOT NULL,
    role_id       INT UNSIGNED NOT NULL,
    is_active     TINYINT(1) NOT NULL DEFAULT 1,
    created_at    TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at    TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_users_role FOREIGN KEY (role_id) REFERENCES roles(id)
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- Catalog
-- ---------------------------------------------------------

CREATE TABLE categories (
    id          INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name        VARCHAR(100) NOT NULL UNIQUE,
    description VARCHAR(255) NULL
) ENGINE=InnoDB;

CREATE TABLE suppliers (
    id           INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    name         VARCHAR(150) NOT NULL,
    contact_name VARCHAR(100) NULL,
    phone        VARCHAR(30) NULL,
    email        VARCHAR(150) NULL,
    address      VARCHAR(255) NULL,
    created_at   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB;

CREATE TABLE products (
    id                INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    sku               VARCHAR(50) NOT NULL UNIQUE,
    name              VARCHAR(150) NOT NULL,
    description       VARCHAR(500) NULL,
    category_id       INT UNSIGNED NULL,
    unit_price        DECIMAL(12,2) NOT NULL,
    cost_price        DECIMAL(12,2) NOT NULL,
    reorder_threshold INT NOT NULL DEFAULT 0,
    is_active         TINYINT(1) NOT NULL DEFAULT 1,
    created_at        TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at        TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_products_category FOREIGN KEY (category_id) REFERENCES categories(id)
) ENGINE=InnoDB;

CREATE INDEX idx_products_category ON products(category_id);

-- ---------------------------------------------------------
-- Stock tracking
-- ---------------------------------------------------------

CREATE TABLE stock_levels (
    product_id     INT UNSIGNED PRIMARY KEY,
    quantity_on_hand INT NOT NULL DEFAULT 0,
    updated_at     TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_stocklevels_product FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB;

-- Every stock change is recorded here — this table is
-- append-only (never UPDATE/DELETE a row), it's the audit trail.
CREATE TABLE stock_movements (
    id              BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    product_id      INT UNSIGNED NOT NULL,
    movement_type   ENUM('purchase','sale','adjustment','return') NOT NULL,
    quantity_change INT NOT NULL,                  -- positive or negative
    note            VARCHAR(255) NULL,
    performed_by    INT UNSIGNED NOT NULL,
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_stockmovements_product FOREIGN KEY (product_id) REFERENCES products(id),
    CONSTRAINT fk_stockmovements_user FOREIGN KEY (performed_by) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_stockmovements_product ON stock_movements(product_id);
CREATE INDEX idx_stockmovements_created ON stock_movements(created_at);

-- ---------------------------------------------------------
-- Sales / POS
-- ---------------------------------------------------------

CREATE TABLE sales (
    id              BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    sale_number     VARCHAR(30) NOT NULL UNIQUE,
    cashier_id      INT UNSIGNED NOT NULL,
    total_amount    DECIMAL(12,2) NOT NULL,
    discount_amount DECIMAL(12,2) NOT NULL DEFAULT 0,
    payment_method  ENUM('cash','card','other') NOT NULL DEFAULT 'cash',
    status          ENUM('completed','voided') NOT NULL DEFAULT 'completed',
    created_at      TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_sales_cashier FOREIGN KEY (cashier_id) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE INDEX idx_sales_created ON sales(created_at);

CREATE TABLE sale_items (
    id                 BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    sale_id            BIGINT UNSIGNED NOT NULL,
    product_id         INT UNSIGNED NOT NULL,
    quantity           INT NOT NULL,
    unit_price_at_sale DECIMAL(12,2) NOT NULL,      -- snapshot, protects history
    line_total         DECIMAL(12,2) NOT NULL,
    CONSTRAINT fk_saleitems_sale FOREIGN KEY (sale_id) REFERENCES sales(id),
    CONSTRAINT fk_saleitems_product FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB;

CREATE INDEX idx_saleitems_sale ON sale_items(sale_id);

-- ---------------------------------------------------------
-- Purchasing (feeds stock_movements on receipt)
-- ---------------------------------------------------------

CREATE TABLE purchase_orders (
    id           INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    supplier_id  INT UNSIGNED NOT NULL,
    status       ENUM('pending','received','cancelled') NOT NULL DEFAULT 'pending',
    created_by   INT UNSIGNED NOT NULL,
    created_at   TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_po_supplier FOREIGN KEY (supplier_id) REFERENCES suppliers(id),
    CONSTRAINT fk_po_user FOREIGN KEY (created_by) REFERENCES users(id)
) ENGINE=InnoDB;

CREATE TABLE purchase_order_items (
    id                 INT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
    purchase_order_id  INT UNSIGNED NOT NULL,
    product_id         INT UNSIGNED NOT NULL,
    quantity_ordered   INT NOT NULL,
    quantity_received  INT NOT NULL DEFAULT 0,
    unit_cost          DECIMAL(12,2) NOT NULL,
    CONSTRAINT fk_poitems_po FOREIGN KEY (purchase_order_id) REFERENCES purchase_orders(id),
    CONSTRAINT fk_poitems_product FOREIGN KEY (product_id) REFERENCES products(id)
) ENGINE=InnoDB;

-- ---------------------------------------------------------
-- Seed data — roles + a starter admin user
-- ---------------------------------------------------------

INSERT INTO roles (name, description) VALUES
    ('admin', 'Full access to all modules'),
    ('cashier', 'POS/checkout access only'),
    ('inventory_manager', 'Inventory and purchasing access');

-- NOTE: replace this password_hash with a real BCrypt hash
-- generated by the app once auth is built. This is a placeholder
-- so the table isn't empty during early development.
INSERT INTO users (username, password_hash, full_name, role_id, is_active) VALUES
    ('admin', 'REPLACE_WITH_BCRYPT_HASH', 'Default Admin', 1, 1);

-- ---------------------------------------------------------
-- Dedicated least-privilege application user
-- ---------------------------------------------------------
-- The WPF app connects as THIS user, never as root.
-- Change 'change_me_strong_password' before using anywhere
-- beyond your own local dev machine.

CREATE USER IF NOT EXISTS 'inventory_app'@'%'
    IDENTIFIED BY 'change_me_strong_password';

GRANT SELECT, INSERT, UPDATE, DELETE ON inventory_pos.* TO 'inventory_app'@'%';

FLUSH PRIVILEGES;
