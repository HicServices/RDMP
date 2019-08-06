using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using NUnit.Framework;
using ReusableLibraryCode.Comments;

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
