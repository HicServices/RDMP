// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode.Exceptions;

namespace Rdmp.Core.Tests.ReusableLibraryCode
{
    [Category("Unit")]
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

            
            
            string eol = Environment.NewLine.Replace("\r","\\r").Replace("\n","\\n");


            Assert.AreEqual(
$@"These are different
Strings differ at index 2
EXPECTED:fi{eol}sh
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

            //line endings change the location in the string that is different.  It also means more preview
            //text is available since newline characters consume more/less of the preview allowance.
            if (Environment.NewLine == "\r\n")
            {
                Assert.AreEqual(
                    $@"These are different
Strings differ at index 38
EXPECTED: the\r\nmeaning \r\nof the que...
ACTUAL  : the\r\nmeaning \r\nif the que...
-----------------------------^"
                    , ex.Message);
            }
            else
            {
                Assert.AreEqual(
                    $@"These are different
Strings differ at index 34
EXPECTED:d be the\nmeaning \nof the que...
ACTUAL  :d be the\nmeaning \nif the que...
-----------------------------^"
                    , ex.Message);
            }

            
        }
    }
}
