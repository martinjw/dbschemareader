using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.SqlGen.PostgreSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DatabaseSchemaReaderTest.SqlGen.PostgreSql
{
    [TestClass]
    public class DataTypesTest
    {
        //documented here: http://developer.postgresql.org/pgdocs/postgres/datatype.html

        /*
         * bigint	int8	signed eight-byte integer
bigserial	serial8	autoincrementing eight-byte integer
bit [ (n) ]	 	fixed-length bit string
bit varying [ (n) ]	varbit	variable-length bit string
boolean	bool	logical Boolean (true/false)
box	 	rectangular box on a plane
bytea	 	binary data ("byte array")
character varying [ (n) ]	varchar [ (n) ]	variable-length character string
character [ (n) ]	char [ (n) ]	fixed-length character string
cidr	 	IPv4 or IPv6 network address
circle	 	circle on a plane
date	 	calendar date (year, month, day)
double precision	float8	double precision floating-point number (8 bytes)
inet	 	IPv4 or IPv6 host address
integer	int, int4	signed four-byte integer
interval [ fields ] [ (p) ]	 	time span
line	 	infinite line on a plane
lseg	 	line segment on a plane
macaddr	 	MAC (Media Access Control) address
money	 	currency amount
numeric [ (p, s) ]	decimal [ (p, s) ]	exact numeric of selectable precision
path	 	geometric path on a plane
point	 	geometric point on a plane
polygon	 	closed geometric path on a plane
real	float4	single precision floating-point number (4 bytes)
smallint	int2	signed two-byte integer
serial	serial4	autoincrementing four-byte integer
text	 	variable-length character string
time [ (p) ] [ without time zone ]	 	time of day (no time zone)
time [ (p) ] with time zone	timetz	time of day, including time zone
timestamp [ (p) ] [ without time zone ]	 	date and time (no time zone)
timestamp [ (p) ] with time zone	timestamptz	date and time, including time zone
tsquery	 	text search query
tsvector	 	text search document
txid_snapshot	 	user-level transaction ID snapshot
uuid	 	universally unique identifier
xml	 	XML data
         */

        private readonly DataTypeWriter _typeWriter = new DataTypeWriter();
        private readonly DatabaseColumn _column = new DatabaseColumn();

        [TestMethod]
        public void TestInteger()
        {
            //arrange
            _column.DbDataType = "INT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("INTEGER", result);
        }

        [TestMethod]
        public void TestInt2()
        {
            //arrange
            _column.DbDataType = "INT2";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("SMALLINT", result);
        }

        [TestMethod]
        public void TestSmallInt()
        {
            //arrange
            _column.DbDataType = "SMALLINT";

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("SMALLINT", result);
        }

        [TestMethod]
        public void TestNumber()
        {
            //arrange
            _column.DbDataType = "NUMBER";
            _column.Precision = 10;
            _column.Scale = 2;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NUMERIC (10,2)", result);
        }

        [TestMethod]
        public void TestNumeric()
        {
            //arrange
            _column.DbDataType = "NUMERIC";
            _column.Precision = 10;
            _column.Scale = 2;

            //act
            var result = _typeWriter.WriteDataType(_column);

            //assert
            Assert.AreEqual("NUMERIC (10,2)", result);
        }

    }
}
