// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs;


namespace Rdmp.UI.PipelineUIs.DemandsInitializationUIs.ArgumentValueControls;

/// <summary>
/// Allows you to specify the value of an IArugment (the database persistence value of a [DemandsInitialization] decorated Property on a MEF class e.g. a Pipeline components public property that the user can set)
/// 
/// <para>This Control is for setting Properties that are of Type derived from ICustomUIDrivenClass and require a specific plugin user interface to be displayed in order to let the user edit
/// the value he wants (e.g. configure a web service endpoint with many properties that should be serialised / configured through a specific UI you have written).</para>
/// </summary>
[TechnicalUI]
public partial class ArgumentValueCustomUIDrivenClassUI : UserControl, IArgumentValueUI
{
    private Type _uiType;
    private ArgumentValueUIArgs _args;

    public ArgumentValueCustomUIDrivenClassUI()
    {
        InitializeComponent();
    }

    public void SetUp(IActivateItems activator, ArgumentValueUIArgs args)
    {
        _args = args;

        try
        {
            var t = _args.Type;

            var expectedUIClassName = $"{t.FullName}UI";

            _uiType = Core.Repositories.MEF.GetType(expectedUIClassName);

            //if we did not find one with the exact name (including namespace), try getting it just by the end of its name (omit namespace)
            if (_uiType == null)
            {
                var shortUIClassName = $"{t.Name}UI";
                var candidates = Core.Repositories.MEF.GetAllTypes().Where(type => type.Name.Equals(shortUIClassName))
                    .ToArray();

                _uiType = candidates.Length switch
                {
                    > 1 => throw new Exception(
                        $"Found {candidates.Length} classes called '{shortUIClassName}' : ({string.Join(",", candidates.Select(c => c.Name))})"),
                    0 => throw new Exception(
                        $"Could not find UI class called {shortUIClassName} make sure that it exists, is public and is marked with class attribute "),
                    _ => candidates[0]
                };
            }


            btnLaunchCustomUI.Text = $"Launch Custom UI ({_uiType.Name})";
            btnLaunchCustomUI.Width = btnLaunchCustomUI.PreferredSize.Width;
        }
        catch (Exception)
        {
            btnLaunchCustomUI.Enabled = false;
        }
    }

    private void btnLaunchCustomUI_Click(object sender, EventArgs e)
    {
        try
        {
            var dataClassInstance = (ICustomUIDrivenClass)_args.InitialValue;

            var uiInstance = Activator.CreateInstance(_uiType);

            var instanceAsCustomUI = (ICustomUI)uiInstance;
            instanceAsCustomUI.CatalogueRepository = _args.CatalogueRepository;

            instanceAsCustomUI.SetGenericUnderlyingObjectTo(dataClassInstance);
            var dr = ((Form)instanceAsCustomUI).ShowDialog();

            if (dr != DialogResult.Cancel)
            {
                var result = instanceAsCustomUI.GetFinalStateOfUnderlyingObject();
                _args.Setter(result);
                _args.InitialValue = result;
            }
        }
        catch (Exception ex)
        {
            ExceptionViewer.Show(ex);
        }
    }
}