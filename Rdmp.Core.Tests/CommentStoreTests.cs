// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Xml;
using NUnit.Framework;
using Rdmp.Core.ReusableLibraryCode.Comments;

namespace Rdmp.Core.Tests
{
    class CommentStoreTests
    {
        [Test]
        public void Test_CommentStoreXmlDoc_Basic()
        {
            var store = new CommentStore();
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(
                @" <member name=""T:ResearchDataManagementPlatform.WindowManagement.WindowFactory"">
                <summary>
                Does some stuff 
                </summary>
                </member>");

            store.AddXmlDoc(doc.FirstChild);

            Assert.AreEqual(
                @"Does some stuff"
                ,store["WindowFactory"]);

        }

        [Test]
        public void Test_CommentStoreXmlDoc_OnePara()
        {
            var store = new CommentStore();
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(
                @" <member name=""T:ResearchDataManagementPlatform.WindowManagement.WindowFactory"">
                <summary>
                <para>
                Does some stuff 
                </para>
                </summary>
                </member>");

            store.AddXmlDoc(doc.FirstChild);

            Assert.AreEqual(
                @"Does some stuff"
                ,store["WindowFactory"]);

        }

        [Test]
        public void Test_CommentStoreXmlDoc_TwoPara()
        {
            var store = new CommentStore();
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(
                @" <member name=""T:ResearchDataManagementPlatform.WindowManagement.WindowFactory"">
                <summary>
                Does some stuff 
                This is still one para
                <para>
                this is next para
                </para>
                </summary>
                </member>");

            store.AddXmlDoc(doc.FirstChild);

            Assert.AreEqual(
                @"Does some stuff This is still one para

this is next para"
                ,store["WindowFactory"]);

        }


        [Test]
        public void Test_CommentStoreXmlDoc_EmptyElements()
        {
            var store = new CommentStore();
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(
                @" <member name=""T:ResearchDataManagementPlatform.WindowManagement.WindowFactory"">
                <summary></summary>
                </member>");

            //shouldn't bomb
            store.AddXmlDoc(null);
            //also shouldn't bomb but should be 0
            store.AddXmlDoc(doc.FirstChild.FirstChild);
            
            Assert.IsEmpty(store);

            store.AddXmlDoc(doc.FirstChild);
            Assert.IsEmpty(store);

            doc.LoadXml(
                @" <member name=""T:ResearchDataManagementPlatform.WindowManagement.WindowFactory"">
                <summary>  </summary>
                </member>");

            store.AddXmlDoc(doc.FirstChild);
            Assert.IsEmpty(store);

            
            doc.LoadXml(
                @" <member name=""T:ResearchDataManagementPlatform.WindowManagement.WindowFactory"">
                <summary> a </summary>
                </member>");

            store.AddXmlDoc(doc.FirstChild);
            Assert.IsNotEmpty(store);
        }


        [Test]
        public void Test_CommentStoreXmlDoc_TwoParaBothFormatted()
        {
            var store = new CommentStore();
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(
                @" <member name=""T:ResearchDataManagementPlatform.WindowManagement.WindowFactory"">
                <summary>
                <para>
                Does some stuff 
                This is still one para
                </para>
                <para>
                this is next para
                </para>
                </summary>
                </member>");

            store.AddXmlDoc(doc.FirstChild);

            Assert.AreEqual(
                @"Does some stuff This is still one para

this is next para"
                ,store["WindowFactory"]);

        }


        [Test]
        public void Test_CommentStoreXmlDoc_WithCrefAndTwoPara()
        {
            var store = new CommentStore();
            
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(
                @" <member name=""T:ResearchDataManagementPlatform.WindowManagement.WindowFactory"">
                <summary>
                Does some stuff 
                And some more stuff
            IObjectCollectionControl (for <see cref=""T:Rdmp.UI.Collections.RDMPCollectionUI""/> see <see cref=""T:ResearchDataManagementPlatform.WindowManagement.WindowManager""/>).

<para> paragraph 2
got it?
</para>
                </summary>
                <exception cref=""T:System.ArgumentOutOfRangeException"">This text shouldn't appear</exception>
                </member>");

            store.AddXmlDoc(doc.FirstChild);

            Assert.AreEqual(
                @"Does some stuff And some more stuff IObjectCollectionControl (for RDMPCollectionUI see WindowManager ).

paragraph 2 got it?"
                ,store["WindowFactory"]);

        }
    }
}
