using DatabaseSchemaReader;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.IntegrationTests
{
    /// <summary>
    /// These are INTEGRATION tests using databases.
    /// </summary>
    [TestClass]
    public class IntersystemsCache
    {
        [TestMethod, TestCategory("Cache")]
        public void CacheTest()
        {
            //  <system.data>
            //    <DbProviderFactories>
            //<add name="InterSystems Data Provider"
            //     invariant="InterSystems.Data.CacheClient"
            //     description="InterSystem .Net Data Provider"
            //     type="InterSystems.Data.CacheClient.CacheFactory, 
            //   Intersystems.Data.CacheClient, Version=2.0.0.1, Culture=neutral, PublicKeyToken=ad350a26c4a4447c"
            // />
            //    </DbProviderFactories>
            //  </system.data>
            const string providername = "InterSystems.Data.CacheClient";
            const string connectionString = "Server=localhost; Port=1972; Namespace=SAMPLES;Password=SYS; User ID=_SYSTEM;";

            ProviderChecker.Check(providername, connectionString);

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
        //	IsLiteralSupported	Boolean
        //	LiteralPrefix	String
        //	LiteralSuffix	String
        //	SQLType	Int16
        //Restrictions
        //	CollectionName	String
        //	RestrictionName	String
        //	RestrictionDefault	String
        //	RestrictionNumber	Int32
        //ReservedWords
        //	ReservedWord	String
        //Columns
        //	TABLE_QUALIFIER	String
        //	TABLE_OWNER	String
        //	TABLE_NAME	String
        //	COLUMN_NAME	String
        //	DATA_TYPE	Int16
        //	TYPE_NAME	String
        //	PRECISION	Int32
        //	LENGTH	Int32
        //	SCALE	Int16
        //	RADIX	Int16
        //	NULLABLE	Int16
        //	REMARKS	String
        //	COLUMN_DEF	String
        //ColumnPrivileges
        //	TABLE_CAT	String
        //	TABLE_SCHEM	String
        //	TABLE_NAME	String
        //	COLUMN_NAME	String
        //	GRANTOR	String
        //	GRANTEE	String
        //	PRIVILEGE	String
        //	IS_GRANTABLE	String
        //ForeignKeys
        //Either primary or foreign key table name must be specified
        //Indexes
        //Table name must be specified
        //PrimaryKeys
        //Table name must be specified
        //Procedures
        //	PROCEDURE_CAT	String
        //	PROCEDURE_SCHEM	String
        //	PROCEDURE_NAME	String
        //	NUM_INPUT_PARAMS	Int16
        //	NUM_OUTPUT_PARAMS	Int16
        //	NUM_RESULT_SETS	Int16
        //	REMARKS	String
        //	PROCEDURE_TYPE	Int16
        //ProcedureColumns
        //	PROCEDURE_QUALIFIER	String
        //	PROCEDURE_OWNER	String
        //	PROCEDURE_NAME	String
        //	COLUMN_NAME	String
        //	COLUMN_TYPE	Int16
        //	DATA_TYPE	Int16
        //	TYPE_NAME	String
        //	PRECISION	Int32
        //	LENGTH	Int32
        //	SCALE	Int16
        //	RADIX	Int16
        //	NULLABLE	Int16
        //	REMARKS	String
        //ProcedureParameters
        //	PROCEDURE_QUALIFIER	String
        //	PROCEDURE_OWNER	String
        //	PROCEDURE_NAME	String
        //	COLUMN_NAME	String
        //	COLUMN_TYPE	Int16
        //	DATA_TYPE	Int16
        //	TYPE_NAME	String
        //	PRECISION	Int32
        //	LENGTH	Int32
        //	SCALE	Int16
        //	RADIX	Int16
        //	NULLABLE	Int16
        //	REMARKS	String
        //SpecialColumns
        //Object reference not set to an instance of an object.
        //Tables
        //	TABLE_CAT	String
        //	TABLE_SCHEM	String
        //	TABLE_NAME	String
        //	TABLE_TYPE	String
        //	REMARKS	String
        //TablePrivileges
        //	TABLE_CAT	String
        //	TABLE_SCHEM	String
        //	TABLE_NAME	String
        //	GRANTOR	String
        //	GRANTEE	String
        //	PRIVILEGE	String
        //	IS_GRANTABLE	String
        //Views
        //	TABLE_CAT	String
        //	TABLE_SCHEM	String
        //	TABLE_NAME	String
        //	TABLE_TYPE	String
        //	REMARKS	String

    }
}
