using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RDMPInstaller
{
    public partial class frmMain : Form
    {
        public string Url { get; set; }

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Url = "https://hic.dundee.ac.uk/installers/rdmp";
            var client = new WebClient();
            var installers = client.DownloadString(Url);
            var allLinks = Regex.Matches(installers, @"HREF=\""(/installers/rdmp/(\d+\.\d+\.\d+\.\d+(\-\w+)?)/)\""");

            var allVersions = new List<RDMPVersion>();

            foreach (Match link in allLinks)
            {
                allVersions.Add(new RDMPVersion
                {
                    Version = link.Groups[2].Value,
                    Link = "/" + link.Groups[2].Value + "/"
                });
            }

            ddlVersions.Items.Add(new RDMPVersion()
            {
                Link = "/Stable/",
                Version = "Latest"
            });

            try
            {
                ddlVersions.Items.AddRange(allVersions.ToArray());
                ddlVersions.ValueMember = "Link";
                ddlVersions.DisplayMember = "Version";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw;
            }
        }

        private void btnInstall_Click(object sender, EventArgs e)
        {
            var selected = ddlVersions.SelectedItem as RDMPVersion;
            if (selected != null)
                Installer.InstallApplication(Url + selected.Link + "ResearchDataManagementPlatform.application");
        }

    }

    internal class RDMPVersion : IComparable<RDMPVersion>
    {
        public string Version { get; set; }
        public string Link { get; set; }

        public bool IsReleaseCandidate { get { return Version.Contains("-"); } }

        public int CompareTo(RDMPVersion other)
        {
            var thisversion = IsReleaseCandidate ? new Version(Version.Split('-')[0]) : new Version(Version);
            var otherversion = other.IsReleaseCandidate ? new Version(other.Version.Split('-')[0]) : new Version(other.Version);

            if (IsReleaseCandidate)
                thisversion = new Version(thisversion.Major, thisversion.Minor, thisversion.Build, thisversion.Revision - 1);
            if (other.IsReleaseCandidate)
                otherversion = new Version(otherversion.Major, otherversion.Minor, otherversion.Build, otherversion.Revision - 1);

            var result = thisversion.CompareWith(otherversion, 4);
            if (result != 0)
                return result;

            if (this.IsReleaseCandidate)
                return -1;

            return 1;
        }
    }

    public static class VersionExtensions
    {
        public static int CompareWith(this Version version, Version otherVersion, int significantParts)
        {
            if (version == null)
            {
                throw new ArgumentNullException("version");
            }
            if (otherVersion == null)
            {
                return 1;
            }

            if (version.Major != otherVersion.Major && significantParts >= 1)
                if (version.Major > otherVersion.Major)
                    return 1;
                else
                    return -1;

            if (version.Minor != otherVersion.Minor && significantParts >= 2)
                if (version.Minor > otherVersion.Minor)
                    return 1;
                else
                    return -1;

            if (version.Build != otherVersion.Build && significantParts >= 3)
                if (version.Build > otherVersion.Build)
                    return 1;
                else
                    return -1;

            if (version.Revision != otherVersion.Revision && significantParts >= 4)
                if (version.Revision > otherVersion.Revision)
                    return 1;
                else
                    return -1;

            return 0;
        }
    }
}
