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
    public enum CustomerContactType
    {
        LIAISON_CONTACT,
        INSTALL_CONTACT,
        BILLING_CONTACT,
    }

    class EnumTypeList : SqlExecuter<DataType>
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

            var compileUnit = new CodeCompileUnit();
            var theNamespace = new CodeNamespace("test");
            theNamespace.Imports.Add(new CodeNamespaceImport("System"));
            compileUnit.Namespaces.Add(theNamespace);

            foreach (var e in enumTypeNameToValueListDictionary)
            {
                var myEnumeration = new CodeTypeDeclaration(e.Key);
                myEnumeration.IsEnum = true;
                myEnumeration.Attributes = MemberAttributes.Public;
                foreach (var v in e.Value)
                {
                    var _v = v.Replace(" ", "_");
                    var value = new CodeMemberField(e.Key, _v);
                    myEnumeration.Members.Add(value);
                }

                theNamespace.Types.Add(myEnumeration);

            }

            var provider = new CSharpCodeProvider();
            using (StreamWriter sw = new StreamWriter($"tst.cs", false))
            {
                var tw = new IndentedTextWriter(sw, "    ");
                provider.GenerateCodeFromCompileUnit(compileUnit, tw, new CodeGeneratorOptions());
                tw.Close();
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

