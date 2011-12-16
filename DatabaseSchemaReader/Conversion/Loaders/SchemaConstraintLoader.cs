using System;
using System.Collections.Generic;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaReader.Conversion.Loaders
{
    /// <summary>
    /// Loads and converts the dataTable (wrapping the Converter). Hides all/byTable logic.
    /// </summary>
    class SchemaConstraintLoader
    {
        private readonly SchemaExtendedReader _sr;
        private readonly SchemaConstraintConverter _pkConverter;
        private readonly SchemaConstraintConverter _fkConverter;
        private readonly ForeignKeyColumnConverter _fkColumnConverter;
        private readonly SchemaConstraintConverter _ukConverter;
        private readonly SchemaConstraintConverter _ckConverter;

        private readonly bool _noPks;
        private readonly bool _noFks;

        public SchemaConstraintLoader(SchemaExtendedReader schemaReader)
        {
            _sr = schemaReader;
            var pks = _sr.PrimaryKeys(null);
            _noPks = (pks.Rows.Count == 0);
            if (!_noPks)
            {
                _pkConverter = new SchemaConstraintConverter(pks, ConstraintType.PrimaryKey);
            }
            var fks = _sr.ForeignKeys(null);
            _noFks = (fks.Rows.Count == 0);
            if (!_noFks)
            {
                _fkConverter = new SchemaConstraintConverter(fks, ConstraintType.ForeignKey);
            }
            //foreign key columns
            var fkcols = _sr.ForeignKeyColumns(null);
            _fkColumnConverter = new ForeignKeyColumnConverter(fkcols);

            var uks = _sr.UniqueKeys(null);
            _ukConverter = new SchemaConstraintConverter(uks, ConstraintType.UniqueKey);
            var cks = _sr.CheckConstraints(null);
            _ckConverter = new SchemaConstraintConverter(cks, ConstraintType.Check);
        }

        private IList<DatabaseConstraint> PrimaryKeys(string tableName)
        {
            if (!_noPks)
            {
                //we have preloaded
                return _pkConverter.Constraints(tableName);
            }
            var constraints = _sr.PrimaryKeys(tableName);
            var converter = new SchemaConstraintConverter(constraints, ConstraintType.PrimaryKey);
            return converter.Constraints();
        }


        private IList<DatabaseConstraint> ForeignKeys(string tableName)
        {
            IList<DatabaseConstraint> fks;
            if (!_noFks)
            {
                //we have preloaded
                fks = _fkConverter.Constraints(tableName);
                _fkColumnConverter.AddForeignKeyColumns(fks);
                return fks;
            }
            var constraints = _sr.ForeignKeys(tableName);
            var converter = new SchemaConstraintConverter(constraints, ConstraintType.ForeignKey);
            fks = converter.Constraints();
            var cols = _sr.ForeignKeyColumns(tableName);
            var colConverter = new ForeignKeyColumnConverter(cols);
            colConverter.AddForeignKeyColumns(fks);
            return fks;
        }

        public IList<DatabaseConstraint> Load(string tableName, ConstraintType constraintType)
        {
            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName", "must have tableName");

            switch (constraintType)
            {
                case ConstraintType.PrimaryKey:
                    return PrimaryKeys(tableName);

                case ConstraintType.ForeignKey:
                    return ForeignKeys(tableName);

                case ConstraintType.UniqueKey:
                    return _ukConverter.Constraints(tableName);

                case ConstraintType.Check:
                    return _ckConverter.Constraints(tableName);

                default:
                    throw new ArgumentOutOfRangeException("constraintType");
            }
        }

    }
}
