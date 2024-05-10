using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Attributes;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Annotations;
using System.Collections.Generic;
using System.Data.Common;

namespace Rdmp.Core.Setting;

public class Setting : DatabaseEntity, ISetting
{
    #region Database Properties

    private string _key;
    private string _value;

    [NotNull]
    [Unique]
    public string Key
    {
        get => _key;
        set => SetField(ref _key, value);
    }

    [NotNull]
    public string Value
    {
        get => _value;
        set => SetField(ref _value, value);
    }

    #endregion

    public Setting(ICatalogueRepository repository, string key, string value)
    {
        Key = key;
        Value = value;
        Repository = repository;
        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { nameof(Key), key },
            { nameof(Value), value },
        });
    }

    public Setting(ICatalogueRepository repository, DbDataReader r): base(repository,r)
    {
        Key = r["Key"].ToString();
        Value = r["Value"].ToString();
    }
}