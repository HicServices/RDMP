// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.CommandLine.Gui;

internal class ConsoleGuiBigListBox<T>
{
    private readonly string _okText;
    private readonly bool _addSearch;
    private readonly string _prompt;
    private IList<ListViewObject<T>> _collection;

    /// <summary>
    /// If the public constructor was used then this is the fixed list we were initialized with
    /// </summary>
    protected IList<T> _publicCollection;

    private bool _addNull;

    public T Selected { get; private set; }

    /// <summary>
    /// Determines what is rendered in the list visually
    /// </summary>
    public Func<T, string> AspectGetter { get; set; }

    /// <summary>
    /// Ongoing filtering of a large collection should be cancelled when the user changes the filter even if it is not completed yet
    /// </summary>
    private readonly ConcurrentBag<CancellationTokenSource> _cancelFiltering = [];

    private Task _currentFilterTask;
    private readonly Lock _taskCancellationLock = new();

    private ListView _listView;
    private bool _changes;
    private TextField _mainInput;
    private DateTime _lastKeypress = DateTime.Now;

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
    /// <param name="addNull">Creates a selection option "Null" that returns a null selection</param>
    public ConsoleGuiBigListBox(string prompt, string okText, bool addSearch, IList<T> collection,
        Func<T, string> displayMember, bool addNull) : this(prompt, okText, addSearch, displayMember)
    {
        _publicCollection = collection ?? throw new ArgumentNullException(nameof(collection));
        _addNull = addNull;
    }

    private class ListViewObject<T2> where T2 : T
    {
        private readonly Func<T2, string> _displayFunc;
        public T2 Object { get; }

        public ListViewObject(T2 o, Func<T2, string> displayFunc)
        {
            _displayFunc = displayFunc;
            Object = o;
        }

        public override string ToString() => _displayFunc(Object);

        public override int GetHashCode() => Object.GetHashCode();

        public override bool Equals(object obj) => obj is ListViewObject<T2> other && Object.Equals(other.Object);
    }

    /// <summary>
    /// Runs the dialog as modal blocking and returns true if a selection was made.
    /// </summary>
    /// <returns>True if selection was made (see <see cref="Selected"/>) or false if user cancelled the dialog</returns>
    public bool ShowDialog()
    {
        var okClicked = false;

        var win = new Window(_prompt)
        {
            X = 0,
            Y = 0,

            // By using Dim.Fill(), it will automatically resize without manual intervention
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            Modal = true,
            ColorScheme = ConsoleMainWindow.ColorScheme
        };

        _listView = new ListView(new List<string>(new[] { "Error" }))
        {
            X = 0,
            Y = 0,
            Height = Dim.Fill(2),
            Width = Dim.Fill(2)
        };

        _listView.KeyPress += _listView_KeyPress;
        _listView.SetSource((_collection = BuildList(GetInitialSource())).ToList());
        win.Add(_listView);

        var btnOk = new Button(_okText, true)
        {
            Y = Pos.Bottom(_listView),
            Width = 8,
            Height = 1
        };
        btnOk.Clicked += () =>
        {
            if (_listView.SelectedItem >= _collection.Count)
                return;

            okClicked = true;
            Application.RequestStop();
            Selected = _collection[_listView.SelectedItem].Object;
        };

        var btnCancel = new Button("Cancel")
        {
            Y = Pos.Bottom(_listView),
            Width = 10,
            Height = 1
        };
        btnCancel.Clicked += () => Application.RequestStop();

        if (_addSearch)
        {
            var searchLabel = new Label("Search:")
            {
                X = 0,
                Y = Pos.Bottom(_listView)
            };

            win.Add(searchLabel);

            _mainInput = new TextField("")
            {
                X = Pos.Right(searchLabel),
                Y = Pos.Bottom(_listView),
                Width = 30
            };

            btnOk.X = 38;
            btnCancel.X = Pos.Right(btnOk) + 1;

            win.Add(_mainInput);
            _mainInput.SetFocus();

            _mainInput.TextChanged += s =>
            {
                // Don't update the UI while user is hammering away on the keyboard
                _lastKeypress = DateTime.Now;
                RestartFiltering();
            };
        }
        else
        {
            btnOk.X = Pos.Center() - 5;
            btnCancel.X = Pos.Center() + 5;
        }


        win.Add(btnOk);
        win.Add(btnCancel);

        AddMoreButtonsAfter(win, btnCancel);

        var callback = Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(500), Timer);

        Application.Run(win, ConsoleMainWindow.ExceptionPopup);

        Application.MainLoop.RemoveTimeout(callback);

        return okClicked;
    }

    private void _listView_KeyPress(View.KeyEventEventArgs obj)
    {
        // if user types in some text change the focus to the text box to enable searching
        if (char.IsLetterOrDigit((char)obj.KeyEvent.KeyValue)) _mainInput.FocusFirst();
    }

    /// <summary>
    /// Last minute method for adding extra stuff to the window (to the right of <paramref name="btnCancel"/>)
    /// </summary>
    /// <param name="btnCancel"></param>
    protected virtual void AddMoreButtonsAfter(Window win, Button btnCancel)
    {
    }

    private bool Timer(MainLoop caller)
    {
        if (_changes && DateTime.Now.Subtract(_lastKeypress) > TimeSpan.FromSeconds(1))
            lock (_taskCancellationLock)
            {
                var oldSelected = _listView.SelectedItem;
                _listView.SetSource(_collection.ToList());

                if (oldSelected < _collection.Count)
                    _listView.SelectedItem = oldSelected;

                _changes = false;
                return true;
            }

        return true;
    }

    protected void RestartFiltering()
    {
        RestartFiltering(_mainInput.Text.ToString());
    }

    protected void RestartFiltering(string searchTerm)
    {
        var cts = new CancellationTokenSource();

        lock (_taskCancellationLock)
        {
            //cancel any previous searches
            foreach (var c in _cancelFiltering)
                c.Cancel();

            _cancelFiltering.Clear();

            _cancelFiltering.Add(cts);
        }

        _currentFilterTask = Task.Run(() =>
        {
            var result = BuildList(GetListAfterSearch(searchTerm, cts.Token));

            lock (_taskCancellationLock)
            {
                _collection = result;
                _changes = true;
            }
        }, cts.Token);
    }

    private IList<ListViewObject<T>> BuildList(IList<T> listOfT)
    {
        var toReturn = listOfT.Select(o => new ListViewObject<T>(o, AspectGetter)).ToList();

        if (_addNull)
            toReturn.Add(new ListViewObject<T>((T)(object)null, o => "Null"));

        return toReturn;
    }

    protected virtual IList<T> GetListAfterSearch(string searchString, CancellationToken token)
    {
        if (_publicCollection == null)
            throw new InvalidOperationException(
                "When using the protected constructor derived classes must override this method ");

        var searchTerms = searchString.Split(' ');

        //stop the Contains searching when the user cancels the search
        return _publicCollection.Where(o => !token.IsCancellationRequested &&
                                            // must have all search terms
                                            searchTerms.All(t =>
                                                AspectGetter(o).Contains(t, StringComparison.CurrentCultureIgnoreCase))
        ).ToList();
    }

    protected virtual IList<T> GetInitialSource() => _publicCollection ??
                                                     throw new InvalidOperationException(
                                                         "When using the protected constructor derived classes must override this method ");
}