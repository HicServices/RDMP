using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.CommandExecution;

namespace CatalogueLibrary.CommandExecution
{
    public class ExecuteCommandCreateLookup : BasicCommandExecution
    {
        private readonly ICatalogueRepository _catalogueRepository;
        private readonly ExtractionInformation _foreignKeyExtractionInformation;
        private readonly ColumnInfo[] _lookupDescriptionColumns;
        private readonly List<Tuple<ColumnInfo, ColumnInfo>> _fkToPkTuples;
        private readonly string _collation;
        private readonly bool _alsoCreateExtractionInformations;
        private readonly Catalogue _catalogue;
        private readonly ExtractionInformation[] _allExtractionInformations;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="catalogueRepository"></param>
        /// <param name="foreignKeyExtractionInformation">The extractable column in the main dataset which contains the lookup code foreign key e.g. PatientSexCode </param>
        /// <param name="lookupDescriptionColumns">The column(s) in the lookup that contain the free text description of the code e.g. SexDescription, SexDescriptionLong etc</param>
        /// <param name="fkToPkTuples">Must have at least 1, Item1 must be the foreign key column in the main dataset, Item2 must be the primary key column in the lookup. 
        ///     <para>MOST lookups have 1 column paring only e.g. genderCode but some crazy lookups have duplication of code with another column e.g. TestCode+Healthboard as primary keys into lookup</para></param>
        /// <param name="collation"></param>
        /// <param name="alsoCreateExtractionInformations"></param>
        public ExecuteCommandCreateLookup(ICatalogueRepository catalogueRepository, ExtractionInformation foreignKeyExtractionInformation, ColumnInfo[] lookupDescriptionColumns, List<Tuple<ColumnInfo, ColumnInfo>> fkToPkTuples, string collation, bool alsoCreateExtractionInformations)
        {
            _catalogueRepository = catalogueRepository;
            _foreignKeyExtractionInformation = foreignKeyExtractionInformation;
            _lookupDescriptionColumns = lookupDescriptionColumns;
            _fkToPkTuples = fkToPkTuples;
            _collation = collation;
            _alsoCreateExtractionInformations = alsoCreateExtractionInformations;

            _catalogue = _foreignKeyExtractionInformation.CatalogueItem.Catalogue;
            _allExtractionInformations = _catalogue.GetAllExtractionInformation(ExtractionCategory.Any);
            if(!_fkToPkTuples.Any())
                throw new Exception("You must pass at least 1 pair of keys");

        }

        public ExecuteCommandCreateLookup(ICatalogueRepository catalogueRepository, ExtractionInformation foreignKeyExtractionInformation, ColumnInfo lookupDescriptionColumn, ColumnInfo mainDatasetForeignKey, ColumnInfo lookupTablePrimaryKey, string collation, bool alsoCreateExtractionInformations)
            :this(catalogueRepository, foreignKeyExtractionInformation, new []{lookupDescriptionColumn},new List<Tuple<ColumnInfo, ColumnInfo>> {Tuple.Create(mainDatasetForeignKey,lookupTablePrimaryKey)},collation, alsoCreateExtractionInformations)
        {
            
        }

        public override void Execute()
        {
            base.Execute();
            
            foreach (var descCol in _lookupDescriptionColumns)
            {
                Lookup lookup = new Lookup(_catalogueRepository, descCol, _fkToPkTuples.First().Item1, _fkToPkTuples.First().Item2, ExtractionJoinType.Left, _collation);

                foreach (var supplementalKeyPair in _fkToPkTuples.Skip(1))
                    new LookupCompositeJoinInfo(_catalogueRepository, lookup, supplementalKeyPair.Item1, supplementalKeyPair.Item2, _collation);

                string proposedName;

                if(_lookupDescriptionColumns.Length == 1)
                    proposedName = _foreignKeyExtractionInformation.GetRuntimeName() + "_Desc";
                else
                    proposedName = _foreignKeyExtractionInformation.GetRuntimeName() + "_" + descCol.GetRuntimeName();
                
                var newCatalogueItem = new CatalogueItem(_catalogueRepository, _catalogue, proposedName);
                newCatalogueItem.SetColumnInfo(descCol);

                if (_alsoCreateExtractionInformations)
                {
                    //bump everyone down 1
                    foreach (var toBumpDown in _allExtractionInformations.Where(e => e.Order > _foreignKeyExtractionInformation.Order))
                    {
                        toBumpDown.Order++;
                        toBumpDown.SaveToDatabase();
                    }
                            
                    var newExtractionInformation = new ExtractionInformation(_catalogueRepository, newCatalogueItem, descCol, descCol.ToString());
                    newExtractionInformation.ExtractionCategory = ExtractionCategory.Supplemental;
                    newExtractionInformation.Alias = newCatalogueItem.Name;
                    newExtractionInformation.Order = _foreignKeyExtractionInformation.Order + 1;
                    newExtractionInformation.SaveToDatabase();
                }
            }
        }
    }
}