using System;

namespace Diagnostics.TestData
{
    public class TestAppointment
    {
        public string Identifier { get; set; }
        public DateTime StartDate { get; set; }

        public TestAppointment(TestPerson testPerson, Random r)
        {
            Identifier = "APPT_" + Guid.NewGuid();
            StartDate = testPerson.GetRandomDateDuringLifetime(r);
        }
    }
}