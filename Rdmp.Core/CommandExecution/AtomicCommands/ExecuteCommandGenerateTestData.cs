// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Linq;
using SynthEHR;
using SynthEHR.Datasets;

namespace Rdmp.Core.CommandExecution.AtomicCommands;

/// <summary>
/// Generates CSV files on disk for RDMP example datasets (based on SynthEHR library)
/// </summary>
public class ExecuteCommandGenerateTestData : BasicCommandExecution
{
    private readonly string _datasetName;
    private readonly int _numberOfPeople;
    private readonly int _numberOfRecords;
    private readonly string _toFile;
    private Random _r;
    private IDataGenerator _generator;

    public ExecuteCommandGenerateTestData(IBasicActivateItems activator, string datasetName, int numberOfPeople,
        int numberOfRecords, int seed, string toFile) : base(activator)
    {
        _datasetName = datasetName;
        _numberOfPeople = numberOfPeople;
        _numberOfRecords = numberOfRecords;
        _toFile = toFile;

        var match = DataGeneratorFactory.GetAvailableGenerators().Cast<DataGeneratorFactory.GeneratorType?>().FirstOrDefault(g =>
            g?.Type.FullName?.Contains(datasetName, StringComparison.InvariantCultureIgnoreCase) == true);

        if (match is null)
        {
            SetImpossible(
                $"Unknown dataset '{datasetName}'.  Known datasets are:{Environment.NewLine}{string.Join(Environment.NewLine, DataGeneratorFactory.GetAvailableGenerators().Select(static g => g.Type.FullName).ToArray())}");
            return;
        }

        _r = new Random(seed);
        _generator = DataGeneratorFactory.Create(match.Value.Type, _r);
    }

    public override void Execute()
    {
        base.Execute();

        var people = new PersonCollection();
        people.GeneratePeople(_numberOfPeople, _r);

        var f = new FileInfo(_toFile);

        if (!f.Directory.Exists)
            f.Directory.Create();

        _generator.GenerateTestDataFile(people, f, _numberOfRecords);
    }
}