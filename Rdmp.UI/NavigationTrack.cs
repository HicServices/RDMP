// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Rdmp.UI;

/// <summary>
/// Handles tracking which where the user navigates to for Forward / Backward purposes.
/// </summary>
public class NavigationTrack<T>
{
    private Stack<T> _navigationStack = new();
    private Stack<T> _forward = new();

    private const int MaxHistory = 10;
    private bool _suspended = false;

    /// <summary>
    /// Called when changes are detected, includes Clear, Append etc. Does not include <see cref="Prune"/> which is often called as part of internal operations.
    /// </summary>
    public event EventHandler Changed;

    /// <summary>
    /// The last T navigated to or null if no T are alive / pushed
    /// </summary>
    public T Current
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
    /// Removes all dead objects in the history (forward and backwards).  This is based on the alive delegate used to construct the <see cref="NavigationTrack{T}"/>
    /// </summary>
    public void Prune()
    {
        _navigationStack = new Stack<T>(_navigationStack.ToArray().Take(MaxHistory + 1).Reverse().Where(_isAlive));
        _forward = new Stack<T>(_forward.ToArray().Reverse().Where(_isAlive));
    }

    /// <summary>
    /// Calls <see cref="Back(bool)"/> <paramref name="i"/> times.  If this results in a valid <see cref="Current"/> then the activate delegate will be triggered
    /// </summary>
    /// <param name="i"></param>
    /// <param name="show"></param>
    /// <returns></returns>
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
    /// Activates the given <typeparamref name="T"/> without adding it to the history stack
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
    /// Returns and optionally Activates the last entry in the history.  This changes the location history
    /// </summary>
    /// <param name="show">True to launch the activation delegate</param>
    /// <returns></returns>
    public T Back(bool show)
    {
        Prune();

        if (Current == null)
            return default;

        var pop = _navigationStack.Pop();

        var newHead = Current;
        if (newHead == null)
        {
            _navigationStack.Push(pop);
            return default;
        }

        _forward.Push(pop);

        if (show)
            Activate(newHead);

        Changed?.Invoke(this,EventArgs.Empty);

        return newHead;
    }

    /// <summary>
    /// Returns and optionally Activates the next object in the history.  This changes the location history
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
            
        Changed?.Invoke(this,EventArgs.Empty);

        return r;
    }

    /// <summary>
    /// Returns true if there is a history that can be navigated back to
    /// </summary>
    /// <returns></returns>
    public bool CanBack() => GetHistory(1).Any();

    /// <summary>
    /// Returns true if the current state is an exploration of the past history and therefore the user can navigate Forwards again
    /// </summary>
    /// <returns></returns>
    public bool CanForward()
    {
        Prune();

        return _forward.Count > 0;
    }

    /// <summary>
    /// Returns x history objects that <see cref="Back(bool)"/> would go to if called.  This does not affect the state of the history.  Result does not include <see cref="Current"/>
    /// </summary>
    /// <returns></returns>
    public T[] GetHistory(int maxToReturn)
    {
        Prune();

        return _navigationStack.ToArray().Skip(1).Take(maxToReturn).ToArray();
    }

    private Func<T, bool> _isAlive;
    private readonly Action<T> _activate;

    /// <summary>
    /// Records that the user has made a new navigation to a fresh page.  This will invalidate any Forward history
    /// </summary>
    /// <param name="newHead"></param>
    public void Append(T newHead)
    {
        //don't push the newHead if we are suspended
        if (_suspended || newHead == null)
            return;

        //don't push the newHead if it is already the head
        if (_navigationStack.Count != 0 && Equals(_navigationStack.Peek(), newHead))
            return;

        //don't allow going forward after a novel forward
        _forward.Clear();

        _navigationStack.Push(newHead);

        Changed?.Invoke(this,EventArgs.Empty);
    }

    /// <summary>
    /// Changes the behaviour of <see cref="Append"/> to do nothing, use this if you want to activate something or load a layout without populating the history / affecting the current history.
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