using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseSchemaReader.SqlGen.Db2
{
    class ProcedureWriter : IProcedureWriter
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly IList<string> _parameters = new List<string>();
        private string _specificName;
        private bool _hasQuery;

        public ProcedureWriter(string procName, string tableName)
        {
            Start(tableName, procName, null);
        }
        public ProcedureWriter(string procName, string tableName, string schema)
        {
            Start(tableName, procName, schema);
        }

        private void Start(string tableName, string procName, string schema)
        {
            _sb.AppendLine("-- " + procName + " - table " + tableName);
            //depending on your UI you probably need one of these
            _sb.AppendLine("--#SET TERMINATOR ~");
            _sb.AppendLine("--<ScriptOptions statementTerminator=\"~\"/>");

            var sqlFormatProvider = new SqlFormatProvider();
            var name = sqlFormatProvider.Escape(procName);
            _specificName = name;
            if (!string.IsNullOrEmpty(schema)) name = sqlFormatProvider.Escape(schema) + "." + name;

            //specific names have a maximum length of 18 (procedure names can be up to 128 chars)
            if (procName.Length > 18) _specificName = sqlFormatProvider.Escape(procName.Substring(procName.Length - 18, 18));

            // CREATE OR REPLACE syntax is new in DB2 v9.7. For older versions, DROP first
            _sb.AppendLine("CREATE OR REPLACE PROCEDURE " + name + "");
        }

        #region Implementation of IProcedureWriter

        public void AddOutputParameter(string parameterName, string dataType)
        {
            _parameters.Add("\tOUT " + parameterName + "\t" + dataType);
        }

        public void AddParameter(string parameterName, string dataType)
        {
            _parameters.Add("\tIN " + parameterName + "\t" + dataType);
        }

        public void AddIntegerParameter(string parameterName)
        {
            _parameters.Add("\tIN " + parameterName + "\tINTEGER");
        }

        public void AddSql(string sql)
        {
            if (!sql.TrimEnd().EndsWith(";", StringComparison.OrdinalIgnoreCase)) sql += ";";
            _sb.AppendLine(sql);
            _sb.AppendLine();
        }

        public void AddQuerySql(string sql)
        {
            AddSql(sql);
        }

        public void BeginProcedure()
        {
            BeginProcedure(false);
        }

        public void BeginProcedure(bool hasQuery)
        {
            _hasQuery = hasQuery;
            _sb.AppendLine("\t(");
            if (_parameters.Count > 0)
            {
                var joined = string.Join("," + Environment.NewLine + "\t", _parameters.ToArray());
                _sb.Append("\t" + joined + Environment.NewLine);
            }
            _sb.AppendLine("\t)");
            _sb.AppendLine("\tSPECIFIC " + _specificName);
            _sb.AppendLine("\tLANGUAGE SQL");
            if (hasQuery)
            {
                _sb.AppendLine("\tDYNAMIC RESULT SETS 1");
            }
            _sb.AppendLine("BEGIN");
            if (hasQuery)
            {
                _sb.AppendLine("\tDECLARE c CURSOR WITH RETURN FOR");
            }
        }

        public string End()
        {
            _sb.AppendLine("");
            if(_hasQuery)
            {
                _sb.AppendLine("\tOPEN c;");
            }
            _sb.AppendLine("END~"); //we set the delimiter before
            return _sb.ToString();
        }

        public string Signature()
        {
            return null;
        }

        #endregion
    }
}
