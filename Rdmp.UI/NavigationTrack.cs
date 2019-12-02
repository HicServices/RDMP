// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.UI
{
    /// <summary>
    /// Handles tracking which tabs the user switched between for Forward / Backward navigation.
    /// </summary>
    public class NavigationTrack<T>
    {
        private Stack<T> _navigationStack = new Stack<T>();
        private Stack<T> _forward = new Stack<T>();

        const int MaxHistory = 10;
        private bool _suspended = false;

        /// <summary>
        /// The last tab navigated to or null if no tabs are open
        /// </summary>
        public T CurrentTab
        {
            get
            {
                while (_navigationStack.Count != 0)
                {
                    var p = _navigationStack.Peek();

                    if (_isAlive(p))
                        return p;

                    _navigationStack.Pop();
                }

                return default;
            }
        }

        /// <summary>
        /// Creates a new Forward/Backward navigation stack supporting pruning.
        /// </summary>
        /// <param name="aliveDelegate">Delegate for pruning dead items from the stack (e.g. closed forms).  Return true if your <typeparamref name="T"/> should survive filter.</param>
        /// <param name="activate">Delegate to execute when Forward/Backward happens (e.g. bring to focus).</param>
        public NavigationTrack(Func<T, bool> aliveDelegate, Action<T> activate)
        {
            _isAlive = aliveDelegate;
            _activate = activate;
        }
        /// <summary>
        /// Removes all closed tabs in the history (forward and backwards)
        /// </summary>
        public void Prune()
        {
            _navigationStack = new Stack<T>(_navigationStack.ToArray().Take(MaxHistory + 1).Reverse().Where(_isAlive));
            _forward = new Stack<T>(_forward.ToArray().Reverse().Where(_isAlive));
        }

        public T Back(int i, bool show)
        {
            T toShow = default;

            while (i-- > 0)
                toShow = Back(false);

            if (toShow != null && show)
                Activate(toShow);

            return toShow;
        }

        /// <summary>
        /// Activates the given tab without adding it to the history stack
        /// </summary>
        /// <param name="toShow"></param>
        private void Activate(T toShow)
        {
            if (toShow == null)
                return;

            var sBefore = _suspended;

            _suspended = true;

            try
            {
                _activate(toShow);
            }
            finally
            {
                _suspended = sBefore;
            }
        }

        /// <summary>
        /// Returns and optionally Activates the previous tab in the history.  This changes the location history
        /// </summary>
        /// <param name="show">True to launch the activation delegate</param>
        /// <returns></returns>
        public T Back(bool show)
        {
            Prune();

            if (CurrentTab == null)
                return default;

            var pop = _navigationStack.Pop();

            var newHead = CurrentTab;
            if (newHead == null)
            {
                _navigationStack.Push(pop);
                return default;
            }

            _forward.Push(pop);

            if (show)
                Activate(newHead);

            return newHead;
        }

        /// <summary>
        /// Returns and optionally Activates the next tab in the history.  This changes the location history
        /// 
        /// <para>Does nothing if you have not already gone <see cref="Back(bool)"/></para>
        /// </summary>
        /// <param name="show"></param>
        /// <returns></returns>
        public T Forward(bool show)
        {
            Prune();

            if (_forward.Count == 0)
                return default;

            var r = _forward.Pop();
            if (r != null && show)
                Activate(r);

            _navigationStack.Push(r);

            return r;
        }

        /// <summary>
        /// Returns true if there are tabs in the history that can be navigated back to
        /// </summary>
        /// <returns></returns>
        public bool CanBack()
        {
            return GetHistory(1).Any();
        }

        /// <summary>
        /// Returns true if there are tabs in the history that can be navigated forwards to
        /// </summary>
        /// <returns></returns>
        public bool CanForward()
        {
            Prune();

            return _forward.Count > 0;
        }

        /// <summary>
        /// Returns x history tabs that <see cref="Back(bool)"/> would go to if called.  This does not affect the state of the history
        /// </summary>
        /// <returns></returns>
        public T[] GetHistory(int maxToReturn)
        {
            Prune();

            return _navigationStack.ToArray().Skip(1).Take(maxToReturn).ToArray();
        }

        Func<T, bool> _isAlive;
        private readonly Action<T> _activate;

        /// <summary>
        /// Records that the user has made a new navigation to a fresh page.  This will invalidate any Forward history
        /// </summary>
        /// <param name="newTab"></param>
        public void Append(T newTab)
        {
            //don't push the tab if we are suspended
            if (_suspended || newTab == null)
                return;

            //don't push the tab if it is already the head
            if (_navigationStack.Count != 0 && Equals(_navigationStack.Peek(), newTab))
                return;

            //don't allow going forward after a novel forward
            _forward.Clear();

            _navigationStack.Push(newTab);
        }

        /// <summary>
        /// Changes the behaviour of <see cref="Append"/> to do nothing, use this if you want to activate a tab or load a layout without
        /// populating the history / affecting the current history.
        /// </summary>
        public void Suspend()
        {
            _suspended = true;
        }

        /// <summary>
        /// Ends the suspended state created by <see cref="Suspend"/>
        /// </summary>
        public void Resume()
        {
            _suspended = false;
        }
    }
}
