// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.Cohort;

[Category("Database")]
public class CohortTests : TestsRequiringACohort
{
    [Test]
    public void TestOverridingReleaseIdentifier()
    {
        Assert.Multiple(() =>
        {
            //get cohort without override
            Assert.That(_extractableCohort.OverrideReleaseIdentifierSQL, Is.Null);

            //should match global release identifier (from its source because there is no override)
            Assert.That(_extractableCohort.GetReleaseIdentifier(true), Is.EqualTo("ReleaseID"));
        });

        //apply override
        _extractableCohort.OverrideReleaseIdentifierSQL = "Fish";
        _extractableCohort.SaveToDatabase();

        //should now match override
        Assert.That(_extractableCohort.GetReleaseIdentifier(), Is.EqualTo("Fish"));

        //now set it back to null (not overriding)
        _extractableCohort.OverrideReleaseIdentifierSQL = null;
        _extractableCohort.SaveToDatabase();

        //now check that we are back to the original release identifier
        Assert.That(_extractableCohort.GetReleaseIdentifier(true), Is.EqualTo("ReleaseID"));
    }

    [Test]
    public void TestSelf_RecordAllFailures()
    {
        var failures = new RecordAllFailures();
        failures.FailureMessages.Add("Hi there Thomas, How's it going?");

        Assert.Multiple(() =>
        {
            Assert.That(failures.AnyFailMessageLike("Carmageddon"), Is.False);

            Assert.That(failures.AnyFailMessageLike("Thomas"));

            Assert.That(failures.AnyFailMessageLike("Thomas", "going"));
            Assert.That(failures.AnyFailMessageLike("Thomas", "going", "Hi"));
            Assert.That(failures.AnyFailMessageLike("thomas", "gOIng", "hi"));

            Assert.That(failures.AnyFailMessageLike("Thomas", "going", "Hi", "Fear the babadook"), Is.False);
        });
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
                    var found = bitsTofind.Any();

                    foreach (var s in bitsTofind)
                        if (!m.ToLower().Contains(s.ToLower()))
                            found = false;

                    return found;
                }
            );
        }


        public bool OnCheckPerformed(CheckEventArgs args)
        {
            if (args.Result == CheckResult.Fail)
                FailureMessages.Add(args.Message);

            //accept all proposed changes
            return true;
        }
    }
}