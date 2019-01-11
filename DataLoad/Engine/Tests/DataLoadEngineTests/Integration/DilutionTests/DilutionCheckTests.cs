using System;
using CatalogueLibrary.Data.DataLoad;
using LoadModules.Generic.Mutilators.Dilution.Exceptions;
using LoadModules.Generic.Mutilators.Dilution.Operations;
using NUnit.Framework;
using ReusableLibraryCode.Checks;
using Rhino.Mocks;

namespace DataLoadEngineTests.Integration.DilutionTests
{
    public class DilutionCheckTests
    {
        [Test]
        public void TestChecking_RoundDateToMiddleOfQuarter_NoColumnSet()
        {
            var dil = new RoundDateToMiddleOfQuarter();
            Assert.Throws<DilutionColumnNotSetException>(() => dil.Check(new ThrowImmediatelyCheckNotifier()));
        }

        [TestCase("varchar(10)")]
        [TestCase("bit")]
        [TestCase("binary(50)")]
        public void TestChecking_RoundDateToMiddleOfQuarter_WrongDataType(string incompatibleType)
        {
            var col = MockRepository.GenerateMock<IPreLoadDiscardedColumn>();
            col.Expect(p => p.SqlDataType).Return(incompatibleType).Repeat.AtLeastOnce();

            var dil = new RoundDateToMiddleOfQuarter();
            dil.ColumnToDilute = col;

            Assert.Throws<Exception>(() => dil.Check(new ThrowImmediatelyCheckNotifier()));

            col.VerifyAllExpectations();
        }

        [TestCase("date")]
        [TestCase("datetime")]
        public void TestChecking_RoundDateToMiddleOfQuarter_CompatibleDataType(string incompatibleType)
        {
            var col = MockRepository.GenerateMock<IPreLoadDiscardedColumn>();
            col.Expect(p => p.SqlDataType).Return(incompatibleType).Repeat.AtLeastOnce();

            var dil = new RoundDateToMiddleOfQuarter();
            dil.ColumnToDilute = col;

            dil.Check(new ThrowImmediatelyCheckNotifier());

            col.VerifyAllExpectations();
        }
    }
}
