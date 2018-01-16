using System;
using System.Collections.Generic;
using System.Linq;
using CatalogueLibrary.DataHelper;
using DataExportLibrary.Repositories;
using Diagnostics;
using NUnit.Framework;
using ReusableLibraryCode;
using ReusableLibraryCode.Checks;

namespace DataExportLibrary.Tests.Cohort
{
    [Category("Database")]
    public class CohortTests : TestsRequiringACohort
    {
     
        [Test]
        public void GetPrivateIdentifierValue()
        {
            Assert.AreEqual(RDMPQuerySyntaxHelper.EnsureValueIsWrapped(CohortDatabaseName)+"..[Cohort].[PrivateID]", _externalCohortTable.PrivateIdentifierField);
        }

        [Test]
        public void TestOverridingReleaseIdentifier()
        {
            //get cohort without override
            Assert.IsNull(_extractableCohort.OverrideReleaseIdentifierSQL);

            //should match global release identifier (from it's source because there is no override)
            Assert.AreEqual("ReleaseID", _extractableCohort.GetReleaseIdentifier(true));
            
            //appy override
            _extractableCohort.OverrideReleaseIdentifierSQL = "Fish";
            _extractableCohort.SaveToDatabase();
            
            //should now match override
            Assert.AreEqual("Fish", _extractableCohort.GetReleaseIdentifier());

            //now set it back to null (not overriding)
            _extractableCohort.OverrideReleaseIdentifierSQL = null;
            _extractableCohort.SaveToDatabase();

            //now check that we are back to the original release identifier
            Assert.AreEqual("ReleaseID", _extractableCohort.GetReleaseIdentifier(true));
            
        }

        [Test]
        public void CohortConfigurationChecker_AllFine()
        {
            RecordAllFailures failureRecorder = new RecordAllFailures();

            //run checker
            CohortConfigurationChecker checker = new CohortConfigurationChecker((DataExportRepository)DataExportRepository);
            checker.Check(failureRecorder);

            if (failureRecorder.FailureMessages.Any())
                Console.WriteLine(failureRecorder.FailureMessages.Aggregate("",(s,n)=>s+n));

            //make sure it didnt find any errors
            Assert.IsTrue(!failureRecorder.FailureMessages.Any());

        }

        [Test]
        public void TestSelf_RecordAllFailures()
        {
            RecordAllFailures failures = new RecordAllFailures();
            failures.FailureMessages.Add("Hi there Thomas, How's it going?");

            Assert.IsFalse(failures.AnyFailMessageLike("Carmageddon"));
            
            Assert.IsTrue(failures.AnyFailMessageLike("Thomas"));

            Assert.IsTrue(failures.AnyFailMessageLike("Thomas","going"));
            Assert.IsTrue(failures.AnyFailMessageLike("Thomas", "going", "Hi"));
            Assert.IsTrue(failures.AnyFailMessageLike("thomas", "gOIng", "hi"));

            Assert.IsFalse(failures.AnyFailMessageLike("Thomas", "going", "Hi","Fear the babadook"));

        }

        private class RecordAllFailures : ICheckNotifier
        {
            public RecordAllFailures()
            {
                FailureMessages = new List<string>();
            }
            public List<string> FailureMessages { get; set; }

            public bool AnyFailMessageLike(params string[] bitsTofind)
            {
                return FailureMessages.Any(m =>
                {
                    bool found = bitsTofind.Any();

                    foreach(string s in bitsTofind) 
                        if(!m.ToLower().Contains(s.ToLower()))
                            found = false;

                    return found;
                }
                   );
            }


            public bool OnCheckPerformed(CheckEventArgs args)
            {
                if(args.Result == CheckResult.Fail)
                    FailureMessages.Add(args.Message);

                //accept all proposed changes
                return true;
            }
        }

    }
}

