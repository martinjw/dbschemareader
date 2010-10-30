using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Library.Data.SqlGen;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class ProcedureWriter : IProcedureWriter
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly IList<string> _parameters = new List<string>();

        public ProcedureWriter(string procName, string tableName)
        {
            Start(tableName, procName);
        }

        #region Procedure Boilerplate
        private void Start(string tableName, string procName)
        {
            _sb.AppendLine("# " + procName + " - table " + tableName);
            _sb.AppendLine("DROP PROCEDURE IF EXISTS `" + procName + "`;");
            _sb.AppendLine();
            _sb.AppendLine("DELIMITER $$");
            _sb.AppendLine();

            _sb.AppendLine("CREATE PROCEDURE `" + procName + "`");
        }

        public void BeginProcedure()
        {
            BeginProcedure(false);
        }
        public void BeginProcedure(bool hasQuery)
        {
            _sb.AppendLine("\t(");
            if (_parameters.Count > 0)
            {
                var joined = string.Join("," + Environment.NewLine + "\t", _parameters.ToArray());
                _sb.Append("\t" + joined + Environment.NewLine);
            }
            _sb.AppendLine("\t)");
            _sb.AppendLine("BEGIN");
            _sb.AppendLine();
        }
        public void AddQuerySql(string sql)
        {
            AddSql(sql);
        }
        public void AddSql(string sql)
        {
            if (!sql.EndsWith(";", StringComparison.OrdinalIgnoreCase)) sql += ";";
            _sb.AppendLine(sql);
            _sb.AppendLine();
        }

        public void AddParameter(string parameterName, string dataType)
        {
            _parameters.Add("\t" + parameterName + "\t" + dataType);
        }
        public void AddIntegerParameter(string parameterName)
        {
            _parameters.Add("\t@" + parameterName + "\tint");
        }
        public void AddOutputParameter(string parameterName, string dataType)
        {
            _parameters.Add("\tOUT " + parameterName + "\t" + dataType);
        }

        public string End()
        {
            _sb.AppendLine("END$$");
            _sb.AppendLine("DELIMITER ;");
            return _sb.ToString();
        }

        public string Signature()
        {
            return null;
        }
        #endregion

    }
}
