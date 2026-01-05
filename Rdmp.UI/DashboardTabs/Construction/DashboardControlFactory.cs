// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.DashboardTabs.Construction.Exceptions;
using Rdmp.UI.ItemActivation;

namespace Rdmp.UI.DashboardTabs.Construction;

public class DashboardControlFactory
{
    private readonly IActivateItems _activator;
    private readonly Point _startLocationForNewControls;

    public DashboardControlFactory(IActivateItems activator, Point startLocationForNewControls)
    {
        _activator = activator;
        _startLocationForNewControls = startLocationForNewControls;
    }

    public Type[] GetAvailableControlTypes() => Core.Repositories.MEF.GetAllTypes().Where(IsCompatibleType).ToArray();

    private bool IsCompatibleType(Type arg) =>
        typeof(IDashboardableControl).IsAssignableFrom(arg)
        &&
        typeof(UserControl).IsAssignableFrom(arg);

    /// <summary>
    /// Creates an instance of the user control described by the database record DashboardControl, including providing the control with a hydrated IPersistableObjectCollection that reflects
    /// the last saved state of the control.  Then mounts it on a DashboardableControlHostPanel and returns it
    /// </summary>
    /// <param name="toCreate"></param>
    /// <returns></returns>
    public DashboardableControlHostPanel Create(DashboardControl toCreate)
    {
        var controlType = Core.Repositories.MEF.GetType(toCreate.ControlType);

        var instance = CreateControl(controlType);

        return Hydrate((IDashboardableControl)instance, toCreate);
    }

    /// <summary>
    /// Creates a new instance of Type t (which must be an IDashboardableControl derived ultimately from UserControl) which is then hydrated with an empty collection and a database
    /// record is created which can be used to save its collection state for the lifetime of the control (allowing you to restore the state later)
    /// </summary>
    /// <param name="forLayout"></param>
    /// <param name="t"></param>
    /// <param name="theControlCreated"></param>
    /// <returns></returns>
    public DashboardControl Create(DashboardLayout forLayout, Type t,
        out DashboardableControlHostPanel theControlCreated)
    {
        var instance = CreateControl(t);

        //get the default size requirements of the control as it exists post construction
        var w = instance.Width;
        var h = instance.Height;

        var dbRecord = new DashboardControl(_activator.RepositoryLocator.CatalogueRepository, forLayout, t,
            _startLocationForNewControls.X, _startLocationForNewControls.Y, w, h, "");
        theControlCreated = Hydrate((IDashboardableControl)instance, dbRecord);

        return dbRecord;
    }

    private DashboardableControlHostPanel Hydrate(IDashboardableControl theControlCreated, DashboardControl dbRecord)
    {
        var emptyCollection = theControlCreated.ConstructEmptyCollection(dbRecord);

        foreach (var objectUse in dbRecord.ObjectsUsed)
        {
            var o = _activator.RepositoryLocator.GetArbitraryDatabaseObject(objectUse.ReferencedObjectRepositoryType,
                objectUse.ReferencedObjectType, objectUse.ReferencedObjectID);
            emptyCollection.DatabaseObjects.Add(o);
        }

        try
        {
            emptyCollection.LoadExtraText(dbRecord.PersistenceString);
        }
        catch (Exception e)
        {
            throw new DashboardControlHydrationException(
                $"Could not resolve extra text persistence string for control '{theControlCreated.GetType()}'", e);
        }

        theControlCreated.SetCollection(_activator, emptyCollection);

        var host = new DashboardableControlHostPanel(_activator, dbRecord, theControlCreated)
        {
            Location = new Point(dbRecord.X, dbRecord.Y),
            Width = dbRecord.Width,
            Height = dbRecord.Height
        };

        return host;
    }

    private UserControl CreateControl(Type t)
    {
        if (!IsCompatibleType(t))
            throw new ArgumentException($"Type '{t}' is not a compatible Type", nameof(t));

        var instance = (UserControl)ObjectConstructor.Construct(t);

        instance.Dock = DockStyle.None;
        instance.Anchor = AnchorStyles.Top | AnchorStyles.Left;

        return instance;
    }
}