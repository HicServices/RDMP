using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using ReusableLibraryCode.Exceptions;

namespace ReusableCodeTests
{
    public class ExpectedIdenticalStringsExceptionTests
    {

        [Test]
        public void CompletelyDifferentCase()
        {
            var ex = new ExpectedIdenticalStringsException("These are different", "fish", "egg");

            Assert.AreEqual(
@"These are different
Strings differ at index 0
EXPECTED:fish
ACTUAL  :egg
---------^", ex.Message);
        }

        [Test]
        public void ShortCase()
        {
            var ex = new ExpectedIdenticalStringsException("These are different", "fish", "fins");

            Assert.AreEqual(
@"These are different
Strings differ at index 2
EXPECTED:fish
ACTUAL  :fins
-----------^"
                , ex.Message);
        }


        [Test]
        public void MoreThanExpected()
        {
            var ex = new ExpectedIdenticalStringsException("These are different", "fish", "fish fly high");

            Assert.AreEqual(
@"These are different
Strings are identical except that Expected string ends at character 4 while the Actual string had 9 additional characters
EXPECTED:fish
ACTUAL  :fish fly high
-------------^"
                , ex.Message);
        }

        [Test]
        public void Whitespace()
        {
            var ex = new ExpectedIdenticalStringsException("These are different", @"fi
sh", "fish");

            Assert.AreEqual(
@"These are different
Strings differ at index 2
EXPECTED:fi\r\nsh
ACTUAL  :fish
-----------^"
                , ex.Message);
        }


        [Test]
        public void VeryLongStrings()
        {
            var ex = new ExpectedIdenticalStringsException("These are different",
@"Theosophists have guessed at the awesome grandeur of the cosmic cycle where our world and human race form transient incidents",
@"Theosophists have guessed at the awesome grandeur of the cosmic cycle wherein our world and human race form transient incidents");

            Assert.AreEqual(
@"These are different
Strings differ at index 75
EXPECTED:e cosmic cycle where our world...
ACTUAL  :e cosmic cycle wherein our wor...
-----------------------------^"
                , ex.Message);
        }

        [Test]
        public void VeryLongStringsWithWhitespace()
        {
            var ex = new ExpectedIdenticalStringsException("These are different",
@" For what could be the
meaning 
of the queer clay bas-relief and the disjointed jottings, ramblings, and cuttings which I found? Had
my uncle, in his latter years become credulous of the most superficial impostures? ",
@" For what could be the
meaning 
if the queer clay bas-relief and the disjointed jottings, ramblings, and cuttings which I found? Had
my uncle, in his latter years become credulous of the most superficial impostures? ");

            Assert.AreEqual(
@"These are different
Strings differ at index 38
EXPECTED: the\r\nmeaning \r\nof the que...
ACTUAL  : the\r\nmeaning \r\nif the que...
-----------------------------^"
                , ex.Message);
        }
    }
}
