// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking
{
    /// <summary>
    /// Handles tracking which tabs the user switched between for Forward / Backward navigation.
    /// </summary>
    public class NavigationTrack
    {
        private Stack<DockContent> _navigationStack = new Stack<DockContent>();
        private Stack<DockContent> _forward = new Stack<DockContent>();

        const int MaxHistory = 10;
        private bool _suspended = false;

        /// <summary>
        /// The last tab navigated to or null if no tabs are open
        /// </summary>
        public DockContent CurrentTab
        {
            get
            {
                while (_navigationStack.Count != 0 )
                {
                    var p = _navigationStack.Peek();
                    
                    if (IsAlive(p))
                        return p;

                    _navigationStack.Pop();
                }

                return null;
            }
        }


        /// <summary>
        /// Removes all closed tabs in the history (forward and backwards)
        /// </summary>
        public void Prune()
        {
            _navigationStack = new Stack<DockContent>(_navigationStack.ToArray().Take(MaxHistory+1).Reverse().Where(IsAlive));
            _forward = new Stack<DockContent>(_forward.ToArray().Reverse().Where(IsAlive));
        }

        public DockContent Back(int i, bool show)
        {
            DockContent toShow = null;

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
        private void Activate(DockContent toShow)
        {
            if(toShow == null)
                return;

            var sBefore = _suspended;

            _suspended = true;
            
            try
            {
                toShow.Activate();
            }
            finally
            {
                _suspended = sBefore;
            }
        }

        /// <summary>
        /// Returns and optionally Activates the previous tab in the history.  This changes the location history
        /// </summary>
        /// <param name="show"></param>
        /// <returns></returns>
        public DockContent Back(bool show)
        {
            Prune();

            if (CurrentTab == null)
                return null;

            var pop = _navigationStack.Pop();

            var newHead = CurrentTab;
            if(newHead == null)
            {
                _navigationStack.Push(pop);
                return null;
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
        public DockContent Forward(bool show)
        {
            Prune();

            if (_forward.Count == 0)
                return null;

            var r = _forward.Pop();
            if(r != null && show)
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
        public DockContent[] GetHistory(int maxToReturn)
        {
            Prune();

            return _navigationStack.ToArray().Skip(1).Take(maxToReturn).ToArray();
        }

        private bool IsAlive(DockContent peek)
        {
            return peek.ParentForm != null;
        }

        /// <summary>
        /// Records that the user has made a new navigation to a fresh page.  This will invalidate any Forward history
        /// </summary>
        /// <param name="newTab"></param>
        public void Append(DockContent newTab)
        {
            //don't push the tab if we are suspended
            if (_suspended || newTab == null)
                return;
            
            //don't push the tab if it is already the head
            if(_navigationStack.Count != 0 && _navigationStack.Peek() == newTab)
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
