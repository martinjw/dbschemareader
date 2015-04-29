using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseSchemaReader.SqlGen.MySql
{
    class ProcedureWriter : IProcedureWriter
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly IList<string> _parameters = new List<string>();
        private string _returnType;

        public ProcedureWriter(string procName, string tableName)
        {
            Start(procName, tableName, "PROCEDURE");
        }
        /// <summary>
        /// Special constructor for functions
        /// </summary>
        /// <param name="procName">Name of the procedure.</param>
        /// <param name="isFunction">if set to <c>true</c> if is function, rather than procedure.</param>
        public ProcedureWriter(string procName, bool isFunction)
        {
            Start(procName, null, isFunction ? "FUNCTION" : "PROCEDURE");
        }

        #region Procedure Boilerplate
        private void Start(string procName, string tableName, string type)
        {
            _sb.Append("# " + procName);
            if(!string.IsNullOrEmpty(tableName))
            {
                _sb.Append(" - table " + tableName);
            }
            _sb.AppendLine();
            _sb.AppendLine("DROP " + type + " IF EXISTS `" + procName + "`;");
            _sb.AppendLine();
            _sb.AppendLine("DELIMITER $$");
            _sb.AppendLine();

            _sb.AppendLine("CREATE " + type + " `" + procName + "`");
        }

        public void BeginProcedure()
        {
            BeginProcedure(false);
        }
        public void BeginProcedure(bool hasQuery)
        {
            _sb.AppendLine("\t(");
            //write the parameters
            if (_parameters.Count > 0)
            {
                var joined = string.Join("," + Environment.NewLine + "\t", _parameters.ToArray());
                _sb.Append("\t" + joined + Environment.NewLine);
            }
            _sb.AppendLine("\t)");
            //function return type
            if (!string.IsNullOrEmpty(_returnType))
            {
                _sb.AppendLine("RETURNS " + _returnType);
            }
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
            _parameters.Add("\tIN " + parameterName + "\t" + dataType);
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

        public void AddReturns(string returnType)
        {
            _returnType = returnType;
        }
    }
}
