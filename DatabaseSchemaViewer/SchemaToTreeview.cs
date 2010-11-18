using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    static class SchemaToTreeview
    {

        public static void PopulateTreeView(DatabaseSchema schema, TreeView treeView1)
        {
            // Suppress repainting the TreeView until all the objects have been created.
            treeView1.BeginUpdate();

            treeView1.Nodes.Clear(); //clear out anything that exists
            treeView1.ShowNodeToolTips = true;

            var treeRoot = new TreeNode("Schema");
            treeView1.Nodes.Add(treeRoot);

            FillTables(treeRoot, schema);
            FillViews(treeRoot, schema);
            FillSprocs(treeRoot, schema.StoredProcedures);
            FillFunctions(treeRoot, schema);
            if(schema.Packages.Count > 0) FillPackages(treeRoot, schema);
            FillUsers(treeRoot, schema);

            treeView1.EndUpdate();
        }

        private static void FillUsers(TreeNode treeRoot, DatabaseSchema schema)
        {
            var root = new TreeNode("Users");
            treeRoot.Nodes.Add(root);
            foreach (var user in schema.Users)
            {
                var node = new TreeNode(user.Name);
                root.Nodes.Add(node);
            }
        }

        private static void FillSprocs(TreeNode treeRoot, IEnumerable<DatabaseStoredProcedure> storedProcedures)
        {
            var root = new TreeNode("Stored Procedures");
            treeRoot.Nodes.Add(root);
            foreach (var storedProcedure in storedProcedures)
            {
                var node = new TreeNode(storedProcedure.Name);
                node.ToolTipText = storedProcedure.Sql;
                root.Nodes.Add(node);
                FillArguments(node, storedProcedure.Arguments);
            }
        }


        private static void FillFunctions(TreeNode treeRoot, DatabaseSchema schema)
        {
            var root = new TreeNode("Functions");
            treeRoot.Nodes.Add(root);
            foreach (var function in schema.Functions)
            {
                var node = new TreeNode(function.Name);
                node.ToolTipText = function.Sql;
                root.Nodes.Add(node);
                FillArguments(node, function.Arguments);
            }
        }


        private static void FillPackages(TreeNode treeRoot, DatabaseSchema schema)
        {
            var root = new TreeNode("Packages");
            treeRoot.Nodes.Add(root);
            foreach (var package in schema.Packages)
            {
                var node = new TreeNode(package.Name);
                node.ToolTipText = package.Definition;
                root.Nodes.Add(node);
                FillSprocs(node, package.StoredProcedures);
            }
        }

        private static void FillArguments(TreeNode node, IEnumerable<DatabaseArgument> arguments)
        {
            foreach (var argument in arguments)
            {
                var sb = new StringBuilder();
                sb.Append(argument.Name);
                sb.Append(" ");
                sb.Append(argument.DatabaseDataType);
                if (argument.DataType != null)
                {
                    if (argument.DataType.IsString)
                    {
                        sb.Append("(");
                        sb.Append(argument.Length);
                        sb.Append(")");
                    }
                    else if (argument.DataType.IsNumeric)
                    {
                        sb.Append("(");
                        sb.Append(argument.Precision);
                        sb.Append(",");
                        sb.Append(argument.Scale);
                        sb.Append(")");
                    }
                }
                sb.Append(" ");
                if (argument.In) sb.Append("IN");
                if (argument.Out) sb.Append("OUT");
                var argNode = new TreeNode(sb.ToString());
                node.Nodes.Add(argNode);
            }
        }

        private static void FillViews(TreeNode treeRoot, DatabaseSchema schema)
        {
            var viewRoot = new TreeNode("Views");
            treeRoot.Nodes.Add(viewRoot);
            foreach (var view in schema.Views)
            {
                var viewNode = new TreeNode(view.Name);
                viewNode.ToolTipText = view.Sql;
                viewRoot.Nodes.Add(viewNode);
                foreach (var column in view.Columns)
                {
                    FillColumn(viewNode, column);
                }
            }
        }

        private static void FillTables(TreeNode treeRoot, DatabaseSchema schema)
        {
            var tableRoot = new TreeNode("Tables");
            treeRoot.Nodes.Add(tableRoot);

            foreach (var table in schema.Tables)
            {
                var tableNode = new TreeNode(table.Name);
                tableRoot.Nodes.Add(tableNode);
                foreach (var column in table.Columns)
                {
                    FillColumn(tableNode, column);
                }
                FillConstraints(table, tableNode);
                FillTriggers(table, tableNode);
                FillIndexes(table, tableNode);
            }
        }

        private static void FillIndexes(DatabaseTable table, TreeNode tableNode)
        {
            var indexRoot = new TreeNode("Indexes");
            tableNode.Nodes.Add(indexRoot);
            foreach (var index in table.Indexes)
            {
                var node = new TreeNode(index.Name);
                indexRoot.Nodes.Add(node);
                foreach (var column in index.Columns)
                {
                    var columnNode = new TreeNode(column.Value);
                    node.Nodes.Add(columnNode);
                }
            }
        }

        private static void FillConstraints(DatabaseTable table, TreeNode tableNode)
        {
            var constraintRoot = new TreeNode("Constraints");
            tableNode.Nodes.Add(constraintRoot);
            if (table.PrimaryKey != null)
            {
                AddConstraint(constraintRoot, table.PrimaryKey);
            }
            foreach (var foreignKey in table.ForeignKeys)
            {
                AddConstraint(constraintRoot, foreignKey);
            }
            foreach (var uniqueKey in table.UniqueKeys)
            {
                AddConstraint(constraintRoot, uniqueKey);
            }
            foreach (var checkConstraint in table.CheckConstraints)
            {
                AddConstraint(constraintRoot, checkConstraint);
            }
        }

        private static void AddConstraint(TreeNode constraintRoot, DatabaseConstraint constraint)
        {
            var node = new TreeNode(constraint.Name);
            constraintRoot.Nodes.Add(node);
            if (constraint.ConstraintType == ConstraintType.Check)
            {
                constraintRoot.ToolTipText = constraint.Expression;
            }
            foreach (var column in constraint.Columns)
            {
                var columnNode = new TreeNode(column);
                node.Nodes.Add(columnNode);
            }
        }

        private static void FillTriggers(DatabaseTable table, TreeNode tableNode)
        {
            if (table.Triggers.Count > 0)
            {
                var triggerRoot = new TreeNode("Triggers");
                tableNode.Nodes.Add(triggerRoot);
                foreach (var trigger in table.Triggers)
                {
                    var triggerNode = new TreeNode(trigger.Name);
                    triggerNode.ToolTipText = trigger.TriggerBody;
                    triggerRoot.Nodes.Add(triggerNode);
                }
            }
        }

        private static void FillColumn(TreeNode tableNode, DatabaseColumn column)
        {
            var sb = new StringBuilder();
            sb.Append(column.Name);
            sb.Append(" ");
            sb.Append(column.DbDataType);
            if (column.DataType.IsString)
            {
                sb.Append("(");
                sb.Append(column.Length);
                sb.Append(")");
            }
            else if (column.DataType.IsNumeric)
            {
                sb.Append("(");
                sb.Append(column.Precision);
                sb.Append(",");
                sb.Append(column.Scale);
                sb.Append(")");
            }
            if (column.IsPrimaryKey)
            {
                sb.Append(" PK");
            }
            if (column.IsIdentity)
            {
                sb.Append(" Identity");
            }
            if (column.IsForeignKey)
            {
                sb.Append(" FK to " + column.ForeignKeyTableName);
            }
            var colNode = new TreeNode(sb.ToString());
            tableNode.Nodes.Add(colNode);
        }
    }
}
