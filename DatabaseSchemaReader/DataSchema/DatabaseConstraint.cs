using System;
using System.Collections.Generic;

namespace DatabaseSchemaReader.DataSchema
{
    /// <summary>
    /// Represents a constraint (of <see cref="ConstraintType"/> such as primary key, foreign key...) that is attached to <see cref="Columns"/> of a table with name <see cref="TableName"/>
    /// </summary>
    [Serializable]
    public class DatabaseConstraint
    {
        public DatabaseConstraint()
        {
            Columns = new List<string>();
        }

        public string Name { get; set; }

        public string TableName { get; set; }

        public string RefersToConstraint { get; set; }

        public string RefersToTable { get; set; }

        public string DeleteRule { get; set; }

        public ConstraintType ConstraintType { get; set; }

        public List<string> Columns { get; set; }

        public string Expression { get; set; }

        public override string ToString()
        {
            return Name + " on " + TableName;
        }

    }
}
