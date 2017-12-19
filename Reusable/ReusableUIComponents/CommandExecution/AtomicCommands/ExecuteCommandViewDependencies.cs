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
        private readonly IObjectVisualisation _visualiser;
        private readonly List<Type> _initialGraphTypes;

        public ExecuteCommandViewDependencies(IHasDependencies hasDependenciesOrNull, IObjectVisualisation visualiser, List<Type> initialGraphTypes = null)
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

            DependencyGraph g = new DependencyGraph(_allowFilterOnTypes, _visualiser);
            if (_initialGraphTypes != null)
                g.ShowTypeList(_initialGraphTypes);
            else
                g.ShowTypeListAll();

            g.Dock = DockStyle.Fill;
            Form f = new Form();
            f.Text = "Object Visualisation Graph: " + _root;
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
