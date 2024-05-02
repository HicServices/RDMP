// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data.Common;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;

namespace Rdmp.Core.DataQualityEngine.Data;

/// <summary>
///     The DQE graphs in Dashboard show the states of datasets over time (every time the DQE is run - see Evaluation).
///     This table stores annotations that the user has added to specific
///     evaluations.  For example if a dataset has a huge hole in it then the data analyst might add an arrow pointing at
///     the hole saying 'Accidentally deleted this data, I'll replace it
///     next Thursday'.  The creation date of the annotation as well as the text and the location on the graph are stored
///     and rendered any time the user is reviewing that DQE graph.
/// </summary>
public class DQEGraphAnnotation : DatabaseEntity
{
    public double StartX { get; set; }
    public double StartY { get; set; }
    public double EndX { get; set; }
    public double EndY { get; set; }

    public string Text { get; set; }
    public int Evaluation_ID { get; set; }

    public string Username { get; set; }
    public DateTime CreationDate { get; set; }
    public DQEGraphType AnnotationIsForGraph { get; set; }

    public string PivotCategory { get; set; }

    public DQEGraphAnnotation()
    {
    }

    public DQEGraphAnnotation(DQERepository repository, double startX, double startY, double endX, double endY,
        string text, Evaluation evaluation, DQEGraphType annotationIsForGraphType, string pivotCategory)
    {
        Repository = repository;

        var username = Environment.UserName;

        Repository.InsertAndHydrate(this, new Dictionary<string, object>
        {
            { "StartX", startX },
            { "StartY", startY },
            { "EndX", endX },
            { "EndY", endY },
            { "Text", text },
            { "Evaluation_ID", evaluation.ID },
            { "Username", username },
            { "CreationDate", DateTime.Now },
            { "AnnotationIsForGraph", annotationIsForGraphType },
            { "PivotCategory", pivotCategory }
        });
    }

    public DQEGraphAnnotation(DQERepository repository, DbDataReader r) : base(repository, r)
    {
        Repository = repository;

        //coordinates of the annotation
        StartX = double.Parse(r["StartX"].ToString());
        StartY = double.Parse(r["StartY"].ToString());
        EndX = double.Parse(r["EndX"].ToString());
        EndY = double.Parse(r["EndY"].ToString());

        Text = r["Text"].ToString();
        Evaluation_ID = int.Parse(r["Evaluation_ID"].ToString());
        Username = r["Username"].ToString();
        CreationDate = (DateTime)r["CreationDate"];
        PivotCategory = (string)r["PivotCategory"];
        AnnotationIsForGraph = (DQEGraphType)Enum.Parse(typeof(DQEGraphType), r["AnnotationIsForGraph"].ToString());
    }
}

public enum DQEGraphType
{
    TimePeriodicityGraph
}