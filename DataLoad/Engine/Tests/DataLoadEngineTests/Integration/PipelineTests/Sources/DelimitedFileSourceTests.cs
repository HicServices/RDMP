using System;
using System.Data;
using System.IO;
using System.Text;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataLoadEngine.Job;
using LoadModules.Generic.DataFlowSources;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace DataLoadEngineTests.Integration.PipelineTests.Sources
{
    public class DelimitedFileSourceTests
    {
        private const string filename = "DelimitedFileSourceTests.txt";

        private FileInfo CreateTestFile()
        {
            if(File.Exists(filename))
                File.Delete(filename);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("CHI,StudyID,Date");
            sb.AppendLine("0101010101,5,2001-01-05");

            File.WriteAllText(filename, sb.ToString());

            return new FileInfo(filename);
        }

        [Test]
        [ExpectedException(ExpectedMessage = "_fileToLoad was not set",MatchType=MessageMatch.Contains)]
        public void FileToLoadNotSet_Throws()
        {
            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            DataTable chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        }
        [Test]
        [ExpectedException(ExpectedMessage = "Separator has not been set", MatchType = MessageMatch.Contains)]
        public void SeparatorNotSet_Throws()
        {
            FileInfo testFile = CreateTestFile();
            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(testFile),new ThrowImmediatelyDataLoadEventListener() );
            source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
        }
        [Test]
        public void LoadCSVWithCorrectDatatypes_DatatypesAreCorrect()
        {

            FileInfo testFile = CreateTestFile();
            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
            source.Separator = ",";
            source.StronglyTypeInput = true;//makes the source interpret the file types properly

            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());

            Assert.AreEqual(3,chunk.Columns.Count);
            Assert.AreEqual(1, chunk.Rows.Count);
            Assert.AreEqual("0101010101", chunk.Rows[0][0]);
            Assert.AreEqual(5, chunk.Rows[0][1]);
            Assert.AreEqual(new DateTime(2001 , 1 , 5), chunk.Rows[0][2]);//notice the strong typing (we are not looking for strings here)
            
            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }

        [Test]
        public void OverrideDatatypes_ForcedFreakyTypesCorrect()
        {

            FileInfo testFile = CreateTestFile();
            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
            source.Separator = ",";
            source.StronglyTypeInput = true;//makes the source interpret the file types properly
            
            source.ExplicitlyTypedColumns = new ExplicitTypingCollection();
            source.ExplicitlyTypedColumns.ExplicitTypesCSharp.Add("StudyID",typeof(string));

            //preview should be correct
            DataTable preview = source.TryGetPreview();
            Assert.AreEqual(typeof(string), preview.Columns["StudyID"].DataType);
            Assert.AreEqual("5", preview.Rows[0]["StudyID"]);

            //as should live run
            var chunk = source.GetChunk(new ThrowImmediatelyDataLoadEventListener(), new GracefulCancellationToken());
            Assert.AreEqual(typeof(string), chunk.Columns["StudyID"].DataType);
            Assert.AreEqual("5", chunk.Rows[0]["StudyID"]);

            source.Dispose(new ThrowImmediatelyDataLoadEventListener(), null);
        }

        [Test]
        [TestCase(null)]
        [TestCase(BehaviourOnUnderReadType.Ignore)]
        [TestCase(BehaviourOnUnderReadType.AppendNextLineToCurrentRow)]
        public void OverReadBehaviour(BehaviourOnUnderReadType? behaviour)
        {
            if (File.Exists(filename))
                File.Delete(filename);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("CHI,StudyID,Date");
            sb.AppendLine("0101010101,5,2001-01-05");
            sb.AppendLine("0101010101,5,2001-01-05");
            sb.AppendLine("0101010101,5,2001-01-05,fish,watafak");
            sb.AppendLine("0101010101,5,2001-01-05");
            sb.AppendLine("0101010101,5,2001-01-05");

            File.WriteAllText(filename, sb.ToString());

            var testFile = new FileInfo(filename);

            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
            source.Separator = ",";

            if(behaviour.HasValue)
                source.UnderReadBehaviour = behaviour.Value;

            source.StronglyTypeInput = true;//makes the source interpret the file types properly

            var ex = Assert.Throws<FileLoadException>(() => source.TryGetPreview());

            Assert.AreEqual("Buffer overrun on line 4 of file 'DelimitedFileSourceTests', it has too many columns (expected 3 columns but line had 5).",ex.Message);

            

        }

        [Test]
        public void OverrideHeadersAndTab()
        {
            if (File.Exists(filename))
                File.Delete(filename);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("0101010101\t5\t2001-01-05");
            sb.AppendLine("0101010101\t5\t2001-01-05");
            File.WriteAllText(filename,sb.ToString());


            var testFile = new FileInfo(filename);

            DelimitedFlatFileDataFlowSource source = new DelimitedFlatFileDataFlowSource();
            source.PreInitialize(new FlatFileToLoad(testFile), new ThrowImmediatelyDataLoadEventListener());
            source.Separator = "\\t"; //<-- Important this is the string value SLASH T not an actual escaped tab as C# understands it.  This reflects the user pressing slash and t on his keyboard for the Separator argument in the UI
            source.ForceHeaders = "CHI\tStudyID\tDate";

            var dt = source.GetChunk(new ThrowImmediatelyDataLoadJob(), new GracefulCancellationToken());

            Assert.NotNull(dt);

            Assert.AreEqual(3,dt.Columns.Count);

            Assert.AreEqual("CHI", dt.Columns[0].ColumnName);
            Assert.AreEqual("StudyID", dt.Columns[1].ColumnName);
            Assert.AreEqual("Date", dt.Columns[2].ColumnName);

            Assert.AreEqual(2,dt.Rows.Count);

            source.Dispose(new ThrowImmediatelyDataLoadJob(), null);

            File.Delete(filename);
        }

        [TestCase("Fish In Barrel", "FishInBarrel")]
        [TestCase("32 Fish In Barrel","_32FishInBarrel")]//Column names can't start with numbers so underscore prefix applies
        [TestCase("once upon a time","onceUponATime")]//where spaces are removed cammel case the next symbol if it's a character
        [TestCase("once _  upon a time", "once_UponATime")]//where spaces are removed cammel case the next symbol if it's a character
        [TestCase("once#upon a", "onceuponA")]
        [TestCase("once #upon", "onceUpon")] //Dodgy characters are stripped before cammel casing after spaces so 'u' gets cammeled even though it has a symbol before it.
        public void TestMakingHeaderNamesSane(string bad, string expectedGood)
        {
            Assert.AreEqual(expectedGood,DelimitedFlatFileDataFlowSource.MakeHeaderNameSane(bad));
        }

    }
}
