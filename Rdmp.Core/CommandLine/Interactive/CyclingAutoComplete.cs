// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System.Collections.Generic;

namespace Rdmp.Core.CommandLine.Interactive
{

    /// <summary>
    /// Records forward/backward 'tabbing' through a list of commands
    /// </summary>
    public class CyclingAutoComplete
    {
        private List<string> _autoCompleteList;
        private string _previousAutoComplete = string.Empty;
        private int _autoCompleteIndex;
        
        public string AutoComplete(string line, List<string> strings, CyclingDirections cyclingDirection, bool ignoreCase)
        {
            if (IsPreviousCycle(line))
            {
                if (cyclingDirection == CyclingDirections.Forward)
                    return ContinueCycle();
                return ContinueCycleReverse();
            }

            _autoCompleteList = Interactive.AutoComplete.GetAutoCompletePossibilities(line, strings, ignoreCase);
            if (_autoCompleteList.Count == 0)
                return line;
            return StartNewCycle();
        }

        private string StartNewCycle()
        {
            _autoCompleteIndex = 0;
            var autoCompleteLine = _autoCompleteList[_autoCompleteIndex];
            _previousAutoComplete = autoCompleteLine;
            return autoCompleteLine;
        }

        private string ContinueCycle()
        {
            _autoCompleteIndex++;
            if (_autoCompleteIndex >= _autoCompleteList.Count)
                _autoCompleteIndex = 0;
            var autoCompleteLine = _autoCompleteList[_autoCompleteIndex];
            _previousAutoComplete = autoCompleteLine;
            return autoCompleteLine;
        }

        private string ContinueCycleReverse()
        {
            _autoCompleteIndex--;
            if (_autoCompleteIndex < 0)
                _autoCompleteIndex = _autoCompleteList.Count - 1;
            var autoCompleteLine = _autoCompleteList[_autoCompleteIndex];
            _previousAutoComplete = autoCompleteLine;
            return autoCompleteLine;
        }

        private bool IsPreviousCycle(string line)
        {
            return _autoCompleteList != null && _autoCompleteList.Count != 0 && _previousAutoComplete == line;
        }
    }
}
