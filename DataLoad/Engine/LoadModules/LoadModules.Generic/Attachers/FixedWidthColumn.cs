namespace LoadModules.Generic.Attachers
{
    public struct FixedWidthColumn
    {
        //order of these must match the order in the flat file!
        public int From;
        public int To;
        public string Field;
        public int Size;
        public string DateFormat;
    }
}