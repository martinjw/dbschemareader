using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;
using Microsoft.CSharp;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    public class EnumTypeList : SqlExecuter<DataType>
    {
        public EnumTypeList()
        {
            Sql = @"select * from pg_type pgt join pg_enum pge on pgt.oid = pge.enumtypid;";
        }

        protected override void AddParameters(DbCommand command)
        {
            //AddDbParameter(command, "OWNER", Owner);
            //AddDbParameter(command, "TABLENAME", _tableName);
        }

        protected override void Mapper(IDbCommand cmd)
        {
            var enumTypeNameToValueListDictionary = new Dictionary<string, List<string>>();
            using (var dr = cmd.ExecuteReader())
            {
                while (dr.Read())
                {
                    var typname = dr["typname"].ToString();
                    var enumlabel = dr["enumlabel"].ToString();
                    List<string> values;
                    if (enumTypeNameToValueListDictionary.TryGetValue(typname, out values))
                    {
                        if (!values.Contains(enumlabel))
                        {
                            values.Add(enumlabel);
                        }

                        continue;
                    }

                    values = new List<string>() { enumlabel };
                    enumTypeNameToValueListDictionary.Add(typname, values);
                }
            }

            foreach (var entry in enumTypeNameToValueListDictionary)
            {
                Result.Add(new EnumeratedDataType(entry.Key, entry.Key)
                {
                    EnumerationValues = entry.Value
                });
            }
        }

        protected override void Mapper(IDataRecord record)
        {
        }

        public IList<DataType> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReaderWhole(connectionAdapter);
            return Result;
        }
    }
}

