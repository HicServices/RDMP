using System;
using System.Linq;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.AtomicCommands.UIFactory;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.SimpleDialogs.NavigateTo;

namespace ResearchDataManagementPlatform.WindowManagement.HomePane
{
    public partial class HomeBoxUI : UserControl
    {
        private IActivateItems _activator;
        private bool _doneSetup = false;

        public HomeBoxUI()
        {
            InitializeComponent();
            olvRecent.ItemActivate += OlvRecent_ItemActivate;
        }
        public void SetUp(IActivateItems activator,string title, Type openType,AtomicCommandUIFactory factory, params IAtomicCommand[] newCommands)
        {
            if (!_doneSetup)
            {
                _activator = activator;
                lblTitle.Text = title;

                btnNew.Image = FamFamFamIcons.add;
                btnNew.Text = "New";
                btnNew.DisplayStyle = ToolStripItemDisplayStyle.Text;

                btnNewDropdown.Image = FamFamFamIcons.add;
                btnNewDropdown.Text = "New...";
                btnNewDropdown.DisplayStyle = ToolStripItemDisplayStyle.Text;

                btnOpen.Text = "Open";
                btnOpen.DisplayStyle = ToolStripItemDisplayStyle.Text;
                btnOpen.Click += (s, e) =>
                {
                    var ui = new NavigateToObjectUI(activator);
                    ui.AlwaysFilterOn = openType;
                    ui.CompletionAction = Open;
                    ui.Show();
                };

            
                //if theres only one command for new
                if (newCommands.Length == 1)
                {
                    //don't use the dropdown
                    toolStrip1.Items.Remove(btnNewDropdown);
                    btnNew.Click += (s,e)=>newCommands.Single().Execute();
                }
                else
                {
                    toolStrip1.Items.Remove(btnNew);
                    btnNewDropdown.DropDownItems.AddRange(newCommands.Select(factory.CreateMenuItem).Cast<ToolStripItem>().ToArray());    
                }

                olvName.AspectGetter = (o) => ((HistoryEntry)o).Object.ToString();
                olvName.ImageGetter = (o) => activator.CoreIconProvider.GetImage(((HistoryEntry)o).Object);

                _doneSetup = true;
            }
            
            olvRecent.ClearObjects();
            olvRecent.AddObjects(activator.HistoryProvider.History.Where(h=>h.Object.GetType() == openType).ToArray());
        }

        private void Open(IMapsDirectlyToDatabaseTable o)
        {
            var cmd = new ExecuteCommandActivate(_activator, o)
            {
                AlsoShow = true
            };
            cmd.Execute();
        }

        private void OlvRecent_ItemActivate(object sender, EventArgs e)
        {
            if (olvRecent.SelectedObject is HistoryEntry he)
                Open(he.Object);
        }
    }
}
