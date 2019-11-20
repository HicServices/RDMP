// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
        /// Determines what is rendered in the list visually
        /// </summary>
        public Func<T, string> AspectGetter { get; set; }

        /// <summary>
        /// Protected constructor for derived classes that want to do funky filtering and hot swap out lists as search
        /// enters (e.g. to serve a completely different collection on each keystroke)
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="okText"></param>
        /// <param name="addSearch"></param>
        /// <param name="displayMember"></param>
        protected ConsoleGuiBigListBox(string prompt, string okText, bool addSearch, Func<T, string> displayMember)
        {
            _okText = okText;
            _addSearch = addSearch;
            _prompt = prompt;
            
            AspectGetter = displayMember ?? (arg => arg?.ToString() ?? string.Empty);
        }

        /// <summary>
        /// Public constructor that uses normal (contains text) search to filter the fixed <paramref name="collection"/>
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="okText"></param>
        /// <param name="addSearch"></param>
        /// <param name="collection"></param>
        /// <param name="displayMember">What to display in the list box (defaults to <see cref="object.ToString"/></param>
        public ConsoleGuiBigListBox(string prompt, string okText, bool addSearch, IList<T> collection,Func<T,string> displayMember):this(prompt,okText,addSearch,displayMember)
        {
            if(collection == null)
                throw new ArgumentNullException("collection");

            _publicCollection = collection;
        }

        

        private class ListViewObject<T2> where T2:T
        {
            private readonly Func<T2, string> _displayFunc;
            public T2 Object { get; }

            public ListViewObject(T2 o, Func<T2,string> displayFunc)
            {
                _displayFunc = displayFunc;
                Object = o;
            }

            public override string ToString()
            {
                return _displayFunc(Object);
            }
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

            listView.SetSource( (this._collection = GetInitialSource()).Select(o=>new ListViewObject<T>(o,AspectGetter)).ToList());
            
            var btnOk = new Button(_okText,true)
            {
                X = Pos.Percent(70),
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

            var btnCancel = new Button("Cancel")
            {
                X = Pos.Percent(85),
                Y = Pos.Bottom(listView),
                Width = 5,
                Height = 1,
                Clicked = Application.RequestStop
            };
            
            win.Add(listView);
            win.Add(btnOk);
            win.Add(btnCancel);

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
                    Width = Dim.Fill() - 30,
                };
                
                win.Add(searchLabel);
                win.Add(mainInput);
                win.SetFocus(mainInput);
                
                mainInput.Changed += (s, e) =>
                {
                    listView.SetSource((_collection = GetListAfterSearch(mainInput.Text.ToString())).Select(o=>new ListViewObject<T>(o,AspectGetter)).ToList());
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
                AspectGetter(o).Contains(searchString, StringComparison.CurrentCultureIgnoreCase)).ToList();
        }

        protected virtual IList<T> GetInitialSource()
        {
            if(_publicCollection == null)
                throw new InvalidOperationException("When using the protected constructor derived classes must override this method ");

            return _publicCollection;
        }
    }
}