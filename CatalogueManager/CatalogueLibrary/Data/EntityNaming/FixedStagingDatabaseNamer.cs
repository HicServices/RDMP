using CatalogueLibrary.DataHelper;

namespace CatalogueLibrary.Data.EntityNaming
{
    /// <summary>
    /// Used when there is a single staging database used for multiple different Catalogues. The name of the database being loaded is prepended to the staging table name.
    /// </summary>
    public class FixedStagingDatabaseNamer : SuffixBasedNamer
    {
        private readonly string _stagingDatabaseName;
        private readonly string _databaseName;

        /// <summary>
        /// <para>---</para>
        /// <para>For 'Staging', returns the table name prefixed with <paramref name="databaseName"/> and suffixed with _STAGING</para>
        /// <para>---</para>
        /// <para>For others, appends:</para>
        /// <para>_Archive for Archive</para>
        /// </summary>
        public FixedStagingDatabaseNamer(string databaseName, string stagingDatabaseName = "DLE_STAGING")
        {
            _databaseName = RDMPQuerySyntaxHelper.EnsureValueIsNotWrapped(databaseName);
            _stagingDatabaseName = RDMPQuerySyntaxHelper.EnsureValueIsNotWrapped(stagingDatabaseName);
        }

        public override string GetName(string tableName, LoadBubble convention)
        {
            if (convention == LoadBubble.Staging)
                return _databaseName + "_" + tableName + Suffixes[convention];

            return base.GetName(tableName, convention);
        }
        
        public override string GetDatabaseName(string rootDatabaseName, LoadBubble stage)
        {
            if (stage == LoadBubble.Staging)
                return _stagingDatabaseName;

            return base.GetDatabaseName(rootDatabaseName, stage);
        }
    }
}