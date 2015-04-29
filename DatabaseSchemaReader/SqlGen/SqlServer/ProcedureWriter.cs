using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DatabaseSchemaReader.SqlGen.SqlServer
{
    class ProcedureWriter : IProcedureWriter
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private readonly IList<string> _parameters = new List<string>();

        public ProcedureWriter(string procName, string tableName)
        {
            Start(tableName, procName, "dbo");
        }
        public ProcedureWriter(string procName, string tableName, string schema)
        {
            if (string.IsNullOrEmpty(schema)) schema = "dbo";
            Start(tableName, procName, schema);
        }

        #region Procedure Boilerplate
        private void Start(string tableName, string procName, string schema)
        {
            _sb.AppendLine("-- " + procName + " - table " + tableName);
            _sb.AppendLine("SET ANSI_NULLS ON");
            _sb.AppendLine("GO");
            _sb.AppendLine("SET QUOTED_IDENTIFIER ON");
            _sb.AppendLine("GO");
            _sb.AppendLine();
            _sb.AppendLine("IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[" + schema + "].[" + procName + "]') AND type in (N'P', N'PC'))");
            _sb.AppendLine("\tDROP PROCEDURE [" + schema + "].[" + procName + "]");
            _sb.AppendLine("GO");
            _sb.AppendLine();

            _sb.AppendLine("CREATE PROCEDURE [" + schema + "].[" + procName + "]");
        }

        public void BeginProcedure()
        {
            BeginProcedure(false);
        }
        public void BeginProcedure(bool hasQuery)
        {
            if (_parameters.Count > 0)
            {
                var joined = string.Join("," + Environment.NewLine + "\t", _parameters.ToArray());
                _sb.Append("\t" + joined + Environment.NewLine);
            }
            _sb.AppendLine("AS");
            _sb.AppendLine("BEGIN");
            _sb.AppendLine("\t-- SET NOCOUNT ON added to prevent extra result sets from");
            _sb.AppendLine("\t-- interfering with SELECT statements.");
            _sb.AppendLine("\tSET NOCOUNT ON;");
            _sb.AppendLine();
        }
        public void AddQuerySql(string sql)
        {
            AddSql(sql);
        }
        public void AddSql(string sql)
        {
            _sb.AppendLine(sql);
            _sb.AppendLine();
        }

        public void AddParameter(string parameterName, string dataType)
        {
            _parameters.Add("\t@" + parameterName + "\t" + dataType);
        }
        public void AddIntegerParameter(string parameterName)
        {
            _parameters.Add("\t@" + parameterName + "\t[int]");
        }
        public void AddOutputParameter(string parameterName, string dataType)
        {
            _parameters.Add("\t@" + parameterName + "\t" + dataType + " OUTPUT");
        }

        public string End()
        {
            _sb.AppendLine("END");
            _sb.AppendLine("GO");
            return _sb.ToString();
        }

        public string Signature()
        {
            return null;
        }
        #endregion
    }
}
