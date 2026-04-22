IF OBJECT_ID(N'dbo.DEXMeter', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DEXMeter
    (
        DexMeterId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DEXMeter PRIMARY KEY DEFAULT NEWID(),
        Machine CHAR(1) NOT NULL,
        DexDateTime DATETIME2(0) NOT NULL,
        MachineSerialNumber NVARCHAR(50) NOT NULL,
        ValueOfPaidVends DECIMAL(18, 2) NOT NULL,
        RawDexContent NVARCHAR(MAX) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_DEXMeter_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT UQ_DEXMeter_Machine_DexDateTime UNIQUE (Machine, DexDateTime)
    );
END
GO

IF OBJECT_ID(N'dbo.DEXLaneMeter', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.DEXLaneMeter
    (
        DexLaneMeterId UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_DEXLaneMeter PRIMARY KEY DEFAULT NEWID(),
        DexMeterId UNIQUEIDENTIFIER NOT NULL,
        ProductIdentifier NVARCHAR(20) NOT NULL,
        Price DECIMAL(18, 2) NOT NULL,
        NumberOfVends INT NOT NULL,
        ValueOfPaidSales DECIMAL(18, 2) NOT NULL,
        CreatedAtUtc DATETIME2(0) NOT NULL CONSTRAINT DF_DEXLaneMeter_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_DEXLaneMeter_DEXMeter FOREIGN KEY (DexMeterId) REFERENCES dbo.DEXMeter (DexMeterId)
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.SaveDexMeter
    @Machine CHAR(1),
    @DexDateTime DATETIME2(0),
    @MachineSerialNumber NVARCHAR(50),
    @ValueOfPaidVends DECIMAL(18, 2),
    @RawDexContent NVARCHAR(MAX),
    @DexMeterId UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    SET @DexMeterId = NEWID();

    INSERT INTO dbo.DEXMeter
    (
        DexMeterId,
        Machine,
        DexDateTime,
        MachineSerialNumber,
        ValueOfPaidVends,
        RawDexContent
    )
    VALUES
    (
        @DexMeterId,
        @Machine,
        @DexDateTime,
        @MachineSerialNumber,
        @ValueOfPaidVends,
        @RawDexContent
    );
END
GO

CREATE OR ALTER PROCEDURE dbo.SaveDexLaneMeter
    @DexMeterId UNIQUEIDENTIFIER,
    @ProductIdentifier NVARCHAR(20),
    @Price DECIMAL(18, 2),
    @NumberOfVends INT,
    @ValueOfPaidSales DECIMAL(18, 2)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.DEXLaneMeter
    (
        DexMeterId,
        ProductIdentifier,
        Price,
        NumberOfVends,
        ValueOfPaidSales
    )
    VALUES
    (
        @DexMeterId,
        @ProductIdentifier,
        @Price,
        @NumberOfVends,
        @ValueOfPaidSales
    );
END
GO
