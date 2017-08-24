namespace HIC.Common.Validation.Tests.TestData
{

    public class ChiAgeDomainObject
    {
        public string chi { get; set; }
        public int age { get; set; }

        public ChiAgeDomainObject(string chi, int age)
        {
            this.chi = chi;
            this.age = age;
        }

    }
}