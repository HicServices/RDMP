// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;

namespace Rdmp.Core.DataQualityEngine.Data;

/// <summary>
/// Root object for a DQE run including the time the DQE engine was run, the <see cref="Catalogue"/> being evaluated and all the results.
/// An <see cref="Evaluation"/> is immutable and created created after each successful run.
/// </summary>
public class Evaluation : DatabaseEntity
{
    public DateTime DateOfEvaluation { get; private set; }
    public int CatalogueID { get; set; }

    [NoMappingToDatabase] public ICatalogue Catalogue { get; private set; }

    private RowState[] rowStates;

    [NoMappingToDatabase]
    public RowState[] RowStates
    {
        get
        {
            if (rowStates == null)
                LoadRowAndColumnStates();

            return rowStates;
        }

        set => rowStates = value;
    }


    private ColumnState[] columnStates;

    [NoMappingToDatabase]
    public ColumnState[] ColumnStates
    {
        get
        {
            if (columnStates == null)
                LoadRowAndColumnStates();

            return columnStates;
        }

        set => columnStates = value;
    }

    [NoMappingToDatabase] public DQERepository DQERepository { get; set; }

    public Evaluation()
    {
    }

    public IEnumerable<DQEGraphAnnotation> GetAllDQEGraphAnnotations(string pivotCategory = null)
    {
        return DQERepository.GetAllObjects<DQEGraphAnnotation>()
            .Where(a => a.Evaluation_ID == ID && a.PivotCategory.Equals(pivotCategory ?? "ALL"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="r"></param>
    internal Evaluation(DQERepository repository, DbDataReader r) : base(repository, r)
    {
        DQERepository = repository;

        DateOfEvaluation = DateTime.Parse(r["DateOfEvaluation"].ToString());
        CatalogueID = int.Parse(r["CatalogueID"].ToString());

        try
        {
            Catalogue = DQERepository.CatalogueRepository.GetObjectByID<Catalogue>(CatalogueID);
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Could not create a DataQualityEngine.Evaluation for Evaluation with ID {ID} because it is a report of an old Catalogue that has been deleted or otherwise does not exist/could not be retrieved (CatalogueID was:{CatalogueID}).  See inner exception for full details",
                e);
        }
    }

    /// <summary>
    /// Starts a new evaluation with the given transaction
    /// </summary>
    internal Evaluation(DQERepository dqeRepository, ICatalogue c)
    {
        DQERepository = dqeRepository;
        Catalogue = c;

        dqeRepository.InsertAndHydrate(this,
            new Dictionary<string, object>
            {
                { "CatalogueID", c.ID },
                { "DateOfEvaluation", DateTime.Now }
            });
    }


    internal void AddRowState(int dataLoadRunID, int correct, int missing, int wrong, int invalid, string validatorXml,
        string pivotCategory, DbConnection con, DbTransaction transaction)
    {
        new RowState(this, dataLoadRunID, correct, missing, wrong, invalid, validatorXml, pivotCategory, con,
            transaction);
    }

    public string[] GetPivotCategoryValues()
    {
        var toReturn = new List<string>();
        var sql = $"select distinct PivotCategory From RowState where Evaluation_ID  = {ID}";

        using (var con = DQERepository.GetConnection())
        {
            using var cmd = DatabaseCommandHelper.GetCommand(sql, con.Connection, con.Transaction);
            using var r = cmd.ExecuteReader();
            while (r.Read())
                toReturn.Add((string)r["PivotCategory"]);
        }

        return toReturn.ToArray();
    }

    public override void DeleteInDatabase()
    {
        var affectedRows = DQERepository.Delete($"DELETE FROM Evaluation where ID = {ID}");

        if (affectedRows == 0)
            throw new Exception($"Delete statement resulted in {affectedRows} affected rows");
    }

    /// <summary>
    /// Returns the count of records in the dataset when this DQE evaluation was made.  This is done by summing the first <see cref="ColumnStates"/>
    /// </summary>
    /// <returns></returns>
    public int? GetRecordCount()
    {
        var state = ColumnStates?.FirstOrDefault();

        return state == null
            ? null
            : state.CountCorrect + state.CountMissing + state.CountWrong + state.CountInvalidatesRow;
    }

    private void LoadRowAndColumnStates()
    {
        var states = new List<RowState>();
        if (Repository is not TableRepository repo)
            throw new Exception(
                $"Repository was not a {nameof(TableRepository)}.  Evaluation class requires a database back repository to fetch RowStates/ColumnStates.  Repository was of Type '{Repository.GetType().Name}'");

        using var con = repo.GetConnection();
        //get all the row level data
        using (var cmdGetRowStates = DatabaseCommandHelper.GetCommand(
                   $"select * from RowState WHERE Evaluation_ID = {ID}",
                   con.Connection, con.Transaction))
        {
            using var r2 = cmdGetRowStates.ExecuteReader();
            while (r2.Read())
                states.Add(new RowState(r2));
            rowStates = states.ToArray();
            r2.Close();
        }


        //get all the column level data
        using (var cmdGetColumnStates = DatabaseCommandHelper.GetCommand(
                   $"select * from ColumnState WHERE ColumnState.Evaluation_ID = {ID}",
                   con.Connection, con.Transaction))
        {
            using var r2 = cmdGetColumnStates.ExecuteReader();
            var colStates = new List<ColumnState>();

            while (r2.Read())
                colStates.Add(new ColumnState(r2));

            columnStates = colStates.ToArray();
            r2.Close();
        }
    }
}