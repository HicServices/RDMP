// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.UI;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.Checks;

using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence
{
    /// <summary>
    /// A Document Tab that hosts an RDMPCollection, the control knows how to save itself to the persistence settings file for the user ensuring that when they next open the
    /// software the Tab can be reloaded and displayed.  Persistance involves storing this Tab type, the Collection Control type being hosted by the Tab (an RDMPCollection).
    /// Since there can only ever be one RDMPCollection of any Type active at a time this is all that must be stored to persist the control
    /// </summary>
    [TechnicalUI]
    [System.ComponentModel.DesignerCategory("")]
    public class PersistableToolboxDockContent:DockContent
    {
        public const string Prefix = "Toolbox";

        public readonly RDMPCollection CollectionType;

        PersistStringHelper persistStringHelper = new PersistStringHelper();

        public PersistableToolboxDockContent(RDMPCollection collectionType)
        {
            CollectionType = collectionType;
        }

        protected override string GetPersistString()
        {
            var ui = Controls.OfType<RDMPCollectionUI>().Single();

            var pin = ui.CommonTreeFunctionality.CurrentlyPinned as IMapsDirectlyToDatabaseTable;

            var args = new Dictionary<string, string>();
            args.Add("Toolbox",CollectionType.ToString());
            
            if(pin != null)
                args.Add("Pin",persistStringHelper.GetObjectCollectionPersistString(pin));

            return Prefix + PersistStringHelper.Separator + persistStringHelper.SaveDictionaryToString(args);
        }

        public void LoadPersistString(IActivateItems activator, string persistString)
        {
            try
            {
                var s = persistString.Substring(Prefix.Length + 1);
                var pinValue = persistStringHelper.GetValueIfExistsFromPersistString("Pin", s);

                if (pinValue != null)
                {
                    var toPin = persistStringHelper.GetObjectCollectionFromPersistString(pinValue, activator.RepositoryLocator).SingleOrDefault();

                    if(toPin != null)
                        activator.RequestItemEmphasis(this,new EmphasiseRequest(toPin){Pin = true,ExpansionDepth = 2});
                }
            }
            catch (Exception e)
            {
                activator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs("Failed to LoadPersistString '" + persistString + "' for collection " + CollectionType, CheckResult.Fail, e));
            }
        }

        public RDMPCollectionUI GetCollection()
        {
            return Controls.OfType<RDMPCollectionUI>().SingleOrDefault();
        }

        public static RDMPCollection? GetToolboxFromPersistString(string persistString)
        {
            var helper = new PersistStringHelper();
            var s = persistString.Substring(PersistableToolboxDockContent.Prefix.Length + 1);

            var args = helper.LoadDictionaryFromString(s);

            RDMPCollection collection;

            if (args.ContainsKey("Toolbox"))
            {
                Enum.TryParse(args["Toolbox"], true, out collection);
                return collection;
            }

            return null;
        }
    }
}
