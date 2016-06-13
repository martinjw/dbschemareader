using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class VistaDb
    {

        [TestMethod, TestCategory("VistaDb")]
        public void VistaDbTest()
        {
            //using VistaDb 4.2.18.4 (trial) with samples in default location
            const string providername = "System.Data.VistaDB";
            const string connectionString = @"Data Source='C:\Users\Public\Documents\VistaDB\Databases\TicketSystemSample.vdb4'";

            ProviderChecker.Check(providername, connectionString);

            //DatabaseSchemaReader.Utilities.DiscoverProviderFactory.Discover(connectionString, providername);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();

            Assert.IsTrue(schema.Tables.Count > 0);
        }
        //MetaDataCollections
        //DataSourceInformation
        //	CompositeIdentifierSeparatorPattern	String
        //	DataSourceProductName	String
        //	DataSourceProductVersion	String
        //	DataSourceProductVersionNormalized	String
        //	GroupByBehavior	GroupByBehavior
        //	IdentifierPattern	String
        //	IdentifierCase	IdentifierCase
        //	OrderByColumnsInSelect	Boolean
        //	ParameterMarkerFormat	String
        //	ParameterMarkerPattern	String
        //	ParameterNameMaxLength	Int32
        //	ParameterNamePattern	String
        //	QuotedIdentifierPattern	String
        //	QuotedIdentifierCase	IdentifierCase
        //	StatementSeparatorPattern	String
        //	StringLiteralPattern	String
        //	SupportedJoinOperators	SupportedJoinOperators
        //DataTypes
        //	TypeName	String
        //	ProviderDbType	Int32
        //	ColumnSize	Int64
        //	CreateFormat	String
        //	CreateParameters	String
        //	DataType	String
        //	IsAutoIncrementable	Boolean
        //	IsBestMatch	Boolean
        //	IsCaseSensitive	Boolean
        //	IsFixedLength	Boolean
        //	IsFixedPrecisionScale	Boolean
        //	IsLong	Boolean
        //	IsNullable	Boolean
        //	IsSearchable	Boolean
        //	IsSearchableWithLike	Boolean
        //	IsUnsigned	Boolean
        //	MaximumScale	Int16
        //	MinimumScale	Int16
        //	IsConcurrencyType	Boolean
        //	IsLiteralsSupported	Boolean
        //	LiteralPrefix	String
        //	LiteralSuffix	String
        //Columns
        //	TABLE_CATALOG	String
        //	TABLE_SCHEMA	String
        //	TABLE_NAME	String
        //	COLUMN_NAME	String
        //	ORDINAL_POSITION	Int32
        //	COLUMN_DEFAULT	String
        //	IS_NULLABLE	Boolean
        //	DATA_TYPE	String
        //	CHARACTER_MAXIMUM_LENGTH	Int32
        //	CHARACTER_OCTET_LENGTH	Int32
        //	NUMERIC_PRECISION	Int32
        //	NUMERIC_PRECISION_RADIX	Int16
        //	NUMERIC_SCALE	Int32
        //	DATETIME_PRECISION	Int64
        //	CHARACTER_SET_CATALOG	String
        //	CHARACTER_SET_SCHEMA	String
        //	CHARACTER_SET_NAME	String
        //	COLLATION_CATALOG	String
        //	COLLATION_SCHEMA	String
        //	COLLATION_NAME	String
        //	DOMAIN_CATALOG	String
        //	DOMAIN_NAME	String
        //	DESCRIPTION	String
        //	PRIMARY_KEY	Boolean
        //	COLUMN_CAPTION	String
        //	COLUMN_ENCRYPTED	Boolean
        //	COLUMN_PACKED	Boolean
        //	TYPE_GUID	Guid
        //	COLUMN_HASDEFAULT	Boolean
        //	COLUMN_GUID	Guid
        //	COLUMN_PROPID	Int64
        //Indexes
        //	CONSTRAINT_CATALOG	String
        //	CONSTRAINT_SCHEMA	String
        //	CONSTRAINT_NAME	String
        //	TABLE_CATALOG	String
        //	TABLE_SCHEMA	String
        //	TABLE_NAME	String
        //	TYPE_DESC	String
        //	INDEX_NAME	String
        //	PRIMARY_KEY	Boolean
        //	UNIQUE	Boolean
        //	FOREIGN_KEY_INDEX	Boolean
        //	EXPRESSION	String
        //	FULLTEXTSEARCH	Boolean
        //IndexColumns
        //	CONSTRAINT_CATALOG	String
        //	CONSTRAINT_SCHEMA	String
        //	CONSTRAINT_NAME	String
        //	TABLE_CATALOG	String
        //	TABLE_SCHEMA	String
        //	TABLE_NAME	String
        //	COLUMN_NAME	String
        //	ORDINAL_POSITION	Int32
        //	KEYTYPE	UInt16
        //	INDEX_NAME	String
        //Tables
        //	TABLE_CATALOG	String
        //	TABLE_SCHEMA	String
        //	TABLE_NAME	String
        //	TABLE_TYPE	String
        //	TABLE_DESCRIPTION	String
        //ForeignKeys
        //	CONSTRAINT_CATALOG	String
        //	CONSTRAINT_SCHEMA	String
        //	CONSTRAINT_NAME	String
        //	TABLE_CATALOG	String
        //	TABLE_SCHEMA	String
        //	TABLE_NAME	String
        //	CONSTRAINT_TYPE	String
        //	IS_DEFERRABLE	String
        //	INITIALLY_DEFERRED	String
        //	FKEY_TO_TABLE	String
        //	FKEY_TO_CATALOG	String
        //	FKEY_TO_SCHEMA	String
        //ForeignKeyColumns
        //	CONSTRAINT_CATALOG	String
        //	CONSTRAINT_SCHEMA	String
        //	CONSTRAINT_NAME	String
        //	TABLE_CATALOG	String
        //	TABLE_SCHEMA	String
        //	TABLE_NAME	String
        //	CONSTRAINT_TYPE	String
        //	IS_DEFERRABLE	Boolean
        //	INITIALLY_DEFERRED	Boolean
        //	FKEY_FROM_COLUMN	String
        //	FKEY_FROM_ORDINAL_POSITION	Int32
        //	FKEY_TO_CATALOG	String
        //	FKEY_TO_SCHEMA	String
        //	FKEY_TO_TABLE	String
        //	FKEY_TO_COLUMN	String
        //ReservedWords
        //	ReservedWord	String
        //Views
        //	TABLE_CATALOG	String
        //	TABLE_SCHEMA	String
        //	TABLE_NAME	String
        //	CHECK_OPTION	String
        //	IS_UPDATABLE	Boolean
        //	TABLE_DESCRIPTION	String
        //ViewColumns
        //	VIEW_CATALOG	String
        //	VIEW_SCHEMA	String
        //	VIEW_NAME	String
        //	TABLE_CATALOG	String
        //	TABLE_SCHEMA	String
        //	TABLE_NAME	String
        //	COLUMN_NAME	String
        //	COLUMN_GUID	Guid
        //	COLUMN_PROPID	Int64
        //	ORDINAL_POSITION	Int32
        //	COLUMN_HASDEFAULT	Boolean
        //	COLUMN_DEFAULT	String
        //	IS_NULLABLE	Boolean
        //	DATA_TYPE	String
        //	TYPE_GUID	Guid
        //	CHARACTER_MAXIMUM_LENGTH	Int32
        //	CHARACTER_OCTET_LENGTH	Int32
        //	NUMERIC_PRECISION	Int32
        //	NUMERIC_SCALE	Int32
        //	DATETIME_PRECISION	Int64
        //	CHARACTER_SET_CATALOG	String
        //	CHARACTER_SET_SCHEMA	String
        //	CHARACTER_SET_NAME	String
        //	COLLATION_CATALOG	String
        //	COLLATION_SCHEMA	String
        //	COLLATION_NAME	String
        //	DOMAIN_CATALOG	String
        //	DOMAIN_NAME	String
        //	DESCRIPTION	String
        //	PRIMARY_KEY	Boolean
        //	COLUMN_CAPTION	String
        //	COLUMN_ENCRYPTED	Boolean
        //	COLUMN_PACKED	Boolean
        //Restrictions
        //	CollectionName	String
        //	RestrictionName	String
        //	RestrictionDefault	String
        //	RestrictionNumber	Int32



    }
}
