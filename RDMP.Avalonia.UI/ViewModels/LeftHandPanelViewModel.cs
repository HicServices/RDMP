using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDMP.Avalonia.UI.ViewModels
{
    public class LeftHandPanelViewModel : ReactiveObject
    {

        private MainAppViewModel.Tabs _selectedTab;
        public MainAppViewModel.Tabs SelectedTab { get => _selectedTab; private set => this.RaiseAndSetIfChanged(ref _selectedTab,value); }


        private void SelectedTabChange(MainAppViewModel.Tabs st)
        {
            SelectedTab = st;
        }

        public LeftHandPanelViewModel()
        {
            //SelectedTab = Globals.SelectedTab;
            Globals.SelectedTabChange += SelectedTabChange;
        }
    }
}
