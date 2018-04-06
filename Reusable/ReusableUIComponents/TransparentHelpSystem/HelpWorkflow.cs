using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ReusableLibraryCode.CommandExecution;
using ReusableUIComponents.TransparentHelpSystem.ProgressTracking;

namespace ReusableUIComponents.TransparentHelpSystem
{
    public class HelpWorkflow
    {
        public Guid WorkflowGuid { get; private set; }
        public Control HostControl { get; private set; }
        public ICommandExecution Command { get; private set; }
        public IHelpWorkflowProgressProvider ProgressProvider { get; set; }

        private TransparentHelpForm _help;
        private bool _helpClosed;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        public HelpStage RootStage { get; set; }
        public HelpStage CurrentStage { get; set; }
        
        public HelpWorkflow(Control hostControl, Guid workflowGuid, IHelpWorkflowProgressProvider progressProvider):this(hostControl,null,progressProvider)
        {
            WorkflowGuid = workflowGuid;
        }

        public HelpWorkflow(Control hostControl, ICommandExecution command, IHelpWorkflowProgressProvider progressProvider)
        {
            HostControl = hostControl;
            Command = command;
            ProgressProvider = progressProvider;
        }

        /// <summary>
        /// Restarts the HelpWorkflow
        /// </summary>
        public void Start(bool force = false)
        {
            if (RootStage == null)
                throw new Exception("No RootStage exists for Help, you will need to create one");
            
            if(!force && !ProgressProvider.ShouldShowUserWorkflow(this))
                return;
            
            _help = new TransparentHelpForm(HostControl);
            _help.ShowWithoutActivate();
            _helpClosed = false;
            _help.FormClosed += (sender, args) =>
            {
                _helpClosed = true;
                _cancellationTokenSource.Cancel();
            };
            
            ShowStage(RootStage);
        }

        public void ShowStage(HelpStage stage)
        {
            //Abandon must have already been called
            if (_help == null)
                return;

            if (HostControl.InvokeRequired)
            {
                HostControl.Invoke(new MethodInvoker(()=>ShowStage(stage)));
                return;
            }

            CurrentStage = stage;
            
            var helpBox = _help.ShowStage(this,CurrentStage);
            helpBox.OptionTaken += () => ShowStage(CurrentStage.OptionDestination);
            
            var t = stage.Await(_cancellationTokenSource.Token);
            t.ContinueWith(r =>
            {
                if(r.IsFaulted || r.IsCanceled)
                    Abandon();
                else
                    try
                    {
                        if(r.Result)
                            ShowNextStageOrClose();
                    }
                    catch (Exception)
                    {
                        Abandon();
                    }
            });
        }

        public bool ShowNextStageOrClose()
        {
            //Abandon must have already been called
            if (_help == null)
                return false;

            if (HostControl.InvokeRequired)
                return (bool) HostControl.Invoke(new Func<bool>(ShowNextStageOrClose));

            //if there is a next stage and help hasn't been closed
            if (CurrentStage != null && CurrentStage.Next != null && !_helpClosed)
            {
                ShowStage(CurrentStage.Next);
                return true;
            }
            
            _help.Close();
            return false;
        }

        /// <summary>
        /// Ends the current help session (cannot be reversed)
        /// </summary>
        public void Abandon()
        {
            if(_help == null)
                return;

            if (HostControl.InvokeRequired)
            {
                HostControl.Invoke(new MethodInvoker(Abandon));
                return;
            }
            ProgressProvider.Completed(this);
            _help.Close();
            _help = null;
        }
    }
}
