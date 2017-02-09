using JCommon;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>A column in the database</summary>
    public partial class DatabaseColumn
    {
        private string _columnNamePinYin;

        private string _displayName;

        private string _entityName;

        /// <summary>显示名称</summary>
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(_displayName))
                {
                    _displayName = Name;
                }
                return _displayName;
            }
            set
            {
                _displayName = value;
            }
        }

        /// <summary>拼音</summary>
        public string ColumnNamePinYin
        {
            get
            {
                if (string.IsNullOrEmpty(_columnNamePinYin))
                {
                    _columnNamePinYin = JPinYin.Get(Name);
                }
                return _columnNamePinYin;
            }
            set
            {
                _columnNamePinYin = value;
            }
        }

        /// <summary>实体名称</summary>
        public string EntityName
        {
            get
            {
                if (_entityName.IsNullOrEmptyStr())
                {
                    _entityName = Name;
                }
                return _entityName;
            }
            set
            {
                _entityName = value;
            }
        }

        /// <summary>C# 类型,读取配置文件匹配</summary>
        public string CSharpType { get; set; }
    }
}