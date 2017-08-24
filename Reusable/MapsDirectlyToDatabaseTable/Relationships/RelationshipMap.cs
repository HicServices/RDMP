using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace MapsDirectlyToDatabaseTable.Relationships
{
    public class RelationshipMap
    {
        private readonly SqlConnectionStringBuilder _builder;
        public Relationship[] Relationships { get; private set; }

        private const string FindRelationshipsSQL = @"--Get dependencies
SELECT
    o1.name AS FK_table,
    c1.name AS FK_column,
    fk.name AS FK_name,
    o2.name AS PK_table,
    c2.name AS PK_column,
    pk.name AS PK_name,
    fk.delete_referential_action_desc AS Delete_Action,
    fk.update_referential_action_desc AS Update_Action
FROM sys.objects o1
    INNER JOIN sys.foreign_keys fk
        ON o1.object_id = fk.parent_object_id
    INNER JOIN sys.foreign_key_columns fkc
        ON fk.object_id = fkc.constraint_object_id
    INNER JOIN sys.columns c1
        ON fkc.parent_object_id = c1.object_id
        AND fkc.parent_column_id = c1.column_id
    INNER JOIN sys.columns c2
        ON fkc.referenced_object_id = c2.object_id
        AND fkc.referenced_column_id = c2.column_id
    INNER JOIN sys.objects o2
        ON fk.referenced_object_id = o2.object_id
    INNER JOIN sys.key_constraints pk
        ON fk.referenced_object_id = pk.parent_object_id
        AND fk.key_index_id = pk.unique_index_id
ORDER BY o1.name, o2.name, fkc.constraint_column_id";


        /// <summary>
        /// Connects to the database and builds an array of referential relationships found by querying sys tables.  This class will ONLY record relationships where the
        /// Primary Key is called ID, this allows for the IMapsDirectlyToDatabaseTable dependant object finding.  A Parent is the primary key table (e.g. Family) and the
        /// Child is the foreign key table (e.g. FamilyMembers).
        /// </summary>
        /// <param name="builder"></param>
        public RelationshipMap(SqlConnectionStringBuilder builder)
        {
            _builder = builder;

            Refresh();
        }


        public void Refresh()
        {
            SqlConnection con = new SqlConnection(_builder.ConnectionString);
            con.Open();

            SqlCommand cmd = new SqlCommand(FindRelationshipsSQL,con);
            var r = cmd.ExecuteReader();
            
            List<Relationship> relationships = new List<Relationship>();


            while (r.Read())
            {
                //we are only interested in dependencies by ID
                if(!r["PK_column"].Equals("ID"))
                    continue;

                relationships.Add(new Relationship()
                {
                    Child = r["FK_table"].ToString(),
                    Parent = r["PK_table"].ToString(),
                    ChildForeignKeyField = r["FK_column"].ToString(),
                    DeleteAction = (CascadeType) Enum.Parse(typeof(CascadeType),r["Delete_Action"].ToString()),
                    UpdateAction = (CascadeType)Enum.Parse(typeof(CascadeType), r["Update_Action"].ToString())
                });
            }

            r.Close();

            Relationships = relationships.ToArray();


            con.Close();
        }

        public Relationship[] GetRelationships(string rootTable, bool cascadeOnly)
        {
            var allObjects = new List<Relationship>();
            GetDependentTablesRecursively(rootTable, allObjects, cascadeOnly);

            return allObjects.ToArray();
        }

        private void GetDependentTablesRecursively(string parent, List<Relationship> objectsFound, bool cascadeOnly)
        {
            //all immediate children
            Relationship[] childRelationships = Relationships
                .Where(
                    r => r.Parent.Equals(parent)//where it is a parent
                &&
                (!cascadeOnly  //and user doesn't care if it is a cascade 
                    ||
                    r.DeleteAction == CascadeType.CASCADE))   //or it is a cascade
                .ToArray();

            foreach (Relationship relation in childRelationships)
            {
                if (relation.Child.Equals(parent))
                    continue;//self reference

                //if we have not encountered this object already
                if (!objectsFound.Contains(relation))
                {
                    //we have now encountered it 
                    if(!cascadeOnly)
                        objectsFound.Add(relation);  //IMPORTANT if we are not looking solely at CASCADES then we can have looped relationships resulting in stack overflow so we have to record the find before we tree off
                    
                    //get the dependents
                    GetDependentTablesRecursively(relation.Child, objectsFound, cascadeOnly);

                    if (cascadeOnly)
                        objectsFound.Add(relation);  //IMPORTANT if cascade only then there CANNOT be a circular dependency (database prevents it) which means we can tree then ADD resulting in an List that is in order of least dependency
                    
                }
            }
        }
    }
}
