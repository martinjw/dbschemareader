using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseSchemaReader.SqlGen.Oracle
{
    class ProcedureWriter : IProcedureWriter
    {
        private readonly StringBuilder _body = new StringBuilder();
        private readonly StringBuilder _sig = new StringBuilder();
        private readonly IList<string> _parameters = new List<string>();
        private string _procedureName;
        private string _cursorParameter = "result";

        public ProcedureWriter(string procName, string tableName)
        {
            Start(tableName, procName);
        }

        public string CursorParameterName
        {
            get
            {
                return _cursorParameter;
            }
            set
            {
                _cursorParameter = value;
            }
        }

        #region Procedure Boilerplate
        private void Start(string tableName, string procName)
        {
            _procedureName = procName;
            _body.AppendLine("-- " + procName + " - table " + tableName);
            _body.AppendLine("PROCEDURE " + procName);
            _sig.AppendLine("PROCEDURE " + procName);
        }

        public void BeginProcedure()
        {
            BeginProcedure(false);
        }
        public void BeginProcedure(bool hasQuery)
        {
            if (hasQuery) _parameters.Add(" " + CursorParameterName + " out T_CURSOR");
            _body.AppendLine(" (");
            _sig.AppendLine(" (");
            if (_parameters.Count > 0)
            {
                var joined = string.Join("," + Environment.NewLine + "  ", _parameters.ToArray());
                _sig.AppendLine("  " + joined);
                _body.AppendLine("  " + joined);
            }
            _sig.AppendLine(" );");
            _body.AppendLine(" ) IS");
            _body.AppendLine(" BEGIN");
            _body.AppendLine();
        }
        public void AddQuerySql(string sql)
        {
            _body.AppendLine("OPEN " + CursorParameterName + " FOR");
            AddSql(sql);
        }
        public void AddSql(string sql)
        {
            _body.AppendLine(sql + ";");
            _body.AppendLine();
        }

        public void AddParameter(string parameterName, string dataType)
        {
            _parameters.Add(" " + parameterName + " IN " + dataType);
        }
        public void AddIntegerParameter(string parameterName)
        {
            _parameters.Add(" " + parameterName + " IN NUMBER");
        }
        public void AddOutputParameter(string parameterName, string dataType)
        {
            _parameters.Add(" " + parameterName + " OUT " + dataType);
        }

        public string Signature()
        {
            return _sig.ToString();
        }

        public string End()
        {
            _body.AppendLine(" END " + _procedureName + ";");
            return _body.ToString();
        }
        #endregion
    }
}
