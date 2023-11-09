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


        private ReactiveObject _currentViewModel;
        public ReactiveObject CurrentViewModel { get => _currentViewModel; private set => this.RaiseAndSetIfChanged(ref _currentViewModel,value); }

        private ReactiveObject GetCurrentViewModel(MainAppViewModel.Tabs t)
        {
            return new CatalogueTreeViewModel();
            //switch (t)
            //{
            //    case MainAppViewModel.Tabs.Catalogue: return new CatalogueTreeViewModel(); default :null;
            //}
        }

        private void SelectedTabChange(MainAppViewModel.Tabs st)
        {
            SelectedTab = st;
            CurrentViewModel = GetCurrentViewModel(st);

        }

        public LeftHandPanelViewModel()
        {
            Globals.SelectedTabChange += SelectedTabChange;
            _currentViewModel = new ReactiveObject();
        }
    }
}
