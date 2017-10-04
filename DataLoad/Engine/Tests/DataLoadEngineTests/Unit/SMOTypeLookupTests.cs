using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLoadEngine.DatabaseManagement.Operations;
using Microsoft.SqlServer.Management.Smo;
using NUnit.Framework;

namespace DataLoadEngineTests.Unit
{
    public class SMOTypeLookupTests
    {
        private SMOTypeLookup lookup = new SMOTypeLookup();

        [Test]
        public void Binary()
        {
            var dt = lookup.GetSMODataTypeForSqlStringDataType("varbinary(max)");
            Assert.AreEqual(DataType.VarBinaryMax,dt);

            dt = lookup.GetSMODataTypeForSqlStringDataType("binary(10)");
            Assert.AreEqual(DataType.Binary(10), dt);
        }


        [Test]
        public void Dates()
        {
            var dt = lookup.GetSMODataTypeForSqlStringDataType("datetime2");
            Assert.AreEqual(DataType.DateTime2(7), dt);

            dt = lookup.GetSMODataTypeForSqlStringDataType("datetime2(3)");
            Assert.AreEqual(DataType.DateTime2(3), dt);
        }

        [Test]
        public void Numbers()
        {
            var dt = lookup.GetSMODataTypeForSqlStringDataType("decimal(3,2)");
            Assert.AreEqual(DataType.Decimal(2,3), dt);

            dt = lookup.GetSMODataTypeForSqlStringDataType("numeric(10,5)");
            Assert.AreEqual(DataType.Numeric(5,10), dt);
        }

        [Test]
        public void Characters()
        {
            DataType dt;
            
            dt = lookup.GetSMODataTypeForSqlStringDataType("varchar(3)");
            Assert.AreEqual(DataType.VarChar(3), dt);

            dt = lookup.GetSMODataTypeForSqlStringDataType("varchar(max)");
            Assert.AreEqual(DataType.VarCharMax, dt);

            dt = lookup.GetSMODataTypeForSqlStringDataType("nvarchar(10)"); //this is actually an illegal type btw since there cant be more scale than precision but that doesn't stop C#!
            Assert.AreEqual(DataType.NVarChar(10), dt);
        }

        [Test]
        public void FreakyBits()
        {
            DataType dt;
            dt = lookup.GetSMODataTypeForSqlStringDataType("var char(3)");
            Assert.AreEqual(DataType.VarChar(3), dt);

            dt = lookup.GetSMODataTypeForSqlStringDataType("varchar(MAX)");
            Assert.AreEqual(DataType.VarCharMax, dt);

            dt = lookup.GetSMODataTypeForSqlStringDataType("nvarchar(!10!)"); //this is actually an illegal type btw since there cant be more scale than precision but that doesn't stop C#!
            Assert.AreEqual(DataType.NVarChar(10), dt);
        }
    }
}
