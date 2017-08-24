namespace ReusableUIComponents
{
    public class BetterListBoxScrollArgs
    {
        // Scroll event argument
        private int mTop;
        private bool mTracking;
        public BetterListBoxScrollArgs(int top, bool tracking)
        {
            mTop = top;
            mTracking = tracking;
        }
        public int Top
        {
            get { return mTop; }
        }
        public bool Tracking
        {
            get { return mTracking; }
        }
    }
}