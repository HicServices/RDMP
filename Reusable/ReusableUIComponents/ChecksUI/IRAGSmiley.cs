using System;
using ReusableLibraryCode.Checks;

namespace ReusableUIComponents.ChecksUI
{
    public interface IRAGSmiley : ICheckNotifier
    {
        bool IsGreen();
        bool IsFatal();
        void Warning(Exception ex);
        void Fatal(Exception ex);

        void Reset();
        void StartChecking(ICheckable checkable);
    }
}