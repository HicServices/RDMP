using System;
using System.Data;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using NUnit.Framework;
using ReusableLibraryCode.Progress;

namespace CatalogueLibraryTests.Unit
{
    public class PreInitializeTests
    {

        DataFlowPipelineContext<DataTable> context = new DataFlowPipelineContext<DataTable>();
        Fish fish = new Fish();

        [Test]
        public void TestNormal()
        {

            FishUser fishUser = new FishUser();

            Assert.AreNotEqual(fishUser.IFish, fish);
            context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(),fishUser, fish);
            Assert.AreEqual(fishUser.IFish, fish);
        }

        [Test]
        public void TestOneOFMany()
        {

            FishUser fishUser = new FishUser();
            
            Assert.AreNotEqual(fishUser.IFish, fish);
            context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), fishUser,new object(), fish);
            Assert.AreEqual(fishUser.IFish, fish);
        }
        [Test]
        public void TestCasting()
        {

            FishUser fishUser = new FishUser();

            Assert.AreNotEqual(fishUser.IFish, fish);
            context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), fishUser, (IFish)fish);
            Assert.AreEqual(fishUser.IFish, fish);
        }
           
        [Test]
        public void TestDownCasting()
        {
            SpecificFishUser fishUser = new SpecificFishUser();

            IFish f = fish;
            Assert.AreNotEqual(fishUser.IFish, fish);
            context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), fishUser, f);
            Assert.AreEqual(fishUser.IFish, fish);
        }
        [Test]
        public void TestNoObjects()
        {
            SpecificFishUser fishUser = new SpecificFishUser();
            var ex = Assert.Throws<Exception>(()=>context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), fishUser, new object[0]));
            Assert.IsTrue(ex.Message.StartsWith("The following expected types were not passed to PreInitialize:Fish"));
        }

        [Test]
        public void TestWrongObjects()
        {
            SpecificFishUser fishUser = new SpecificFishUser();
            var ex = Assert.Throws<Exception>(() => context.PreInitialize(new ThrowImmediatelyDataLoadEventListener(), fishUser, new Penguin()));
            Assert.IsTrue(ex.Message.StartsWith("The following expected types were not passed to PreInitialize:Fish"));
            Assert.IsTrue(ex.Message.Contains("The object types passed were:"));
            Assert.IsTrue(ex.Message.Contains("Penguin"));
        }




        private class FishUser:IPipelineRequirement<IFish>, IDataFlowComponent<DataTable>
        {
            public IFish IFish;

            public void PreInitialize(IFish value, IDataLoadEventListener listener)
            {
                IFish = value;

            }
            #region boiler plate
            public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
                GracefulCancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
            {
                throw new NotImplementedException();
            }

            public void Abort(IDataLoadEventListener listener)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
        private class SpecificFishUser : IPipelineRequirement<Fish>, IDataFlowComponent<DataTable>
        {
            public IFish IFish;

            public void PreInitialize(Fish value, IDataLoadEventListener listener)
            {
                IFish = value;

            }
            #region boiler plate
            public DataTable ProcessPipelineData(DataTable toProcess, IDataLoadEventListener listener,
                GracefulCancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }

            public void Dispose(IDataLoadEventListener listener, Exception pipelineFailureExceptionIfAny)
            {
                throw new NotImplementedException();
            }

            public void Abort(IDataLoadEventListener listener)
            {
                throw new NotImplementedException();
            }
            #endregion
        }
        private interface IFish
        {
            string GetFish();
        }

        private class Fish:IFish
        {
            public string GetFish()
            {
                return "fish";
            }
        }
        private class Penguin
        {
            public string GetPenguin()
            {
                return "Penguin";
            }
        }
    }
}