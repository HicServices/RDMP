// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Revertable;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.Core.ReusableLibraryCode.Icons.IconProvision;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Rdmp.Core.CommandExecution;

/// <summary>
/// Basic implementation of ICommandExecution ensures that if a command is marked IsImpossible then it cannot be run.  Call SetImpossible to render your command
/// un runnable with the given arguments.  You cannot make an IsImpossible command Possible again (therefore you should probably make this discision in your
/// constructor).  Override Execute to provide the implementation logic of your command but make sure to leave the base.Execute() call in first to ensure
/// IsImpossible is respected in the unlikely event that some code or user attempts to execute an impossible command.
/// 
/// <para>Override GetCommandHelp and GetCommandName to change the presentation layer of the command (if applicable).</para>
/// </summary>
public abstract class BasicCommandExecution : IAtomicCommand
{
    /// <summary>
    /// The last command executed by RDMP (will be null at start)
    /// </summary>
    public static IAtomicCommand LastCommand;

    public IBasicActivateItems BasicActivator { get; }

    public bool IsImpossible { get; private set; }
    public string ReasonCommandImpossible { get; private set; }
    public string OverrideCommandName { get; set; }

    public Image<Rgba32> OverrideIcon { get; set; }

    /// <summary>
    /// Set to true to suppress the <see cref="Publish(IMapsDirectlyToDatabaseTable)"/> method.  Only use if you are running multiple commands one after the other and don't want to wait for state updates
    /// </summary>
    public bool NoPublish { get; set; }

    /// <summary>
    /// The prefix that must appear on all Types derived from <see cref="BasicCommandExecution"/> in order to be rendered correctly in
    /// menus, called from the command line etc.
    /// </summary>
    public const string ExecuteCommandPrefix = "ExecuteCommand";

    /// <summary>
    /// True to add "..." to the end of the <see cref="ICommandExecution.GetCommandName"/>
    /// </summary>
    protected bool UseTripleDotSuffix { get; set; }


    /// <summary>
    /// When presenting the command in a hierarchical presentation should it be under a subheading
    /// (e.g. in a context menu).  Null if not
    /// </summary>
    public string SuggestedCategory { get; set; }

    /// <summary>
    /// Key which should result in this command being fired e.g. "F2"
    /// </summary>
    public string SuggestedShortcut { get; set; }

    /// <summary>
    /// True to require Ctrl key to be pressed when <see cref="SuggestedShortcut"/> is entered
    /// </summary>
    public bool Ctrl { get; set; }

    /// <inheritdoc/>
    public float Weight { get; set; }

    protected void SetImpossibleIfReadonly(IMightBeReadOnly m)
    {
        if (m?.ShouldBeReadOnly(out var reason) == true)
            SetImpossible(
                $"{(m is IContainer ? "Container" : '\'' + m.ToString() + '\'')} is readonly beacause:{reason}");
    }

    public BasicCommandExecution()
    {
    }

    public BasicCommandExecution(IBasicActivateItems basicActivator)
    {
        BasicActivator = basicActivator;
    }

    public virtual void Execute()
    {
        LastCommand = this;

        if (IsImpossible)
            throw new ImpossibleCommandException(this, ReasonCommandImpossible);
    }

    /// <summary>
    /// Returns human readable name for the command (This includes spaces and may have triple dots at the end, see <see cref="UseTripleDotSuffix"/>).
    /// For a programmatic name e.g. if user is to type command name into a CLI then use the static method <see cref="GetCommandName(string)"/>
    /// </summary>
    /// <returns></returns>
    public virtual string GetCommandName()
    {
        if (!string.IsNullOrWhiteSpace(OverrideCommandName))
            return UseTripleDotSuffix ? $"{OverrideCommandName}..." : OverrideCommandName;

        var name = GetType().Name;
        var adjusted = name.Replace(ExecuteCommandPrefix, "");

        if (UseTripleDotSuffix)
            adjusted += "...";

        return UsefulStuff.PascalCaseStringToHumanReadable(adjusted);
    }

    /// <summary>
    /// Returns the name of the command in programmatic format e.g. no spaces no triple dot suffix etc.
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static string GetCommandName(string typeName) => typeName.Replace(ExecuteCommandPrefix, "");

    /// <summary>
    /// Returns the name of the command in programmatic format e.g. no spaces no triple dot suffix etc.
    /// </summary>
    /// <returns></returns>
    public static string GetCommandName<T>() where T : BasicCommandExecution => GetCommandName(typeof(T).Name);

    public virtual string GetCommandHelp() => string.Empty;

    public virtual Image<Rgba32> GetImage(IIconProvider iconProvider) => OverrideIcon;

    /// <summary>
    /// disables the command because of the given reason.  This will result in grayed out menu items, crashes when executed programatically etc.
    /// </summary>
    /// <param name="reason"></param>
    protected void SetImpossible(string reason)
    {
        IsImpossible = true;
        ReasonCommandImpossible = reason;
    }

    /// <summary>
    /// disables the command because of the given reason.  This will result in grayed out menu items, crashes when executed programatically etc.
    /// This overload calls string.Format with the <paramref name="objects"/>
    /// </summary>
    /// /// <param name="reason"></param>
    /// <param name="objects">Objects to pass to string.Format</param>
    protected void SetImpossible(string reason, params object[] objects)
    {
        IsImpossible = true;
        ReasonCommandImpossible = string.Format(reason, objects);
    }

    /// <summary>
    /// Resets the IsImpossible status of the command
    /// </summary>
    protected void ResetImpossibleness()
    {
        IsImpossible = false;
        ReasonCommandImpossible = null;
    }

    /// <summary>
    /// Offers the user a binary choice and returns true if they accept it.  This method is blocking.
    /// </summary>
    /// <param name="text">The question to pose</param>
    /// <param name="caption"></param>
    /// <returns></returns>
    protected bool YesNo(string text, string caption) => BasicActivator.YesNo(text, caption);

    protected virtual void Publish(IMapsDirectlyToDatabaseTable o)
    {
        if (NoPublish)
            return;

        if (o is DatabaseEntity d)
            BasicActivator.Publish(d);
    }

    /// <summary>
    /// Reports a low visibility error to the <see cref="IBasicActivateItems.GlobalErrorCheckNotifier"/>.  Throws <paramref name="ex"/>
    /// with <paramref name="msg"/> if no global errors handler is registered
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="ex"></param>
    protected void GlobalError(string msg, Exception ex)
    {
        if (BasicActivator?.GlobalErrorCheckNotifier == null)
            throw new Exception(msg, ex);

        BasicActivator.GlobalErrorCheckNotifier.OnCheckPerformed(new CheckEventArgs(msg, CheckResult.Fail, ex));
    }

    /// <summary>
    /// Displays the given message to the user
    /// </summary>
    /// <param name="message"></param>
    protected void Show(string message)
    {
        BasicActivator.Show(message);
    }

    /// <summary>
    /// Displays the given message to the user with the given title
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    protected void Show(string title, string message)
    {
        BasicActivator.Show(title, message);
    }

    /// <summary>
    /// Displays the given message to the user, calling String.Format
    /// </summary>
    /// <param name="message"></param>
    /// <param name="objects">Objects to use for {0},{1} etc tokens in <paramref name="message"/></param>
    protected void Show(string message, params object[] objects)
    {
        BasicActivator.Show(string.Format(message, objects));
    }


    /// <summary>
    /// Prompts the user to type in some text (up to a maximum length).  Returns true if they supplied some text or false if they didn't or it was blank/cancelled etc
    /// </summary>
    /// <param name="header"></param>
    /// <param name="prompt"></param>
    /// <param name="maxLength"></param>
    /// <param name="initialText"></param>
    /// <param name="text"></param>
    /// <param name="requireSaneHeaderText"></param>
    /// <returns></returns>
    protected bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText = false) =>
        BasicActivator.TypeText(header, prompt, maxLength, initialText, out text, requireSaneHeaderText);

    /// <inheritdoc cref="TypeText(string, string, int, string, out string,bool)"/>
    protected bool TypeText(string header, string prompt, out string text) =>
        TypeText(header, prompt, 500, null, out text);

    /// <inheritdoc cref="IBasicActivateItems.ShowException"/>
    protected void ShowException(string message, Exception exception)
    {
        BasicActivator.ShowException(message, exception);
    }

    /// <summary>
    /// Prompts user to select 1 of the objects of type T in the list you provide
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="availableObjects"></param>
    /// <param name="initialSearchText"></param>
    /// <param name="allowAutoSelect">True to silently auto select the object if there are only 1 <paramref name="availableObjects"/></param>
    /// <returns></returns>
    protected T SelectOne<T>(IList<T> availableObjects, string initialSearchText = null, bool allowAutoSelect = false)
        where T : DatabaseEntity =>
        SelectOne(new DialogArgs
        {
            InitialSearchText = initialSearchText,
            AllowAutoSelect = allowAutoSelect
        }, availableObjects, out var selected)
            ? selected
            : null;

    /// <summary>
    /// Prompts user to select 1 of the objects of type T in the list you provide
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <param name="availableObjects"></param>
    /// <returns></returns>
    protected T SelectOne<T>(DialogArgs args, IList<T> availableObjects) where T : DatabaseEntity =>
        SelectOne(args, availableObjects, out var selected) ? selected : null;

    /// <summary>
    /// Prompts user to select 1 object of type T from all the ones stored in the repository provided
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="initialSearchText"></param>
    /// <param name="allowAutoSelect">True to silently auto select the object if there are only 1 compatible object in the <paramref name="repository"/></param>
    /// <returns></returns>
    protected T SelectOne<T>(IRepository repository, string initialSearchText = null, bool allowAutoSelect = false)
        where T : DatabaseEntity =>
        SelectOne(new DialogArgs
        {
            InitialSearchText = initialSearchText,
            AllowAutoSelect = allowAutoSelect
        }, repository.GetAllObjects<T>().ToList(), out var answer)
            ? answer
            : null;

    /// <summary>
    /// Prompts user to select 1 of the objects of type T from all the ones stored in the repository provided, returns true if they made a non null selection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="selected"></param>
    /// <param name="initialSearchText"></param>
    /// <param name="allowAutoSelect">True to silently auto select the object if there are only 1 compatible object in the <paramref name="repository"/></param>
    /// <returns></returns>
    protected bool SelectOne<T>(IRepository repository, out T selected, string initialSearchText = null,
        bool allowAutoSelect = false) where T : DatabaseEntity =>
        SelectOne(new DialogArgs
        {
            InitialSearchText = initialSearchText,
            AllowAutoSelect = allowAutoSelect
        }, repository.GetAllObjects<T>().ToList(), out selected);

    /// <summary>
    /// Prompts user to select 1 of the objects of type T in the list you provide, returns true if they made a non null selection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="availableObjects"></param>
    /// <param name="selected"></param>
    /// <param name="initialSearchText"></param>
    /// <param name="allowAutoSelect">True to silently auto select the object if there are only 1 <paramref name="availableObjects"/></param>
    /// <returns></returns>
    protected bool SelectOne<T>(IList<T> availableObjects, out T selected, string initialSearchText = null,
        bool allowAutoSelect = false) where T : DatabaseEntity =>
        SelectOne(new DialogArgs
        {
            InitialSearchText = initialSearchText,
            AllowAutoSelect = allowAutoSelect
        }, availableObjects, out selected);

    /// <summary>
    /// Prompts user to select 1 of the objects of type T in the list you provide, returns true if they made a non null selection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <param name="availableObjects"></param>
    /// <param name="selected"></param>
    /// <returns></returns>
    protected bool SelectOne<T>(DialogArgs args, IList<T> availableObjects, out T selected) where T : DatabaseEntity
    {
        selected = (T)BasicActivator.SelectOne(args, availableObjects.ToArray());
        return selected != null;
    }

    /// <summary>
    /// Prompts user to select 1 of the objects of type T from the objects existing in <paramref name="repository"/>, returns true if they made a non null selection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="args"></param>
    /// <param name="repository"></param>
    /// <param name="selected"></param>
    /// <returns></returns>
    protected bool SelectOne<T>(DialogArgs args, IRepository repository, out T selected) where T : DatabaseEntity
    {
        selected = (T)BasicActivator.SelectOne(args, repository.GetAllObjects<T>().ToArray());
        return selected != null;
    }

    protected bool SelectMany<T>(T[] available, out T[] selected, string initialSearchText = null)
        where T : DatabaseEntity
    {
        selected = BasicActivator.SelectMany("Select Objects", typeof(T), available, initialSearchText)?.Cast<T>()
            ?.ToArray();
        return selected != null && selected.Any();
    }

    protected bool SelectMany<T>(DialogArgs dialogArgs, T[] available, out T[] selected) where T : DatabaseEntity
    {
        selected = BasicActivator.SelectMany(dialogArgs, typeof(T), available)?.Cast<T>()?.ToArray();
        return selected != null && (dialogArgs.AllowSelectingNull || selected.Any());
    }

    protected void Wait(string title, Task task, CancellationTokenSource cts)
    {
        BasicActivator.Wait(title, task, cts);
    }

    protected void Emphasise(object o, int expansionDepth = 0)
    {
        BasicActivator.RequestItemEmphasis(this, new EmphasiseRequest(o, expansionDepth));
    }

    protected DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription) =>
        BasicActivator.SelectDatabase(allowDatabaseCreation, taskDescription);

    protected DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription) =>
        BasicActivator.SelectTable(allowDatabaseCreation, taskDescription);

    protected virtual void Activate(DatabaseEntity o)
    {
        BasicActivator.Activate(o);
    }

    /// <summary>
    /// Executes a given constructor (identified by <paramref name="constructorSelector"/>) by reading values out of the picker
    /// (or prompting the user if <paramref name="pickerArgsIfAny"/> is null)
    /// </summary>
    /// <param name="toConstruct">The Type you want to construct</param>
    /// <param name="constructorSelector">Selects which constructor on <paramref name="toConstruct"/> you want to invoke</param>
    /// <param name="pickerArgsIfAny"></param>
    /// <returns></returns>
    protected object Construct(Type toConstruct, Func<ConstructorInfo> constructorSelector,
        IEnumerable<CommandLineObjectPickerArgumentValue> pickerArgsIfAny = null)
    {
        var invoker = new CommandInvoker(BasicActivator);

        var constructor = constructorSelector();

        var constructorValues = new List<object>();

        using (var pickerEnumerator = pickerArgsIfAny?.GetEnumerator())
        {
            foreach (var parameterInfo in constructor.GetParameters())
            {
                var required = new RequiredArgument(parameterInfo);
                var parameterDelegate = invoker.GetDelegate(required);

                if (parameterDelegate.IsAuto)
                {
                    constructorValues.Add(parameterDelegate.Run(required));
                }
                else
                {
                    //it's not auto
                    if (pickerEnumerator != null)
                    {
                        pickerEnumerator.MoveNext();

                        if (pickerEnumerator.Current == null)
                            throw new ArgumentException(
                                $"Value needed for parameter '{required.Name}' (of type '{required.Type}')");

                        //construct with the picker arguments
                        if (!pickerEnumerator.Current.HasValueOfType(required.Type))
                            throw new NotSupportedException(
                                $"Argument '{pickerEnumerator.Current.RawValue}' could not be converted to required Type '{required.Type}' for argument {required.Name}");

                        //it is a valid object yay!
                        constructorValues.Add(pickerEnumerator.Current.GetValueForParameterOfType(required.Type));
                    }
                    else
                    {
                        //construct by prompting user for the values
                        constructorValues.Add(invoker.GetValueForParameterOfType(parameterInfo));
                    }
                }
            }
        }

        return constructor.Invoke(constructorValues.ToArray());
    }

    /// <summary>
    /// Runs checks on the <paramref name="checkable"/> and calls <see cref="SetImpossible(string)"/> if there are any failures
    /// </summary>
    /// <param name="checkable"></param>
    protected void SetImpossibleIfFailsChecks(ICheckable checkable)
    {
        try
        {
            checkable.Check(ThrowImmediatelyCheckNotifier.Quiet);
        }
        catch (Exception e)
        {
            SetImpossible(ExceptionHelper.ExceptionToListOfInnerMessages(e));
        }
    }

    public override string ToString() => GetCommandName();

    protected static CommentStore CreateCommentStore()
    {
        var help = new CommentStore();
        help.ReadComments(Environment.CurrentDirectory);
        return help;
    }

    /// <summary>
    /// Returns true if the supplied command Type is known (directly or via alias)
    /// as <paramref name="name"/>
    /// </summary>
    public static bool HasCommandNameOrAlias(Type commandType, string name)
    {
        return
            commandType.Name.Equals(ExecuteCommandPrefix + name, StringComparison.InvariantCultureIgnoreCase)
            ||
            commandType.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)
            ||
            commandType.GetCustomAttributes<AliasAttribute>(false)
                .Any(a => a.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
    }


    /// <summary>
    /// <para>
    /// Performs the <paramref name="toRun"/> action within a <see cref="Commit"/> (if
    /// commits are supported by platform).  Returns true if no commit was used or commit
    /// was completed successfully.  Returns false if commit was abandonned (e.g. by user cancelling).
    /// </para>
    /// <remarks> If commit is abandoned then <paramref name="trackObjects"/> will all be reverted
    /// to database state (i.e. local changes discarded)</remarks>
    /// </summary>
    /// <param name="toRun"></param>
    /// <param name="description"></param>
    /// <param name="trackObjects">Objects to do change tracking on within the transaction</param>
    /// <returns></returns>
    protected bool ExecuteWithCommit(Action toRun, string description,
        params IMapsDirectlyToDatabaseTable[] trackObjects)
    {
        CommitInProgress commit = null;
        var revert = false;

        if (BasicActivator.UseCommits())
            commit = new CommitInProgress(BasicActivator.RepositoryLocator, new CommitInProgressSettings(trackObjects)
            {
                UseTransactions = true,
                Description = description
            });

        try
        {
            toRun();

            // if user cancells transaction
            if (commit != null && commit.TryFinish(BasicActivator) == null) revert = true;
        }
        finally
        {
            commit?.Dispose();
        }

        if (revert)
            foreach (var o in trackObjects)
                if (o is IRevertable re)
                    re.RevertToDatabaseState();

        return !revert;
    }
}