using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using DatabaseSchemaReader.DataSchema;
using DatabaseSchemaReader.ProviderSchemaReaders.ConnectionContext;

namespace DatabaseSchemaReader.ProviderSchemaReaders.Databases.PostgreSql
{
    class Constraints : SqlExecuter<DatabaseConstraint>
    {
        private readonly string _tableName;
        private readonly ConstraintType _constraintType;

        public Constraints(int? commandTimeout, string owner, string tableName, ConstraintType constraintType) : base(commandTimeout, owner)
        {
            _tableName = tableName;
            _constraintType = constraintType;
            Owner = owner;
            // #200 ora2pg (Oracle To PostgreSQL) tool does not fully support INFORMATION_SCHEMA, so we use the PostgreSQL system tables
            Sql = @"SELECT 
  con.conname AS constraint_name,
  nsp.nspname AS constraint_schema,
  rel.relname AS table_name,
  att.attname AS column_name,
  i.ordinality AS ordinal_position,

  -- If it is a FK, give the information of the referenced constraint
  CASE WHEN con.contype = 'f' THEN ref_con.conname ELSE NULL END AS unique_constraint_name,
  CASE WHEN con.contype = 'f' THEN ref_nsp.nspname ELSE NULL END AS fk_schema,
  CASE WHEN con.contype = 'f' THEN ref_rel.relname ELSE NULL END AS fk_table,
    
  CASE con.confdeltype::text
    WHEN 'a' THEN 'NO ACTION'
    WHEN 'r' THEN 'RESTRICT'
    WHEN 'c' THEN 'CASCADE'
    WHEN 'n' THEN 'SET NULL'
    WHEN 'd' THEN 'SET DEFAULT'
    ELSE con.confdeltype::text
  END AS delete_rule,
  CASE con.confupdtype::text
    WHEN 'a' THEN 'NO ACTION'
    WHEN 'r' THEN 'RESTRICT'
    WHEN 'c' THEN 'CASCADE'
    WHEN 'n' THEN 'SET NULL'
    WHEN 'd' THEN 'SET DEFAULT'
    ELSE con.confupdtype::text
  END AS update_rule
  
  --CASE WHEN con.contype = 'f' THEN ref_att.attname ELSE NULL END AS fk_column,
  --con.contype AS constraint_type,
  --con.oid as constraint_oid

FROM 
  pg_constraint con
  JOIN pg_class rel ON rel.oid = con.conrelid
  JOIN pg_namespace nsp ON nsp.oid = rel.relnamespace
  JOIN LATERAL unnest(con.conkey) WITH ORDINALITY AS i(attnum, ordinality) ON TRUE
  JOIN pg_attribute att ON att.attnum = i.attnum AND att.attrelid = rel.oid

  -- For FOREIGN KEY, attach the referenced constraint (UNIQUE/PK)
  LEFT JOIN pg_constraint ref_con 
    ON con.contype = 'f'
    AND ref_con.conrelid = con.confrelid
    AND ref_con.contype IN ('u', 'p')
    AND ref_con.conkey = con.confkey -- This direct join only works if the order of the columns and their number match

  LEFT JOIN pg_class ref_rel ON ref_rel.oid = ref_con.conrelid
  LEFT JOIN pg_namespace ref_nsp ON ref_nsp.oid = ref_rel.relnamespace
  LEFT JOIN LATERAL (
    SELECT att2.attname
    FROM unnest(con.confkey) WITH ORDINALITY AS c(attnum, ordinality)
    JOIN pg_attribute att2 ON att2.attnum = c.attnum AND att2.attrelid = con.confrelid
    WHERE c.ordinality = i.ordinality
  ) AS ref_att ON TRUE

WHERE 
  (nsp.nspname = :schemaOwner OR :schemaOwner IS NULL) AND 
  (
    (:constraint_type = 'PRIMARY KEY' AND con.contype = 'p') OR
    (:constraint_type = 'FOREIGN KEY' AND con.contype = 'f') OR
    (:constraint_type = 'UNIQUE' AND con.contype = 'u') OR
    (:constraint_type IS NULL AND con.contype IN ('p', 'u', 'f'))
  )
  AND rel.relname NOT LIKE 'pg_%'
  AND rel.relname NOT LIKE 'sql_%'
--ORDER BY nsp.nspname, rel.relname, con.conname, i.ordinality

Union

SELECT DISTINCT
cons.constraint_name, 
cons.constraint_schema,
keycolumns.table_name, 
column_name, 
ordinal_position, 
refs.unique_constraint_name, 
cons2.table_schema AS fk_schema,
cons2.table_name AS fk_table,
refs.delete_rule::text AS delete_rule,
refs.update_rule::text AS update_rule
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons
    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS keycolumns
        ON (cons.constraint_catalog = keycolumns.constraint_catalog
            OR cons.constraint_catalog IS NULL) AND
        cons.constraint_schema = keycolumns.constraint_schema AND
        cons.constraint_name = keycolumns.constraint_name AND
        cons.table_name = keycolumns.table_name
    LEFT OUTER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS refs
        ON (cons.constraint_catalog = refs.constraint_catalog
            OR cons.constraint_catalog IS NULL) AND
        cons.constraint_schema = refs.constraint_schema AND
        cons.constraint_name = refs.constraint_name
    LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons2
        ON (cons2.constraint_catalog = refs.constraint_catalog
            OR cons2.constraint_catalog IS NULL) AND
        cons2.constraint_name = refs.unique_constraint_name
WHERE 
    (keycolumns.table_name = :tableName OR :tableName IS NULL) AND 
    (cons.constraint_schema = :schemaOwner OR :schemaOwner IS NULL) AND 
    cons.constraint_type = :constraint_type

--ORDER BY cons.constraint_schema, keycolumns.table_name, cons.constraint_name,ordinal_position
Order by constraint_schema, table_name, constraint_name, ordinal_position";

//            Sql = @"SELECT DISTINCT
//cons.constraint_name, 
//cons.constraint_schema,
//keycolumns.table_name, 
//column_name, 
//ordinal_position, 
//refs.unique_constraint_name, 
//cons2.table_schema AS fk_schema,
//cons2.table_name AS fk_table,
//refs.delete_rule AS delete_rule,
//refs.update_rule AS update_rule
//FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons
//    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS keycolumns
//        ON (cons.constraint_catalog = keycolumns.constraint_catalog
//            OR cons.constraint_catalog IS NULL) AND
//        cons.constraint_schema = keycolumns.constraint_schema AND
//        cons.constraint_name = keycolumns.constraint_name AND
//        cons.table_name = keycolumns.table_name
//    LEFT OUTER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS refs
//        ON (cons.constraint_catalog = refs.constraint_catalog
//            OR cons.constraint_catalog IS NULL) AND
//        cons.constraint_schema = refs.constraint_schema AND
//        cons.constraint_name = refs.constraint_name
//    LEFT OUTER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS cons2
//        ON (cons2.constraint_catalog = refs.constraint_catalog
//            OR cons2.constraint_catalog IS NULL) AND
//        --cons2.constraint_schema = refs.constraint_schema AND
//        cons2.constraint_name = refs.unique_constraint_name
//WHERE 
//    (keycolumns.table_name = :tableName OR :tableName IS NULL) AND 
//    (cons.constraint_schema = :schemaOwner OR :schemaOwner IS NULL) AND 
//    cons.constraint_type = :constraint_type
//ORDER BY
//  cons.constraint_schema, keycolumns.table_name, cons.constraint_name,ordinal_position";

        }

        protected override void AddParameters(DbCommand command)
        {
            AddDbParameter(command, "schemaOwner", Owner);
            AddDbParameter(command, "tableName", _tableName);

            string constraintType;
            switch (_constraintType)
            {
                case ConstraintType.ForeignKey:
                    constraintType = "FOREIGN KEY";
                    break;
                case ConstraintType.UniqueKey:
                    constraintType = "UNIQUE";
                    break;
                default:
                    constraintType = "PRIMARY KEY";
                    break;
            }

            AddDbParameter(command, "constraint_type", constraintType);
        }

        private DatabaseConstraint FindConstraint(string name, string constraintTableName, string schemaName)
        {
            return Result.Find(f => f.Name == name && f.TableName == constraintTableName && f.SchemaOwner == schemaName);
        }

        protected override void Mapper(IDataRecord record)
        {
            var schema = record.GetString("constraint_schema");
            var tableName = record.GetString("table_name");
            var name = record.GetString("constraint_name");

            var constraint = FindConstraint(name, tableName, schema);
            if (constraint == null)
            {
                constraint = new DatabaseConstraint
                {
                    ConstraintType = _constraintType,
                    SchemaOwner = schema,
                    TableName = tableName,
                    Name = name,
                    RefersToConstraint = record.GetString("unique_constraint_name"),
                    RefersToTable = record.GetString("fk_table"),
                    RefersToSchema = record.GetString("fk_schema"),
                    DeleteRule = record.GetString("delete_rule"),
                    UpdateRule = record.GetString("update_rule"),
                };
                Result.Add(constraint);
            }
            var columnName = record.GetString("column_name");
            constraint.Columns.Add(columnName);
        }

        public IList<DatabaseConstraint> Execute(IConnectionAdapter connectionAdapter)
        {
            ExecuteDbReader(connectionAdapter);
            return Result;
        }
    }
}
