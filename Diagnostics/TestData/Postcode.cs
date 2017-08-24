namespace Diagnostics.TestData
{
    public class Postcode
    {
        public string Value { get; set; }
        public string Ward { get; set; }
        public string District { get; set; }

        public Postcode( string value, string ward, string district)
        {
            Value = value;
            Ward = ward;
            District = district;
        }
    }
}