using DatabaseSchemaReader;
using DatabaseSchemaReader.Utilities.DbProvider;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class Sybase
    {

        [TestMethod, TestCategory("Sybase")]
        public void SybaseAseTest()
        {
            //using pubs3 with default sa account with Ase Developer Edition 15 on localhost (had to use IP address to get it to connect)
            const string providername = "Sybase.Data.AseClient";
            const string connectionString = "Server=192.168.1.100;Port=5000;Uid=sa;Pwd='';Initial Catalog=pubs3";

            ProviderChecker.Check(providername, connectionString);

            DiscoverProviderFactory.Discover(connectionString, providername);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();

            Assert.IsTrue(schema.Tables.Count > 0);
        }

        [TestMethod, TestCategory("Sybase")]
        public void SybaseAnyWhereTest()
        {
            const string providername = "iAnyWhere.Data.SQLAnyWhere";
            const string connectionString = "Data Source=SQL Anywhere 12 Demo";

            ProviderChecker.Check(providername, connectionString);

            DiscoverProviderFactory.Discover(connectionString, providername);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();

            Assert.IsTrue(schema.Tables.Count > 0);
        }

        [TestMethod, TestCategory("Sybase")]
        public void SybaseUltraLiteTest()
        {
            const string providername = "iAnyWhere.Data.UltraLite";
            //default sample location on Windows 7
            //In .Net 4 this isn't added to machine.config, so add the DbProvider in app.config
            const string connectionString = @"DBF=C:\Users\Public\Documents\SQL Anywhere 12\Samples\UltraLite.NET\CustDB\custdb.udb";

            ProviderChecker.Check(providername, connectionString);

            var dbReader = new DatabaseReader(connectionString, providername);
            var schema = dbReader.ReadAll();

            Assert.IsTrue(schema.Tables.Count > 0);
        }
        //Columns
        //	table_name	String
        //	column_name	String
        //	default	String
        //	nulls	String
        //DataSourceInformation
        //	CompositeIdentifierSeparatorPattern	String
        //	DataSourceProductName	String
        //	DataSourceProductVersion	String
        //	DataSourceProductVersionNormalized	String
        //	GroupByBehavior	String
        //	IdentifierPattern	String
        //	IdentifierCase	String
        //	OrderByColumnsInSelect	String
        //	ParameterMarkerFormat	String
        //	ParameterMarkerPattern	String
        //	ParameterNameMaxLength	String
        //	ParameterNamePattern	String
        //	QuotedIdentifierPattern	String
        //	QuotedIdentifierCase	String
        //	StatementSeparatorPattern	String
        //	StringLiteralPattern	String
        //	SupportedJoinOperators	String
        //	CaseSensitive	String
        //	CharSet	String
        //	Collation	String
        //	ConnCount	String
        //	date_format	String
        //	date_order	String
        //	Encryption	String
        //	File	String
        //	global_database_id	String
        //	ml_remote_id	String
        //	Name	String
        //	nearest_century	String
        //	PageSize	String
        //	precision	String
        //	scale	String
        //	time_format	String
        //	timestamp_format	String
        //DataTypes
        //	TypeName	String
        //	ProviderDbType	String
        //	ColumnSize	Int32
        //	CreateFormat	String
        //	CreateParameters	String
        //	DataType	Type
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
        //	MaximumScale	Int32
        //	MinimumScale	Int32
        //	IsConcurrencyType	Boolean
        //	IsLiteralSupported	Boolean
        //	LiteralPrefix	String
        //	LiteralSuffix	String
        //ForeignKeys
        //	table_name	String
        //	index_name	String
        //	column_name	String
        //	order	String
        //IndexColumns
        //	table_name	String
        //	index_name	String
        //	type	String
        //	column_name	String
        //	order	String
        //Indexes
        //	table_name	String
        //	index_name	String
        //	type	String
        //MetaDataCollections
        //Publications
        //	publication_name	String
        //ReservedWords
        //	reserved_word	String
        //Restrictions
        //	CollectionName	String
        //	RestrictionName	String
        //	RestrictionDefault	String
        //	RestrictionNumber	String
        //Tables
        //	object_id	String
        //	table_name	String
        //	table_type	String
        //	sync_type	String


    }
}
