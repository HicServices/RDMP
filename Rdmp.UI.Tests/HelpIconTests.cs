using NUnit.Framework;
using ReusableUIComponents;

namespace Rdmp.UI.Tests
{
    class HelpIconTests
    {
        [Test]
        public void TestNullInputs_HelpIcon()
        {
            var hi = new HelpIcon();
            hi.SetHelpText(null,null);
            hi.SetHelpText("","");
            Assert.IsNull(hi.HoverText);
        }

        [Test]
        public void TestLongInputs_HelpIcon()
        {
            var hi = new HelpIcon();

            //length is over 150 characters
            string testLongString = "kdsfldsfjsdafdfjsdafldsafadsfksdafjdfjdsfasdjfdsjfsdfldsjfkdsfkdsfksdafjdfsdaf;sdafsdafadsflsdafksdfjadslfjdsflsdjfldsfksadkfadkfasdfadsjfasdsdfladsfjsdjfkdflsdfksdfkadsfladsfj";
            hi.SetHelpText(null,testLongString);
            Assert.AreEqual(HelpIcon.MaxHoverTextLength,hi.HoverText.Length);
        }
    }
}