using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HIC.Common.Validation.UIAttributes;

namespace HIC.Common.Validation.Constraints.Secondary
{
    public abstract class SecondaryConstraint : ISecondaryConstraint
    {
        public Consequence? Consequence { get; set; }

        [Description("Optional, Allows you to record why you have set this rule as a future reminder")]
        [ExpectsLotsOfText]
        public string Rationale { get; set; }

        public abstract void RenameColumn(string originalName, string newName);
        public abstract string GetHumanReadableDescriptionOfValidation();
        public abstract ValidationFailure Validate(object value, object[] otherColumns, string[] otherColumnNames);
        
    }
}
