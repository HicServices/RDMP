namespace ReusableUIComponents
{
    public interface IKnowIfImHostedByVisualStudio
    {
        bool VisualStudioDesignMode { get;}
        void SetVisualStudioDesignMode(bool visualStudioDesignMode);
    }
}