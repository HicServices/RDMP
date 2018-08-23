using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using HIC.Common.Validation.Constraints.Primary;
using NUnit.Framework;

namespace HIC.Common.Validation.Tests
{
    public class ValidationDeserializationMemoryTest
    {
        [Test]
        public void TestMemoryLeak()
        {
            Validator v = new Validator();
            v.ItemValidators.Add(new ItemValidator("CHI"){PrimaryConstraint = new Chi()});
            string xml = v.SaveToXml();

            long bytesAtStart = Process.GetCurrentProcess().WorkingSet64;

            for (int i = 0; i < 1000; i++)
            {
                Validator deser = Validator.LoadFromXml(xml);
                
                if(i%500==0)
                {
                    GC.Collect();
                    Console.WriteLine("Commited Bytes:" + Process.GetCurrentProcess().WorkingSet64);
                }
            }

            long bytesAtEnd = Process.GetCurrentProcess().WorkingSet64;

            Assert.Less(bytesAtEnd,bytesAtStart * 2 , "Should not be using double the working memory as many bytes by the end, at start we were using " + bytesAtStart + " at end we were using " + bytesAtEnd + " (Increase of " +((float)bytesAtEnd/bytesAtStart) + " times)");

        }

    }
}
