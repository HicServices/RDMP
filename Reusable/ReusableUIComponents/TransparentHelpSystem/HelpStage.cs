using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BrightIdeasSoftware;
using EnvDTE;


namespace ReusableUIComponents.TransparentHelpSystem
{
    public class HelpStage
    {
        private readonly Func<bool> _moveOnWhenConditionMet;
        private readonly int _pollMilliseconds;
        public readonly Control HighlightControl;
        public readonly string HelpText;

        /// <summary>
        /// If true then HostLocationForStageBox will be ignored and the positioning of hte HelpBox will be decided based on the location of the highlighted control and the
        /// surrounding available placement space on the host Form.
        /// </summary>
        public bool UseDefaultPosition { get; set; }
        public readonly Point HostLocationForStageBox;

        public string OptionButtonText;
        public HelpStage OptionDestination;

        public HelpStage Next;


        public HelpStage(Control highlightControl, string helpText, params HelpStage[] nextStagesInOrder)
        {
            HighlightControl = highlightControl;
            HelpText = helpText;
            UseDefaultPosition = true;

            HandleParams(nextStagesInOrder);
        }

        public HelpStage(Control highlightControl, string helpText, Point hostLocationForStageBox, params HelpStage[] nextStagesInOrder)
        {
            HighlightControl = highlightControl;
            HelpText = helpText;
            HostLocationForStageBox = hostLocationForStageBox;
            UseDefaultPosition = false;

            HandleParams(nextStagesInOrder);
        }

        public HelpStage(Control highlightControl, string helpText, Func<bool> moveOnWhenConditionMet,int pollMilliseconds = 300, params HelpStage[] nextStagesInOrder):this(highlightControl,helpText,nextStagesInOrder)
        {
            _moveOnWhenConditionMet = moveOnWhenConditionMet;
            _pollMilliseconds = pollMilliseconds;
        }

        /// <summary>
        /// If there is moveOnWhenConditionMet set up in this HelpStage then this will start polling for the condition.  If the condition is met before cancellation true
        /// is returned (i.e. show the next stage).  Returns false if there is no moveOnCondition set up or the CancellationToken is cancelled.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> Await(CancellationToken token)
        {
            if(_moveOnWhenConditionMet == null)
                return false;

           while (!token.IsCancellationRequested && !_moveOnWhenConditionMet())
                await Task.Delay(_pollMilliseconds,token);

            return !token.IsCancellationRequested;
        }

        private void HandleParams(HelpStage[] nextStagesInOrder)
        {
            for (int i = 0; i < nextStagesInOrder.Length; i++)
                if (i == 0)
                    Next = nextStagesInOrder[i];
                else
                    nextStagesInOrder[i - 1].Next = nextStagesInOrder[i];
        }

        public void SetOption(string optionButtonText, HelpStage destinationWhenOptionTaken)
        {
            OptionButtonText = optionButtonText;
            OptionDestination = destinationWhenOptionTaken;
        }

        public HelpStage SetNext(HelpStage next)
        {
            Next = next;
            return next;
        }

        public override string ToString()
        {
            var textToShow = HelpText;

            if (HelpText.Length > 47)
                textToShow = HelpText.Substring(0, 47) + "...";

            return "Help Stage:"  + textToShow;
        }
    }
}