using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DatabaseSchemaReader.DataSchema;

namespace DatabaseSchemaViewer
{
    static class SchemaToTreeview
    {
        private const string RightClickToScript = "Right click to script";

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
            if (schema.Packages.Count > 0) FillPackages(treeRoot, schema);
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
            foreach (var storedProcedure in storedProcedures.OrderBy(x => x.SchemaOwner).ThenBy(x => x.Name))
            {
                var name = storedProcedure.Name;
                if (!string.IsNullOrEmpty(storedProcedure.SchemaOwner)) name = storedProcedure.SchemaOwner + "." + name;
                var node = new TreeNode(name);
                node.Tag = storedProcedure;
                root.Nodes.Add(node);
                FillArguments(node, storedProcedure.Arguments);
            }
        }


        private static void FillFunctions(TreeNode treeRoot, DatabaseSchema schema)
        {
            var root = new TreeNode("Functions");
            treeRoot.Nodes.Add(root);
            foreach (var function in schema.Functions.OrderBy(x => x.SchemaOwner).ThenBy(x => x.Name))
            {
                var name = function.Name;
                if (!string.IsNullOrEmpty(function.SchemaOwner)) name = function.SchemaOwner + "." + name;
                var node = new TreeNode(name);
                if (!string.IsNullOrEmpty(function.Sql)) node.Tag = function;
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
                if (!string.IsNullOrEmpty(package.Definition)) node.Tag = package;
                root.Nodes.Add(node);
                FillSprocs(node, package.StoredProcedures);
            }
        }

        private static void FillArguments(TreeNode node, IEnumerable<DatabaseArgument> arguments)
        {
            foreach (var argument in arguments)
            {
                var sb = new StringBuilder();
                var name = argument.Name;
                if (string.IsNullOrEmpty(name)) name = "?";
                sb.Append(name);
                sb.Append(" ");
                sb.Append(argument.DatabaseDataType);
                if (argument.DataType != null)
                {
                    if (argument.DataType.IsString)
                    {
                        sb.Append("(");
                        var length = argument.Length.GetValueOrDefault();
                        sb.Append(length != -1 ? length.ToString(CultureInfo.InvariantCulture) : "MAX");
                        sb.Append(")");
                    }
                    else if (argument.DataType.IsNumeric && !argument.DataType.IsInt)
                    {
                        sb.Append("(");
                        sb.Append(argument.Precision);
                        if (argument.Scale > 0)
                        {
                            sb.Append(",");
                            sb.Append(argument.Scale);
                        }
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
            foreach (var view in schema.Views.OrderBy(x => x.SchemaOwner).ThenBy(x => x.Name))
            {
                var name = view.Name;
                if (!string.IsNullOrEmpty(view.SchemaOwner)) name = view.SchemaOwner + "." + name;
                var viewNode = new TreeNode(name);
                viewNode.Tag = view;
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
            tableRoot.Tag = schema;
            tableRoot.ToolTipText = RightClickToScript;
            treeRoot.Nodes.Add(tableRoot);

            foreach (var table in schema.Tables.OrderBy(x => x.SchemaOwner).ThenBy(x => x.Name))
            {
                var name = table.Name;
                if (!string.IsNullOrEmpty(table.SchemaOwner)) name = table.SchemaOwner + "." + name;
                var tableNode = new TreeNode(name);
                tableNode.Tag = table;
                tableNode.ToolTipText = RightClickToScript;
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
            if (!table.Indexes.Any()) return;

            var indexRoot = new TreeNode("Indexes");
            tableNode.Nodes.Add(indexRoot);
            foreach (var index in table.Indexes)
            {
                var node = new TreeNode(index.Name);
                node.Tag = index;
                indexRoot.Nodes.Add(node);
                foreach (var column in index.Columns)
                {
                    var columnNode = new TreeNode(column.Name);
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
            foreach (var defaultConstraint in table.DefaultConstraints)
            {
                AddConstraint(constraintRoot, defaultConstraint);
            }
        }

        private static void AddConstraint(TreeNode constraintRoot, DatabaseConstraint constraint)
        {
            var node = new TreeNode(constraint.Name);
            node.Tag = constraint;
            node.ToolTipText = RightClickToScript;
            constraintRoot.Nodes.Add(node);
            if (constraint.ConstraintType == ConstraintType.Check)
            {
                node.ToolTipText = constraint.Expression;
            }
            else if (constraint.ConstraintType == ConstraintType.Default)
            {
                node.ToolTipText = constraint.Expression;
            }
            foreach (var column in constraint.Columns)
            {
                var columnNode = new TreeNode(column);
                node.Nodes.Add(columnNode);
            }
        }

        private static void FillTriggers(DatabaseTable table, TreeNode tableNode)
        {
            if (!table.Triggers.Any()) return;
            var triggerRoot = new TreeNode("Triggers");
            tableNode.Nodes.Add(triggerRoot);
            foreach (var trigger in table.Triggers)
            {
                var triggerNode = new TreeNode(trigger.Name);
                if (!string.IsNullOrEmpty(trigger.TriggerBody))
                    triggerNode.Tag = trigger;
                triggerRoot.Nodes.Add(triggerNode);
            }
        }

        private static void FillColumn(TreeNode tableNode, DatabaseColumn column)
        {
            var sb = new StringBuilder();
            sb.Append(column.Name);
            sb.Append(" ");

            sb.Append(column.DataTypeDefinition());

            if (!column.Nullable)
            {
                sb.Append(" NOT NULL");
            }
            if (column.IsPrimaryKey)
            {
                sb.Append(" PK");
            }
            if (column.IsAutoNumber)
            {
                //identity or autonumber
                if (column.IdentityDefinition != null)
                {
                    sb.Append(" Identity");
                    if (column.IdentityDefinition.IsNonTrivialIdentity())
                    {
                        sb.Append('(');
                        sb.Append(column.IdentityDefinition.IdentitySeed);
                        sb.Append(',');
                        sb.Append(column.IdentityDefinition.IdentityIncrement);
                        sb.Append(')');
                    }
                }
                else
                {
                    sb.Append(" AutoNumber");
                }
            }
            if (column.IsForeignKey)
            {
                sb.Append(" FK to " + column.ForeignKeyTableName);
            }
            var colNode = new TreeNode(sb.ToString());
            if (!(column.Table is DatabaseView))
            {
                colNode.Tag = column;
                colNode.ToolTipText = RightClickToScript;
            }
            tableNode.Nodes.Add(colNode);
        }
    }
}
