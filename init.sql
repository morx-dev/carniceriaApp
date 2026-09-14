CREATE DATABASE IF NOT EXISTS carniceria_db
  CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

USE carniceria_db;

CREATE TABLE Roles (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO Roles (Nombre) VALUES
('Administrador'), ('CallCenter'), ('Mostrador'), ('Repartidor');

CREATE TABLE Productos (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(150) NOT NULL,
    Categoria VARCHAR(80),
    UnidadMedida VARCHAR(20) NOT NULL DEFAULT 'Libra',
    PrecioActual DECIMAL(10,2) NOT NULL,
    Activo BOOLEAN DEFAULT TRUE
);

CREATE TABLE Clientes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(150) NOT NULL,
    Telefono VARCHAR(20),
    Direccion VARCHAR(255),
    Referencia VARCHAR(255),
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE EstadosVenta (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nombre VARCHAR(50) NOT NULL UNIQUE
);

INSERT INTO EstadosVenta (Nombre) VALUES
('Pendiente'), ('En preparacion'), ('En camino'), ('Entregado'), ('Cancelado');

CREATE TABLE Ventas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ClienteId INT NULL,
    TipoOrigen ENUM('Presencial','WhatsApp','Llamada') NOT NULL,
    EstadoId INT NOT NULL DEFAULT 1,
    UsuarioCreadorId VARCHAR(450) NOT NULL,
    RepartidorId VARCHAR(450) NULL,
    Total DECIMAL(10,2) NOT NULL DEFAULT 0,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP,
    FechaEntrega DATETIME NULL,
    FOREIGN KEY (ClienteId) REFERENCES Clientes(Id),
    FOREIGN KEY (EstadoId) REFERENCES EstadosVenta(Id)
);

CREATE TABLE DetalleVentas (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    VentaId INT NOT NULL,
    ProductoId INT NOT NULL,
    Cantidad DECIMAL(10,2) NOT NULL,
    PrecioUnitario DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (VentaId) REFERENCES Ventas(Id) ON DELETE CASCADE,
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);

CREATE TABLE HistorialPrecios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    ProductoId INT NOT NULL,
    PrecioAnterior DECIMAL(10,2) NOT NULL,
    PrecioNuevo DECIMAL(10,2) NOT NULL,
    FechaCambio DATETIME DEFAULT CURRENT_TIMESTAMP,
    UsuarioId VARCHAR(450) NOT NULL,
    FOREIGN KEY (ProductoId) REFERENCES Productos(Id)
);

CREATE TABLE CuadresDiarios (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Fecha DATE NOT NULL,
    UsuarioId VARCHAR(450) NOT NULL,
    TotalVentasPresenciales DECIMAL(10,2) DEFAULT 0,
    TotalVentasSistema DECIMAL(10,2) DEFAULT 0,
    TotalGeneral DECIMAL(10,2) DEFAULT 0,
    FechaCreacion DATETIME DEFAULT CURRENT_TIMESTAMP
);