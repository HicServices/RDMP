using System.Windows.Forms;

namespace ReusableUIComponents.SingleControlForms
{
    public interface IConsultableBeforeClosing
    {
        void ConsultAboutClosing(object sender, FormClosingEventArgs e);
    }
}