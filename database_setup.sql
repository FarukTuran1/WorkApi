-- Veritabanını oluştur (eğer zaten yoksa) ve kullan
CREATE DATABASE IF NOT EXISTS work_db DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE work_db;

-- users tablosu
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role ENUM('admin', 'customer') NOT NULL, -- *** Gerekirse rolleri düzenle ***
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- products tablosu
CREATE TABLE IF NOT EXISTS products (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    description TEXT NULL,
    price DECIMAL(10, 2) NOT NULL,
    stock_quantity INT NOT NULL DEFAULT 0,
    category ENUM('Elektronik', 'Giyim', 'Kitap', 'Diğer') NOT NULL, -- *** Kendi kategorilerini buraya yaz ***
    image_url VARCHAR(512) NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- orders tablosu
CREATE TABLE IF NOT EXISTS orders (
    id INT AUTO_INCREMENT PRIMARY KEY,
    user_id INT NOT NULL,
    total_price DECIMAL(10, 2) NOT NULL,
    status ENUM('pending', 'processing', 'shipped', 'delivered', 'cancelled') NOT NULL DEFAULT 'pending', -- *** Kendi durumlarını buraya yaz ***
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_user_id (user_id),
    FOREIGN KEY (user_id) REFERENCES users(id)
        ON DELETE RESTRICT -- Kullanıcı silinirse siparişleri silme (varsayılan, daha güvenli)
        ON UPDATE CASCADE -- Kullanıcı id değişirse siparişlerde de güncelle
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- order_items tablosu
CREATE TABLE IF NOT EXISTS order_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    price DECIMAL(10, 2) NOT NULL COMMENT 'Sipariş anındaki birim fiyat',
    INDEX idx_order_id (order_id),
    INDEX idx_product_id (product_id),
    FOREIGN KEY (order_id) REFERENCES orders(id)
        ON DELETE CASCADE -- Sipariş silinirse, ilgili kalemleri de sil
        ON UPDATE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id)
        ON DELETE RESTRICT -- Ürün silinmeye çalışılırsa (siparişte varsa) engelle
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- payments tablosu
CREATE TABLE IF NOT EXISTS payments (
    id INT AUTO_INCREMENT PRIMARY KEY,
    order_id INT NOT NULL,
    payment_method VARCHAR(50) NOT NULL,
    payment_status ENUM('pending', 'completed', 'failed', 'refunded') NOT NULL DEFAULT 'pending', -- *** Kendi durumlarını buraya yaz ***
    transaction_id VARCHAR(255) NULL UNIQUE COMMENT 'Ödeme sağlayıcısından gelen işlem IDsi',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_order_id (order_id),
    INDEX idx_transaction_id (transaction_id),
    FOREIGN KEY (order_id) REFERENCES orders(id)
        ON DELETE RESTRICT -- Sipariş silinirse ödemeyi silme (belki log için tutulmalı?)
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- admin_logs tablosu
CREATE TABLE IF NOT EXISTS admin_logs (
    id INT AUTO_INCREMENT PRIMARY KEY,
    admin_id INT NULL, -- Admin silinirse log kalsın diye NULL olabilir
    action TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_admin_id (admin_id),
    FOREIGN KEY (admin_id) REFERENCES users(id)
        ON DELETE SET NULL -- Admin kullanıcısı silinirse logdaki admin_id'yi NULL yap
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- İsteğe bağlı: Başlangıçta bir admin kullanıcısı ekleyebilirsiniz (Şifreyi güvenli bir şekilde hash'lemeyi unutmayın!)
-- INSERT INTO users (name, email, password_hash, role) VALUES ('Admin User', 'admin@example.com', 'GUVENLI_BIR_HASH_DEGERI', 'admin'); 