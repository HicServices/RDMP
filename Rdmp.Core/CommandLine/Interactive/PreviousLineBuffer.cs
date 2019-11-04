// This code is adapted from https://www.codeproject.com/Articles/1182358/Using-Autocomplete-in-Windows-Console-Applications

using System.Collections.Generic;

namespace Rdmp.Core.CommandLine.Interactive
{
    /// <summary>
    /// Records previously typed lines that the user has entered into the console
    /// </summary>
    public class PreviousLineBuffer
    {
        private bool _cyclingStarted;
        private readonly List<string> _previousLines = new List<string>();

        public bool HasLines { get { return _previousLines.Count > 0; } }
        public string LastLine { get { return _previousLines.Count == 0 ? null : _previousLines[_previousLines.Count - 1]; } }
        public string LineAtIndex { get { return _previousLines.Count == 0 ? null : _previousLines[Index]; } }

        public int Index { get; set; }

        public List<string> PreviousLines
        {
            get { return _previousLines; }
        }

        public void AddLine(string line)
        {
            if (!string.IsNullOrEmpty(line))
                _previousLines.Add(line);
            if (_previousLines.Count > 0 && _previousLines[Index] != line)
                Index = _previousLines.Count - 1;
            _cyclingStarted = false;
        }

        public bool CycleUp()
        {
            if (!HasLines)
                return false;
            if (!_cyclingStarted)
            {
                _cyclingStarted = true;
                return true;
            }
            if (Index > 0)
            {
                Index--;
                return true;
            }
            return false;
        }

        public void CycleUpAndAround()
        {
            if (!HasLines)
                return;
            if (!_cyclingStarted)
            {
                _cyclingStarted = true;
                return;
            }
            Index--;
            if (Index < 0)
                Index = _previousLines.Count - 1;
        }

        public bool CycleDown()
        {
            if (!HasLines)
                return false;
            if (Index >= _previousLines.Count - 1)
                return false;
            Index++;
            return true;
        }

        public bool CycleTop()
        {
            if (!HasLines || Index == 0)
                return false;
            Index = 0;
            return true;
        }

        public bool CycleBottom()
        {
            if (!HasLines || Index >= _previousLines.Count - 1)
                return false;
            Index = _previousLines.Count - 1;
            return true;
        }
    }
}
