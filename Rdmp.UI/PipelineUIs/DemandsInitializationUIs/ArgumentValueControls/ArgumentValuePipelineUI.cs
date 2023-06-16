// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.Repositories;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.PipelineUIs.Pipelines;
using Rdmp.UI.PipelineUIs.Pipelines.PluginPipelineUsers;
using Rdmp.UI.SimpleDialogs;


namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
/// 
/// <para>This Control is for setting Properties that are Pipeline, (Requires the class to implement <see cref="IDemandToUseAPipeline"/>).</para>
/// </summary>
[TechnicalUI]
public partial class ArgumentValuePipelineUI : UserControl, IArgumentValueUI
{
    private IPipelineSelectionUI _pipelineSelectionUIInstance;
    private Type _typeOfUnderlyingClass;


    public ArgumentValuePipelineUI(ICatalogueRepository catalogueRepository, IArgumentHost parent, Type argumentType)
    {
        InitializeComponent();

        var typeName = parent.GetClassNameWhoArgumentsAreFor();

        _typeOfUnderlyingClass = catalogueRepository.MEF.GetType(typeName);

        if (_typeOfUnderlyingClass == null)
            throw new Exception($"Could not identify a Type called {typeName} in any loaded assemblies");
    }

    public void SetUp(IActivateItems activator, ArgumentValueUIArgs args)
    {
        var instanceOfParentType = Activator.CreateInstance(_typeOfUnderlyingClass);

        var factory =
            new PipelineSelectionUIFactory(args.CatalogueRepository, args.Required, args, instanceOfParentType);
        _pipelineSelectionUIInstance = factory.Create(activator);
        _pipelineSelectionUIInstance.CollapseToSingleLineMode();

        var c = (Control)_pipelineSelectionUIInstance;
        Controls.Add(c);

        try
        {
            try
            {
                _ = (Pipeline)args.InitialValue;
            }
            catch (Exception e)
            {
                ExceptionViewer.Show(e);
            }
        }
        catch (Exception e)
        {
            args.Fatal(e);
        }
    }
}