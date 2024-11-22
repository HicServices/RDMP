// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.Curation.Data.Serialization;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Annotations;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// A virtual column that is made available to researchers.  Each Catalogue has 1 or more CatalogueItems, these contain the descriptions of what is contained
/// in the column as well as any outstanding/resolved issues.
/// 
/// <para>It is important to note that <see cref="CatalogueItem"/> are tied to underlying database via <see cref="ExtractionInformation"/>.  This means
/// that you can for example have multiple different versions of the same underlying <see cref="ColumnInfo"/> </para>
/// 
/// <para>e.g.
/// CatalogueItem: PatientDateOfBirth (ExtractionInformation verbatim but 'Special Approval Required')
/// CatalogueItem: PatientDateOfBirthApprox (ExtractionInformation rounds to nearest quarter but governance is 'Core')</para>
/// 
/// <para>Both the above would extract from the same ColumnInfo DateOfBirth</para>
/// </summary>
public class CatalogueItem : DatabaseEntity, IComparable, IHasDependencies, INamed,
    IInjectKnown<ExtractionInformation>, IInjectKnown<ColumnInfo>, IInjectKnown<Catalogue>
{
    #region Database Properties

    private string _Name;
    private string _Statistical_cons;
    private string _Research_relevance;
    private string _Description;
    private string _Topic;
    private string _Agg_method;
    private string _Limitations;
    private string _Comments;
    private int _catalogueID;
    private int? _columnInfoID;
    private Catalogue.CataloguePeriodicity _periodicity;

    private Lazy<ExtractionInformation> _knownExtractionInformation;
    private Lazy<ColumnInfo> _knownColumnInfo;
    private Lazy<Catalogue> _knownCatalogue;

    /// <summary>
    /// The ID of the parent <see cref="Catalogue"/> (dataset) to which this is a virtual column/column description
    /// </summary>
    [Relationship(typeof(Catalogue), RelationshipType.SharedObject)]
    [DoNotExtractProperty]
    public int Catalogue_ID
    {
        get => _catalogueID;
        set
        {
            SetField(ref _catalogueID, value);
            ClearAllInjections();
        }
    }

    /// <inheritdoc/>
    [NotNull]
    [DoNotImportDescriptions]
    public string Name
    {
        get => _Name;
        set => SetField(ref _Name, value);
    }

    /// <summary>
    /// User supplied field meant to identify any statistical anomalies with the data in the column described.  Not used for anything by RDMP.
    /// </summary>
    public string Statistical_cons
    {
        get => _Statistical_cons;
        set => SetField(ref _Statistical_cons, value);
    }

    /// <summary>
    /// User supplied field meant for describing research applicability/field of the data in the column.  Not used for anything by RDMP.
    /// </summary>
    public string Research_relevance
    {
        get => _Research_relevance;
        set => SetField(ref _Research_relevance, value);
    }


    /// <summary>
    /// User supplied description of what is in the column
    /// </summary>
    [UsefulProperty]
    public string Description
    {
        get => _Description;
        set => SetField(ref _Description, value);
    }

    /// <summary>
    /// User supplied heading or keywords of what is in the column relates to.  Not used for anything by RDMP.
    /// </summary>
    public string Topic
    {
        get => _Topic;
        set => SetField(ref _Topic, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Agg_method
    {
        get => _Agg_method;
        set => SetField(ref _Agg_method, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Limitations
    {
        get => _Limitations;
        set => SetField(ref _Limitations, value);
    }

    /// <summary>
    /// User specified free text field.  Not used for anything by RDMP.
    /// </summary>
    public string Comments
    {
        get => _Comments;
        set => SetField(ref _Comments, value);
    }

    /// <summary>
    /// The ID of the underlying <see cref="ColumnInfo"/> to which this CatalogueItem describes.  This can be null if the underlying column has been deleted / removed.
    /// You can have multiple <see cref="CatalogueItem"/>s in a <see cref="Catalogue"/> that share the same underlying <see cref="ColumnInfo"/> if one of them is a transform
    /// e.g. you might release the first 3 digits of a postcode to anyone (<see cref="ExtractionCategory.Core"/>) but only release the full postcode with
    /// <see cref="ExtractionCategory.SpecialApprovalRequired"/>.
    /// </summary>
    [Relationship(typeof(ColumnInfo),
        RelationshipType.IgnoreableLocalReference)] //will appear as empty, then the user can guess from a table
    public int? ColumnInfo_ID
    {
        get => _columnInfoID;
        set
        {
            //don't change it to the same value it already has
            if (value == ColumnInfo_ID)
                return;

            SetField(ref _columnInfoID, value);
            ClearAllInjections();
        }
    }

    /// <summary>
    /// How frequently this column is updated... why this would be different from <see cref="Data.Catalogue.Periodicity"/>?
    /// </summary>
    public Catalogue.CataloguePeriodicity Periodicity
    {
        get => _periodicity;
        set => SetField(ref _periodicity, value);
    }

    #endregion


    #region Relationships

    /// <inheritdoc cref="Catalogue_ID"/>
    [NoMappingToDatabase]
    public Catalogue Catalogue => _knownCatalogue.Value;

    /// <summary>
    /// Fetches the <see cref="ExtractionInformation"/> (if any) that specifies how to extract this column.  This can be the underlying column name (fully specified) or a transform.
    /// <para>This will be null if the <see cref="CatalogueItem"/> is not extractable</para>
    /// </summary>
    [NoMappingToDatabase]
    public ExtractionInformation ExtractionInformation => _knownExtractionInformation.Value;

    /// <inheritdoc cref="ColumnInfo_ID"/>
    [NoMappingToDatabase]
    public ColumnInfo ColumnInfo => _knownColumnInfo.Value;

    internal bool IsColumnInfoCached() => _knownColumnInfo.IsValueCreated;

    #endregion

    /// <summary>
    /// Name of the parent <see cref="Catalogue"/>.  This property value will be cached once fetched for a given object thanks to Lazy/IInjectKnown&lt;Catalogue&gt;"/>.
    /// </summary>
    [UsefulProperty]
    [NoMappingToDatabase]
    [DoNotExtractProperty]
    public string CatalogueName => Catalogue.Name;

    public CatalogueItem()
    {
        ClearAllInjections();
    }

    /// <summary>
    /// Creates a new virtual column description for for a column in the dataset (<paramref name="parent"/>) supplied with the given Name.
    /// <para><remarks>You should next choose which <see cref="ColumnInfo"/> powers it and optionally create an <see cref="ExtractionInformation"/> to
    /// make the column extractable</remarks></para>
    /// </summary>
    public CatalogueItem(ICatalogueRepository repository, ICatalogue parent, string name)
    {
        repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "Name", name },
            { "Catalogue_ID", parent.ID }
        });

        ClearAllInjections();
        parent.ClearAllInjections();
    }

    internal CatalogueItem(ICatalogueRepository repository, DbDataReader r)
        : base(repository, r)
    {
        Catalogue_ID =
            int.Parse(r["Catalogue_ID"]
                .ToString()); //gets around decimals and other random crud number field types that sql returns
        Name = (string)r["Name"];
        Statistical_cons = r["Statistical_cons"].ToString();
        Research_relevance = r["Research_relevance"].ToString();
        Description = r["Description"].ToString();
        Topic = r["Topic"].ToString();
        Agg_method = r["Agg_method"].ToString();
        Limitations = r["Limitations"].ToString();
        Comments = r["Comments"].ToString();
        ColumnInfo_ID = ObjectToNullableInt(r["ColumnInfo_ID"]);

        //Periodicity - with handling for invalid enum values listed in database
        var periodicity = r["Periodicity"];
        if (periodicity == null || periodicity == DBNull.Value)
            Periodicity = Catalogue.CataloguePeriodicity.Unknown;
        else
        {
            if(Enum.TryParse(periodicity.ToString(), true, out Catalogue.CataloguePeriodicity periodicityAsEnum))
                Periodicity = periodicityAsEnum;
            else
                Periodicity = Catalogue.CataloguePeriodicity.Unknown;
        }

        ClearAllInjections();
    }

    internal CatalogueItem(ShareManager shareManager, ShareDefinition shareDefinition)
    {
        shareManager.UpsertAndHydrate(this, shareDefinition);
    }

    /// <inheritdoc/>
    public void InjectKnown(Catalogue instance)
    {
        _knownCatalogue = new Lazy<Catalogue>(instance);
    }

    /// <inheritdoc/>
    public void ClearAllInjections()
    {
        _knownColumnInfo = new Lazy<ColumnInfo>(FetchColumnInfoIfAny);
        _knownExtractionInformation = new Lazy<ExtractionInformation>(FetchExtractionInformationIfAny);
        _knownCatalogue = new Lazy<Catalogue>(FetchCatalogue);
    }

    private Catalogue FetchCatalogue() => Repository.GetObjectByID<Catalogue>(Catalogue_ID);

    private ExtractionInformation FetchExtractionInformationIfAny() =>
        Repository.GetAllObjectsWithParent<ExtractionInformation>(this).SingleOrDefault();

    private ColumnInfo FetchColumnInfoIfAny() =>
        !ColumnInfo_ID.HasValue ? null : Repository.GetObjectByID<ColumnInfo>(ColumnInfo_ID.Value);

    /// <inheritdoc/>
    public void InjectKnown(ExtractionInformation instance)
    {
        _knownExtractionInformation = new Lazy<ExtractionInformation>(instance);
    }

    /// <inheritdoc/>
    public void InjectKnown(ColumnInfo instance)
    {
        _knownColumnInfo = new Lazy<ColumnInfo>(instance);
    }

    /// <inheritdoc/>
    public override string ToString() => Name;

    /// <summary>
    /// Sorts alphabetically by <see cref="Name"/>
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public int CompareTo(object obj)
    {
        if (obj is CatalogueItem)
            return -string.Compare(obj.ToString(), ToString(),
                StringComparison.CurrentCulture); //sort alphabetically (reverse)

        throw new Exception($"Cannot compare {GetType().Name} to {obj.GetType().Name}");
    }

    /// <summary>
    /// Copies the descriptive metadata from one <see cref="CatalogueItem"/> (this) into a new <see cref="CatalogueItem"/> in the supplied <paramref name="cataToImportTo"/>
    /// </summary>
    /// <param name="cataToImportTo">The <see cref="Catalogue"/> to import into (cannot be the current <see cref="CatalogueItem"/> parent)</param>
    /// <returns></returns>
    public CatalogueItem CloneCatalogueItemWithIDIntoCatalogue(Catalogue cataToImportTo)
    {
        if (Catalogue_ID == cataToImportTo.ID)
            throw new ArgumentException(
                "Cannot clone a CatalogueItem into its own parent, specify a different catalogue to clone into");

        var clone = new CatalogueItem((ICatalogueRepository)cataToImportTo.Repository, cataToImportTo, Name);

        //Get all the properties
        var propertyInfo =
            GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //Assign all source property to taget object 's properties
        foreach (var property in propertyInfo)
            //Check whether property can be written to
            if (property.CanWrite && !property.Name.Equals("ID") && !property.Name.Equals("Catalogue_ID"))
                if (property.PropertyType.IsValueType || property.PropertyType.IsEnum ||
                    property.PropertyType == typeof(string))
                    property.SetValue(clone, property.GetValue(this, null), null);

        clone.SaveToDatabase();

        return clone;
    }

    /// <summary>
    /// Guesses which <see cref="ColumnInfo"/> from a collection is probably the right one for underlying this <see cref="CatalogueItem"/>.  This is done
    /// by looking for a <see cref="ColumnInfo"/> whose name is the same as the <see cref="CatalogueItem.Name"/> if not then it gets more flexible looking
    /// for .Contains etc.
    /// </summary>
    /// <param name="guessPool"></param>
    /// <param name="allowPartial">Set to false to avoid last-resort match based on String.Contains</param>
    /// <returns></returns>
    public IEnumerable<ColumnInfo> GuessAssociatedColumn(ColumnInfo[] guessPool, bool allowPartial = true)
    {
        //exact matches exist so return those
        var guess = guessPool.Where(col => col.GetRuntimeName()?.Equals(Name) == true).ToArray();
        if (guess.Length != 0)
            return guess;

        //ignore caps match instead
        guess = guessPool.Where(col => col.GetRuntimeName()?.Equals(Name, StringComparison.OrdinalIgnoreCase) == true).ToArray();
        if (guess.Length != 0)
            return guess;

        //ignore caps and remove spaces match instead
        guess = guessPool.Where(col =>
            col.GetRuntimeName()?.Replace(" ", "").Equals(Name.Replace(" ", ""), StringComparison.OrdinalIgnoreCase) == true).ToArray();
        if (guess.Length != 0)
            return guess;

        if (allowPartial)
            //contains match is final last resort
            return guessPool.Where(col =>
                col.GetRuntimeName()?.Contains(Name, StringComparison.OrdinalIgnoreCase) == true
                ||
                Name.Contains(col.GetRuntimeName() ?? "(dummy for null)", StringComparison.OrdinalIgnoreCase));

        return Array.Empty<ColumnInfo>();
    }

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsThisDependsOn() => null;

    /// <inheritdoc/>
    public IHasDependencies[] GetObjectsDependingOnThis()
    {
        var dependantObjects = new List<IHasDependencies>();

        var exInfo = ExtractionInformation;

        if (exInfo != null)
            dependantObjects.Add(exInfo);

        if (ColumnInfo_ID != null)
            dependantObjects.Add(ColumnInfo);

        dependantObjects.Add(Catalogue);
        return dependantObjects.ToArray();
    }

    /// <summary>
    /// Changes the CatalogueItem in the database to be based off of the specified ColumnInfo (or none if null is specified).  This will
    /// likely result in the ExtractionInformation being corrupt / out of sync in terms of the SQL appearing in its
    /// <see cref="IColumn.SelectSQL"/>.
    /// </summary>
    /// <param name="columnInfo"></param>
    public void SetColumnInfo(ColumnInfo columnInfo)
    {
        ColumnInfo_ID = columnInfo?.ID;
        SaveToDatabase();
        InjectKnown(columnInfo);
    }

    public CatalogueItem ShallowClone(ICatalogue into)
    {
        var clone = new CatalogueItem(CatalogueRepository, into, Name);
        CopyShallowValuesTo(clone);
        return clone;
    }

    public override string GetSummary(bool includeName, bool includeID)
    {
        var sb = new StringBuilder();

        foreach (var prop in GetType().GetProperties().Where(static p => p.Name.Contains("Description")))
            AppendPropertyToSummary(sb, prop, includeName, includeID, false);

        sb.AppendLine(SUMMARY_LINE_DIVIDER);

        sb.AppendLine($"Extractable: {FormatForSummary(ExtractionInformation != null)}");
        sb.AppendLine($"Transforms Data: {FormatForSummary(ExtractionInformation?.IsProperTransform() ?? false)}");
        sb.AppendLine($"Category: {ExtractionInformation?.ExtractionCategory ?? (object)"Not Extractable"}");

        foreach (var prop in GetType().GetProperties().Where(static p => !p.Name.Contains("Description")))
            AppendPropertyToSummary(sb, prop, includeName, includeID);

        return sb.ToString();
    }
}