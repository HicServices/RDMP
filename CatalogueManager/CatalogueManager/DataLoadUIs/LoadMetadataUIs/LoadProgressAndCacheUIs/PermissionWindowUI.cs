using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;


namespace CatalogueManager.DataLoadUIs.LoadMetadataUIs.LoadProgressAndCacheUIs
{
    /// <summary>
    /// Restricts the times of day in which caching can take place (e.g. from midnight-4am only).  For a description of what caching is see CacheProgressUI or the RDMP user manual.  Format is
    /// in standard TimeSpan.TryParase format (see https://msdn.microsoft.com/en-us/library/3z48198e(v=vs.110).aspx or search online for 'TimeSpan.TryParse c#') each TimeSpan can be followed by
    /// a comma and then another TimeSpan format e.g.  '10:20:00-10:40:00,11:20:00-11:40:00' would create a permission window that could download from the cache between 10:20 AM and 10:40 AM then
    /// caching wouldn't be allowed again until 11:20am to 11:40am.
    /// </summary>
    public partial class PermissionWindowUI : Form
    {
        private IPermissionWindow _permissionWindow;

        public PermissionWindowUI()
        {
            InitializeComponent();
        }

        public void SetPermissionWindow(IPermissionWindow permissionWindow)
        {
            _permissionWindow = permissionWindow;

            tbName.Text = _permissionWindow.Name;
            tbDescription.Text = _permissionWindow.Description;

            var periods = _permissionWindow.PermissionWindowPeriods;
            var periodsByDay = new Dictionary<int, List<PermissionWindowPeriod>>();
            foreach (var period in periods)
            {
                if (!periodsByDay.ContainsKey(period.DayOfWeek))
                    periodsByDay.Add(period.DayOfWeek, new List<PermissionWindowPeriod>());

                periodsByDay[period.DayOfWeek].Add(period);
            }

            var textBoxes = new[] {tbSunday, tbMonday, tbTuesday, tbWednesday, tbThursday, tbFriday, tbSaturday};
            for (var i = 0; i < 7; ++i)
                PopulatePeriodTextBoxForDay(textBoxes[i], i, periodsByDay);
        }

        private void PopulatePeriodTextBoxForDay(TextBox textBox, int dayNum, Dictionary<int, List<PermissionWindowPeriod>> periodsByDay)
        {
            if (periodsByDay.ContainsKey(dayNum)) PopulateTextBox(textBox, periodsByDay[dayNum]);
        }

        private void PopulateTextBox(TextBox textBox, IEnumerable<PermissionWindowPeriod> periods)
        {
            textBox.Text = string.Join(",", periods.Select(period => period.ToString()));
        }

        private List<PermissionWindowPeriod> CreatePeriodListFromTextBox(int dayOfWeek, TextBox textBox)
        {
            var listString = textBox.Text;
            var periodList = new List<PermissionWindowPeriod>();
            foreach (var periodString in listString.Split(new[] {","}, StringSplitOptions.RemoveEmptyEntries))
            {
                var parts = periodString.Split(new[] {"-"}, StringSplitOptions.RemoveEmptyEntries);
                TimeSpan start;
                if (!TimeSpan.TryParse(parts[0], out start))
                    throw new Exception("Could not parse " + parts[0] + " as a TimeSpan");

                TimeSpan end;
                if (!TimeSpan.TryParse(parts[1], out end))
                    throw new Exception("Could not parse " + parts[1] + " as a TimeSpan");

                periodList.Add(new PermissionWindowPeriod(dayOfWeek, start, end));
            }

            return periodList;
        }

        private void btnSaveAndClose_Click(object sender, EventArgs e)
        {
            _permissionWindow.Name = tbName.Text;
            _permissionWindow.Description = tbDescription.Text;

            var periodList = new List<PermissionWindowPeriod>();
            periodList.AddRange(CreatePeriodListFromTextBox(0, tbSunday));
            periodList.AddRange(CreatePeriodListFromTextBox(1, tbMonday));
            periodList.AddRange(CreatePeriodListFromTextBox(2, tbTuesday));
            periodList.AddRange(CreatePeriodListFromTextBox(3, tbWednesday));
            periodList.AddRange(CreatePeriodListFromTextBox(4, tbThursday));
            periodList.AddRange(CreatePeriodListFromTextBox(5, tbFriday));
            periodList.AddRange(CreatePeriodListFromTextBox(6, tbSaturday));

            _permissionWindow.PermissionWindowPeriods = periodList;
            _permissionWindow.SaveToDatabase();

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
