// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ReusableLibraryCode;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;
using ReusableUIComponents.Dependencies;
using ReusableUIComponents.Dependencies.Models;

namespace ReusableUIComponents.CommandExecution.AtomicCommands
{
    public class ExecuteCommandViewDependencies : BasicCommandExecution,IAtomicCommand
    {
        private readonly IHasDependencies _root;
        private readonly Type[] _allowFilterOnTypes;
        private readonly Lazy<IObjectVisualisation> _visualiser;
        private readonly List<Type> _initialGraphTypes;

        public ExecuteCommandViewDependencies(IHasDependencies hasDependenciesOrNull, Lazy<IObjectVisualisation> visualiser, List<Type> initialGraphTypes = null)
        {
            if (hasDependenciesOrNull == null)
            {
                SetImpossible("Object is not IHasDependencies");
                return;
            }

            _root = hasDependenciesOrNull;

            //Find all other IHasDependencies objects in the assembly and let the user filter on them
            _allowFilterOnTypes = _root.GetType().Assembly.GetTypes()
                    .Where(t => typeof(IHasDependencies).IsAssignableFrom(t) && !(t.IsInterface || t.IsAbstract))
                    .ToArray();

            _visualiser = visualiser;
            _initialGraphTypes = initialGraphTypes;
        }

        public override void Execute()
        {
            base.Execute();

            DependencyGraphUI g = new DependencyGraphUI(_allowFilterOnTypes, _visualiser.Value);
            if (_initialGraphTypes != null)
                g.ShowTypeList(_initialGraphTypes);
            else
                g.ShowTypeListAll();

            g.Dock = DockStyle.Fill;
            Form f = new Form();
            f.Text = "Dependencies of " + _root;
            f.WindowState = FormWindowState.Maximized;
            f.Controls.Add(g);
            f.Show();

            g.GraphDependenciesOf(_root);
        }

        public Image GetImage(IIconProvider iconProvider)
        {
            return Images.ViewDependencies;
        }
    }
}
