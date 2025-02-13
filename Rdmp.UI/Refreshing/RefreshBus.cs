// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.UI.CommandExecution.AtomicCommands;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.TestsAndSetup.ServicePropogation;

namespace Rdmp.UI.Refreshing;

/// <summary>
/// Dispatcher class for publish events (<see cref="ExecuteCommandRefreshObject"/>)
/// </summary>
public class RefreshBus
{
    /// <summary>
    /// This event exists so that the IActivateItems can precache data for use by subscribers during publishing refresh events.  Do not subscribe to this event if you just want to
    /// know when stuff has changed, instead use the Subscribe and Unsubscribe methods
    /// </summary>
    public event RefreshObjectEventHandler BeforePublish;

    public event RefreshObjectEventHandler AfterPublish;
    private event RefreshObjectEventHandler RefreshObject;

    public bool PublishInProgress { get; private set; }

    public ICoreChildProvider ChildProvider { get; set; }

    private readonly Lock _oPublishLock = new();

    public void Publish(object sender, RefreshObjectEventArgs e)
    {
        if (PublishInProgress)
            throw new SubscriptionException(
                $"Refresh Publish Cascade error.  Subscriber {sender} just attempted a publish during an existing publish execution, cyclic inception publishing is not allowed, you cannot respond to a refresh callback by issuing more refresh publishes");

        lock (_oPublishLock)
        {
            BeforePublish?.Invoke(sender, e);

            try
            {
                PublishInProgress = true;
                // Set cursor as hourglass
                Cursor.Current = Cursors.WaitCursor;

                //refresh it from the child provider
                if (e.Exists)
                {
                    e.Object.RevertToDatabaseState();
                }
                else
                {
                    if (ChildProvider != null && e.DeletedObjectDescendancy == null)
                        e.DeletedObjectDescendancy = ChildProvider.GetDescendancyListIfAnyFor(e.Object);
                }

                RefreshObject?.Invoke(sender, e);
            }
            finally
            {
                AfterPublish?.Invoke(this, e);
                PublishInProgress = false;
                Cursor.Current = Cursors.Default;
            }
        }
    }

    private HashSet<IRefreshBusSubscriber> subscribers = new();

    public void Subscribe(IRefreshBusSubscriber subscriber)
    {
        if (subscribers.Contains(subscriber))
            throw new SubscriptionException(
                $"You cannot subscribe to the RefreshBus more than once. Subscriber '{subscriber}' just attempted to register a second time its type was({subscriber.GetType().Name})");

        RefreshObject += subscriber.RefreshBus_RefreshObject;

        subscribers.Add(subscriber);
    }

    public void Unsubscribe(IRefreshBusSubscriber unsubscriber)
    {
        if (!subscribers.Contains(unsubscriber))
            throw new SubscriptionException(
                $"You cannot unsubscribe from the RefreshBus if never subscribed in the first place. '{unsubscriber}' just attempted to unsubscribe when it wasn't subscribed in the first place its type was ({unsubscriber.GetType().Name})");

        RefreshObject -= unsubscriber.RefreshBus_RefreshObject;
        subscribers.Remove(unsubscriber);
    }

    public void EstablishLifetimeSubscription(ILifetimeSubscriber c)
    {
        if (c is not IRefreshBusSubscriber subscriber)
            throw new ArgumentException("Control must be an IRefreshBusSubscriber to establish a lifetime subscription",
                nameof(c));

        //ignore double requests for subscription
        if (subscribers.Contains(subscriber))
            return;

        if (c is not ContainerControl containerControl)
            throw new ArgumentOutOfRangeException(nameof(c));

        var parentForm = containerControl.ParentForm ?? throw new ArgumentException(
            "Control must have an established ParentForm, you should not attempt to establish a lifetime subscription until your control is loaded (i.e. don't call this in your constructor)",
            nameof(c));
        Subscribe(subscriber);
        parentForm.FormClosing += (s, e) => Unsubscribe(subscriber);
    }

    private List<object> _selfDestructors = new();


    /// <summary>
    /// Registers your control as a lifetime user of RefreshBus without you having to implement ILifetimeSubscriber.  The implementation instead is the following:
    /// 1. If the RefreshBus sees a refresh for any object that was not Published by yourself
    ///     1.1 Refreshbus will check your originalObject still Exists
    ///     1.2 If not then your ParentForm will be .Closed
    /// 2. If the RefreshBus sees a refresh for your object specifically (that was not published by yourself)
    ///     2.1 SetDatabaseObject will be called with the new state (in memory as it was passed to Publish) of the object
    /// 
    /// <para>Note: you can subscribe to EstablishSelfDestructProtocol in your SetDatabaseObject method if you want without worrying about repeat subscriptions but know that only
    /// the first subscription is respected therefore you should NOT change the database object to a different one</para>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="user"></param>
    /// <param name="activator"></param>
    /// <param name="originalObject"></param>
    public void EstablishSelfDestructProtocol<T>(RDMPSingleDatabaseObjectControl<T> user, IActivateItems activator,
        T originalObject) where T : DatabaseEntity
    {
        //they already subscribed to self-destruct protocols - repeat subscriptions can be caused by registering in SetDatabaseObject and then refresh callbacks triggering more calls to SetDatabaseObject within the Controls lifetime
        var existingSubscription =
            _selfDestructors.OfType<SelfDestructProtocol<T>>().SingleOrDefault(s => s.User == user);

        //they have subscribed for this before
        if (existingSubscription != null)
            if (!existingSubscription.OriginalObject
                    .Equals(originalObject)) //wait a minute! they subscribed for a different object!
                throw new ArgumentException(
                    $"user {user} attempted to subscribe twice for self destruct but with two different objects '{existingSubscription.OriginalObject}' and '{originalObject}'",
                    nameof(user));
            else
                return; //they subscribed for the same object it's all ok

        //The anonymous refresh callback that updates the user when the object changes or deletes
        var subscriber = new SelfDestructProtocol<T>(user, activator, originalObject);

        //keep track of the self destructors independently of the subscription list
        _selfDestructors.Add(subscriber);

        //subscribe them now
        Subscribe(subscriber);

        var parentForm = user.ParentForm ?? throw new ArgumentException(
            "Control must have an established ParentForm, you should not attempt to establish a lifetime subscription until your control is loaded (i.e. don't call this in your constructor)",
            nameof(user));

        //when their parent closes we unsubscribe them
        parentForm.FormClosed += (s, e) =>
        {
            Unsubscribe(subscriber);
            var toRemove = _selfDestructors.OfType<SelfDestructProtocol<T>>().Single(u => u.User == user);
            _selfDestructors.Remove(toRemove);
        };
    }
}