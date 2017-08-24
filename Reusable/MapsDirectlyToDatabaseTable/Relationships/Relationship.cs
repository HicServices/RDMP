using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapsDirectlyToDatabaseTable.Relationships
{
    public class Relationship
    {
        public string Parent { get; set; }
        public string Child { get; set; }
        public CascadeType DeleteAction { get; set; }
        public CascadeType UpdateAction { get; set; }
        public string ChildForeignKeyField { get; set; }

    }

    public enum CascadeType
    {
        CASCADE,
        NO_ACTION,
        SET_NULL
        //Resonance Cascade?
    }
}

