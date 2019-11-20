using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui
{
    internal class ConsoleGuiBigListBox<T>
    {
        private readonly string _okText;
        private readonly bool _addSearch;
        private readonly string _prompt;
        private IList<T> _collection;

        /// <summary>
        /// If the public constructor was used then this is the fixed list we were initialized with
        /// </summary>
        private IList<T> _publicCollection;

        public T Selected { get; private set; }

        /// <summary>
        /// Protected constructor for derived classes that want to do funky filtering and hot swap out lists as search
        /// enters (e.g. to serve a completely different collection on each keystroke)
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="okText"></param>
        /// <param name="addSearch"></param>
        protected ConsoleGuiBigListBox(string prompt, string okText, bool addSearch)
        {
            _okText = okText;
            _addSearch = addSearch;
            _prompt = prompt;
        }

        /// <summary>
        /// Public constructor that uses normal (contains text) search to filter the fixed <paramref name="collection"/>
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="okText"></param>
        /// <param name="addSearch"></param>
        /// <param name="collection"></param>
        public ConsoleGuiBigListBox(string prompt, string okText, bool addSearch, IList<T> collection):this(prompt,okText,addSearch)
        {
            if(collection == null)
                throw new ArgumentNullException("collection");

            _publicCollection = collection;
        }

        public bool ShowDialog()
        {
            bool okClicked = false;

            var win = new Window (_prompt) {
                X = 0,
                Y = 0,

                // By using Dim.Fill(), it will automatically resize without manual intervention
                Width = Dim.Fill (),
                Height = Dim.Fill ()
            };

            var listView = new ListView(new List<string>(new []{"Error"}))
            {
                X = 0,
                Y = 0,
                Height = Dim.Fill(2),
                Width = Dim.Fill(2)
            };

            listView.SetSource( (this._collection = GetInitialSource()).ToList());
            
            var btnOk = new Button(_okText,true)
            {
                X = Pos.Percent(100)-10,
                Y = Pos.Bottom(listView),
                Width = 5,
                Height = 1
            };

            btnOk.Clicked = () =>
            {
                okClicked = true;
                Application.RequestStop();
                Selected = _collection[listView.SelectedItem];
            };
            
            win.Add(listView);
            win.Add(btnOk);

            if (_addSearch)
            {
                var searchLabel = new Label("Search:")
                {
                    X = 0,
                    Y = Pos.Bottom(listView),
                };
            
                var mainInput = new TextField ("") {
                    X = Pos.Right(searchLabel),
                    Y = Pos.Bottom(listView),
                    Width = Dim.Fill() - 15,
                };
                
                win.Add(searchLabel);
                win.Add(mainInput);
                win.SetFocus(mainInput);
                
                mainInput.Changed += (s, e) =>
                {
                    listView.SetSource((_collection = GetListAfterSearch(mainInput.Text.ToString())).ToList());
                };
            }
            

            Application.Run(win);

            return okClicked;
        }

        protected virtual IList<T> GetListAfterSearch(string searchString)
        {
            if(_publicCollection == null)
                throw new InvalidOperationException("When using the protected constructor derived classes must override this method ");

            return _publicCollection.Where(o =>
                o.ToString().Contains(searchString, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        protected virtual IList<T> GetInitialSource()
        {
            if(_publicCollection == null)
                throw new InvalidOperationException("When using the protected constructor derived classes must override this method ");

            return _publicCollection;
        }
    }
}