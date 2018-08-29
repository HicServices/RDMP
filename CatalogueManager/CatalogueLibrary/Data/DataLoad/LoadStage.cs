namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Describes a stage related to the RAW=>STAGING=>LIVE bubble data load model in RDMP.  The stages are more refined than LoadBubbles (RAW / STAGING / LIVE) and
    /// include such things as GetFiles (where you download remote resources ahead of the actual load etc).
    /// </summary>
    public enum LoadStage
    {
        /// <summary>
        /// Processes in this category should result in the generation or modification of files (e.g.
        ///  FTP file download, unzip local file etc).  The data load engine will not provide processes 
        /// in this stage with any information about the database being loaded (but it will provide 
        /// the root project directory so that processes know where to generate files into)"
        /// </summary>
        GetFiles,

        /// <summary>
        /// Processes in this category should be concerned with moving data from the project directory
        /// into the RAW database.  The data load engine will provide both the root directory and the 
        /// location of the RAW database.
        /// </summary>
        Mounting,

        /// <summary>
        /// Processes in this category should be concerned with modifying the content/structure of the data
        /// in the RAW database.  This data will not be annonymised at this point.  After running all the
        /// processes in this category, the structure of the database must match the _STAGING database.
        /// Assuming the RAW database structure matches _STAGING, the data load engine will then move the data
        /// (performing appropriate anonymisation steps on a column by column basis as defined in the Catalogue
        ///  ColumnInfos)
        /// </summary>
        AdjustRaw,

        /// <summary>
        /// "Processes in this category should be concerned with modifying the content (not structure) of the data in the 
        /// STAGING database.  This data will be annonymous.  After all processes have been executed and assuming the _STAGING 
        /// database structure still matches the LIVE structure, the data load engine will use the primary key informtion
        /// defined in the Catalogue ColumnInfos to merge the new data into the current LIVE database
        /// </summary>
        AdjustStaging,
        
        /// <summary>
        /// "Processes in this category are executed after the new data has been merged into the LIVE database.  This 
        /// is your opportunity to update dependent data, run longitudinal/dataset wide cleaning algorithms etc."
        /// </summary>
        PostLoad
    }
}