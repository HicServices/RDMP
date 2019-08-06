using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ReusableLibraryCode.Comments
{
    public static class XmlDocumentExtensions
    {
        public static void IterateThroughAllNodes(
            this XmlDocument doc, 
            Action<XmlNode> elementVisitor)
        {
            if (doc != null && elementVisitor != null)
            {
                foreach (XmlNode node in doc.ChildNodes)
                {
                    doIterateNode(node, elementVisitor);
                }
            }
        }

        public static void IterateThroughAllNodes(
            this XmlNode node, 
            Action<XmlNode> elementVisitor)
        {
            
            foreach (XmlNode c in node.ChildNodes)
            {
                doIterateNode(c, elementVisitor);
            }
        
        }

        private static void doIterateNode(
            XmlNode node, 
            Action<XmlNode> elementVisitor)
        {
            elementVisitor(node);

            foreach (XmlNode childNode in node.ChildNodes)
            {
                doIterateNode(childNode, elementVisitor);
            }
        }
    }
}
