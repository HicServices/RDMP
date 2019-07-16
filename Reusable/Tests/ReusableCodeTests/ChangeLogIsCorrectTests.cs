using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ReusableCodeTests
{
    class ChangeLogIsCorrectTests
    {
        [TestCase("../../../../../../CHANGELOG.md")]
        public void TestChangeLogContents(string changeLogPath)
        {
            if (changeLogPath != null && !Path.IsPathRooted(changeLogPath))
                changeLogPath = Path.Combine(TestContext.CurrentContext.TestDirectory, changeLogPath);

            if (!File.Exists(changeLogPath))
                Assert.Fail($"Could not find file {changeLogPath}");

            var fi = new FileInfo(changeLogPath);

            var assemblyInfo = Path.Combine(fi.Directory.FullName, "SharedAssemblyInfo.cs");

            if (!File.Exists(assemblyInfo))
                Assert.Fail($"Could not find file {assemblyInfo}");

            var match = Regex.Match(File.ReadAllText(assemblyInfo), @"AssemblyInformationalVersion\(""(.*)""\)");
            Assert.IsTrue(match.Success, $"Could not find AssemblyInformationalVersion tag in {assemblyInfo}");

            string currentVersion = match.Groups[1].Value;

            var changeLog = File.ReadAllText(changeLogPath);

            Assert.IsTrue(changeLog.Contains($"## [{currentVersion}]"), $"{changeLogPath} did not contain a header for the current version '{currentVersion}'");

        }
    }
}