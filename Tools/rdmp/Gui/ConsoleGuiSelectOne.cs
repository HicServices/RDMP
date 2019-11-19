using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Providers;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    class ConsoleGuiSelectOne
    {
        private readonly ICoreChildProvider _childProvider;
        public IMapsDirectlyToDatabaseTable Selected { get; private set; }

        private List<IMapsDirectlyToDatabaseTable> _collection;

        public ConsoleGuiSelectOne(ICoreChildProvider childProvider)
        {
            _childProvider = childProvider;
        }

        public bool ShowDialog()
        {
            const int MaxMatches = 100;
            bool okClicked = false;

            var win = new Window ("Open") {
                X = 0,
                Y = 0,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };

            var listView = new ListView(new List<string>(new []{"Error"}))
            {
                X = 1,
                Y = 1,
                Height = Dim.Fill()-3,
                Width = Dim.Fill()
            };

            
            listView.SetSource( this._collection = _childProvider.GetAllSearchables().Keys.Take(MaxMatches).ToList());

            var searchLabel = new Label("Search:")
            {
                X = 0,
                Y = Pos.Percent(100)-2
            };
            
            var mainInput = new TextField ("") {
                X = 1,
                Y = Pos.Percent(100)-1,
                Width = Dim.Fill() - 10,
            };

            var btnOk = new Button("Ok",true)
            {
                X = Pos.Percent(100)-10,
                Y = Pos.Percent(100)-1,
                Width = 10,
                Height = 1
            };

            btnOk.Clicked = () =>
            {
                okClicked = true;
                Application.RequestStop();
                Selected = _collection[listView.SelectedItem];
            };
            
            win.Add(listView);
            win.Add(searchLabel);
            win.Add(btnOk);
            win.Add(mainInput);

            win.SetFocus(mainInput);
            
            mainInput.Changed += (s, e) =>
            {
                if (_childProvider != null)
                {
                    listView.SetSource(
                        _collection =

                            new SearchablesMatchScorer()
                                .ScoreMatches(_childProvider.GetAllSearchables(),mainInput.Text.ToString(),new CancellationToken())
                                .Where(score => score.Value > 0)
                                .OrderByDescending(score => score.Value)
                                .ThenByDescending(id=>id.Key.Key.ID) //favour newer objects over ties
                                .Take(MaxMatches)
                                .Select(score => score.Key.Key)
                                .ToList()
                    );

                }
            };

            Application.Run(win);

            return okClicked;
        }

    }
}