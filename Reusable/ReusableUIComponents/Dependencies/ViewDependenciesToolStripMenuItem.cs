using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableLibraryCode;
using ReusableUIComponents.Dependencies.Models;

namespace ReusableUIComponents.Dependencies
{
    public class ViewDependenciesToolStripMenuItem : ToolStripMenuItem
    {
        private readonly IHasDependencies _root;
        private readonly Type[] _allowFilterOnTypes;
        private readonly IObjectVisualisation _visualiser;
        private readonly List<Type> _initialGraphTypes;

        public ViewDependenciesToolStripMenuItem(IHasDependencies root, IObjectVisualisation visualiser, List<Type> initialGraphTypes = null )
            : base("View Dependencies", Images.ViewDependencies)
        {
            _root = root;

            //Find all other IHasDependencies objects in the assembly and let the user filter on them
            _allowFilterOnTypes = root.GetType().Assembly.GetTypes()
                    .Where(t => typeof(IHasDependencies).IsAssignableFrom(t) && !(t.IsInterface || t.IsAbstract))
                    .ToArray();

            _visualiser = visualiser;
            _initialGraphTypes = initialGraphTypes;
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            
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
    }
}
