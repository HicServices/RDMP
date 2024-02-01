using System;
using System.Linq;

namespace Rdmp.Core.ReusableLibraryCode;

public sealed class DebugHelper
{
    private static readonly Lazy<DebugHelper> lazy =
        new Lazy<DebugHelper>(() => new DebugHelper());

    public static DebugHelper Instance { get { return lazy.Value; } }

    private bool _debugMode { get; set; }

    private DebugHelper()
    {
        _debugMode = Environment.GetCommandLineArgs().Where(arg => arg == "--debug").Any();
    }

    public void DoIfInDebugMode(Action func)
    {
        if (_debugMode)
        {
            func();
        }
    }

    public bool IsInDebugMode { get { return _debugMode; } }
}
