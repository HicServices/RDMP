using System.Collections.Generic;
using System.Linq;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking
{
    public class NavigationTrack
    {
        private Stack<DockContent> _navigationStack = new Stack<DockContent>();
        private Stack<DockContent> _forward = new Stack<DockContent>();

        const int MaxHistory = 10;
        private bool _suspended = false;

        /// <summary>
        /// The last tab navigated to
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

        public bool CanBack()
        {
            return GetHistory(1).Any();
        }
        public bool CanForward()
        {
            Prune();

            return _forward.Count > 0;
        }

        public DockContent[] GetHistory(int maxToReturn)
        {
            Prune();

            return _navigationStack.ToArray().Skip(1).Take(maxToReturn).ToArray();
        }

        private bool IsAlive(DockContent peek)
        {
            return peek.ParentForm != null;
        }

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

        
        public void Suspend()
        {
            _suspended = true;
        }

        public void Resume()
        {
            _suspended = false;
        }
    }
}
