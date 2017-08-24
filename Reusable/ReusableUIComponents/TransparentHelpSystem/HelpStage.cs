using System;
using System.Drawing;
using System.Windows.Forms;


namespace ReusableUIComponents.TransparentHelpSystem
{
    public class HelpStage
    {
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