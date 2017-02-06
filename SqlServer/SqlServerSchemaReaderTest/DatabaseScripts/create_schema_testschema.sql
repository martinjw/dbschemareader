
SET NOCOUNT ON;
SET XACT_ABORT ON;

--derived from https://raw.githubusercontent.com/Microsoft/sql-server-samples/master/samples/features/in-memory/t-sql-scripts/enable-in-memory-oltp.sql
IF SERVERPROPERTY(N'IsXTPSupported') = 1 
BEGIN  
    BEGIN TRY;
-- 2. add MEMORY_OPTIMIZED_DATA filegroup when not using Azure SQL DB
    IF SERVERPROPERTY('EngineEdition') != 5 
    BEGIN
        DECLARE @SQLDataFolder nvarchar(max) = cast(SERVERPROPERTY('InstanceDefaultDataPath') as nvarchar(max))
        DECLARE @MODName nvarchar(max) = DB_NAME() + N'_mod';
        DECLARE @MemoryOptimizedFilegroupFolder nvarchar(max) = @SQLDataFolder + @MODName;

        DECLARE @SQL nvarchar(max) = N'';

        -- add filegroup
        IF NOT EXISTS (SELECT 1 FROM sys.filegroups WHERE type = N'FX')
        BEGIN
            SET @SQL = N'
ALTER DATABASE CURRENT 
ADD FILEGROUP ' + QUOTENAME(@MODName) + N' CONTAINS MEMORY_OPTIMIZED_DATA;';
            EXECUTE (@SQL);

        END;

        -- add container in the filegroup
        IF NOT EXISTS (SELECT * FROM sys.database_files WHERE data_space_id IN (SELECT data_space_id FROM sys.filegroups WHERE type = N'FX'))
        BEGIN
            SET @SQL = N'
ALTER DATABASE CURRENT
ADD FILE (name = N''' + @MODName + ''', filename = '''
                        + @MemoryOptimizedFilegroupFolder + N''') 
TO FILEGROUP ' + QUOTENAME(@MODName);
            EXECUTE (@SQL);
        END
    END

    -- 3. set compat level to 130 if it is lower
    IF (SELECT compatibility_level FROM sys.databases WHERE database_id=DB_ID()) < 130
        ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 130 

    -- 4. enable MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT for the database
    ALTER DATABASE CURRENT SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT = ON;


    END TRY
    BEGIN CATCH
        PRINT N'Error enabling In-Memory OLTP';
        IF XACT_STATE() != 0
            ROLLBACK;
        THROW;
    END CATCH;
END;

--https://blogs.msdn.microsoft.com/sqlserverstorageengine/2016/11/17/in-memory-oltp-in-standard-and-express-editions-with-sql-server-2016-sp1/
-- configure recommended DB option
 ALTER DATABASE CURRENT SET MEMORY_OPTIMIZED_ELEVATE_TO_SNAPSHOT=ON
 GO
 -- memory-optimized table
 CREATE TABLE dbo.table1
 ( c1 INT IDENTITY PRIMARY KEY NONCLUSTERED,
   c2 NVARCHAR(MAX))
 WITH (MEMORY_OPTIMIZED=ON)
 GO
 -- non-durable table
 CREATE TABLE dbo.temp_table1
 ( c1 INT IDENTITY PRIMARY KEY NONCLUSTERED,
   c2 NVARCHAR(MAX))
 WITH (MEMORY_OPTIMIZED=ON,
       DURABILITY=SCHEMA_ONLY)
 GO
 -- memory-optimized table type
 CREATE TYPE dbo.tt_table1 AS TABLE
 ( c1 INT IDENTITY,
   c2 NVARCHAR(MAX),
   is_transient BIT NOT NULL DEFAULT (0),
   INDEX ix_c1 HASH (c1) WITH (BUCKET_COUNT=1024))
 WITH (MEMORY_OPTIMIZED=ON)
 GO
 -- natively compiled stored procedure
 CREATE PROCEDURE dbo.usp_ingest_table1
   @table1 dbo.tt_table1 READONLY
 WITH NATIVE_COMPILATION, SCHEMABINDING
 AS
 BEGIN ATOMIC
     WITH (TRANSACTION ISOLATION LEVEL=SNAPSHOT,
           LANGUAGE=N'us_english')
 
   DECLARE @i INT = 1
 
   WHILE @i > 0
   BEGIN
     INSERT dbo.table1
     SELECT c2
     FROM @table1
     WHERE c1 = @i AND is_transient=0
 
     IF @@ROWCOUNT > 0
       SET @i += 1
     ELSE
     BEGIN
       INSERT dbo.temp_table1
       SELECT c2
       FROM @table1
       WHERE c1 = @i AND is_transient=1
 
       IF @@ROWCOUNT > 0
         SET @i += 1
       ELSE
         SET @i = 0
     END
   END
 
 END
 GO
if exists (select * from sysobjects where id = object_id('dbo.Products') and sysstat & 0xf = 3)
    drop table [dbo].[Products]
GO
if exists (select * from sysobjects where id = object_id('dbo.Categories') and sysstat & 0xf = 3)
    drop table [dbo].[Categories]
GO

CREATE TABLE [Categories] (
    [CategoryID] [int] IDENTITY (1, 1) NOT NULL ,
    [CategoryName] nvarchar (15) NOT NULL ,
    [Description] [ntext] NULL ,
    [Picture] [image] NULL ,
    CONSTRAINT [PK_Categories] PRIMARY KEY  CLUSTERED 
    (
        [CategoryID]
    )
);
 CREATE  INDEX [CategoryName] ON [dbo].[Categories]([CategoryName]);

CREATE TABLE [Products] (
    [ProductID] [int] IDENTITY (1, 1) NOT NULL ,
    [ProductName] nvarchar (40) NOT NULL ,
    [CategoryID] [int] NULL ,
    [QuantityPerUnit] nvarchar (20) NULL ,
    [UnitPrice] [money] NULL CONSTRAINT [DF_Products_UnitPrice] DEFAULT (0),
    [UnitsInStock] [smallint] NULL CONSTRAINT [DF_Products_UnitsInStock] DEFAULT (0),
    [UnitsOnOrder] [smallint] NULL CONSTRAINT [DF_Products_UnitsOnOrder] DEFAULT (0),
    [ReorderLevel] [smallint] NULL CONSTRAINT [DF_Products_ReorderLevel] DEFAULT (0),
    [Discontinued] [bit] NOT NULL CONSTRAINT [DF_Products_Discontinued] DEFAULT (0),
    CONSTRAINT [PK_Products] PRIMARY KEY  CLUSTERED 
    (
        [ProductID]
    ),
    CONSTRAINT [FK_Products_Categories] FOREIGN KEY 
    (
        [CategoryID]
    ) REFERENCES [dbo].[Categories] (
        [CategoryID]
    ),
    CONSTRAINT [CK_Products_UnitPrice] CHECK (UnitPrice >= 0),
    CONSTRAINT [CK_ReorderLevel] CHECK (ReorderLevel >= 0),
    CONSTRAINT [CK_UnitsInStock] CHECK (UnitsInStock >= 0),
    CONSTRAINT [CK_UnitsOnOrder] CHECK (UnitsOnOrder >= 0)
);
 CREATE  INDEX [CategoriesProducts] ON [dbo].[Products]([CategoryID]);
 CREATE  INDEX [CategoryID] ON [dbo].[Products]([CategoryID]);
 CREATE  INDEX [ProductName] ON [dbo].[Products]([ProductName]);

--populate the Categories table
INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Beverages' ,'Soft drinks, coffees, teas, beers, and ales' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Condiments' ,'Sweet and savory sauces, relishes, spreads, and seasonings' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Confections' ,'Desserts, candies, and sweet breads' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Dairy Products' ,'Cheeses' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Grains/Cereals' ,'Breads, crackers, pasta, and cereal' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Meat/Poultry' ,'Prepared meats' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Produce' ,'Dried fruit and bean curd' ,NULL
);

INSERT INTO [Categories] (
  [CategoryName],  [Description],  [Picture]) VALUES (
'Seafood' ,'Seaweed and fish' ,NULL
);

--this should add a Statistics
SELECT * FROM [Categories] ORDER BY [CategoryName];