using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Tests.Common;

namespace HIC.Common.Validation.Tests.ValidationPluginTests
{
    public class LegacySerializationTest:DatabaseTests
    {
        [Test]
        public void TestLegacyDeserialization()
        {
            string s = LegacyXML;
            Validator.LocatorForXMLDeserialization = RepositoryLocator;
            Validator v = Validator.LoadFromXml(s);
            Assert.IsNotNull(v);
        }

        private const string LegacyXML = @"<?xml version=""1.0"" encoding=""utf-16""?>
<Validator xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <ItemValidators>
    <ItemValidator>
      <TargetProperty>ADMISSION_DATE</TargetProperty>
      <SecondaryConstraints>
        <SecondaryConstraint xsi:type=""BoundDate"">
          <Name>bounddate</Name>
          <Consequence>Wrong</Consequence>
          <LowerFieldName />
          <UpperFieldName>DISCHARGE_DATE</UpperFieldName>
          <Inclusive>true</Inclusive>
          <Lower xsi:nil=""true"" />
          <Upper xsi:nil=""true"" />
        </SecondaryConstraint>
      </SecondaryConstraints>
    </ItemValidator>
    <ItemValidator>
      <PrimaryConstraint xsi:type=""Chi"">
        <Name>chi</Name>
        <Consequence>Wrong</Consequence>
      </PrimaryConstraint>
      <TargetProperty>CHI</TargetProperty>
      <SecondaryConstraints>
        <SecondaryConstraint xsi:type=""ReferentialIntegrityConstraint"">
          <Consequence>InvalidatesRow</Consequence>
          <Rationale>People should be in CHI_ULTRA</Rationale>
          <InvertLogic>false</InvertLogic>
        </SecondaryConstraint>
        <SecondaryConstraint xsi:type=""NotNull"">
          <Name>not null</Name>
          <Consequence>Missing</Consequence>
          <Rationale>All records must have a CHI</Rationale>
        </SecondaryConstraint>
      </SecondaryConstraints>
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>ADMISSION_REASON</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>ADMISSION_REASON_Desc</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>ADMISSION_TRANSFER_FROM</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>ADMISSION_TRANSFER_FROM_Desc</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>ADMISSION_TRANSFER_FROM_LOC</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>ADMISSION_TRANSFER_FROM_LOCATION_Desc</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>ADMISSION_TYPE</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>ADMISSION_TYPE_Desc</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>CIS_MARKER</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>DISCHARGE_DATE</TargetProperty>
      <SecondaryConstraints>
        <SecondaryConstraint xsi:type=""NotNull"">
          <Name>not null</Name>
          <Consequence>Missing</Consequence>
        </SecondaryConstraint>
      </SecondaryConstraints>
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>DISCHARGE_TRANSFER_TO</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>DISCHARGE_TYPE</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>EPISODE_RECORD_KEY</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>LENGTH_OF_STAY</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>SENDING_LOCATION</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>SIGNIFICANT_FACILITY</TargetProperty>
      <SecondaryConstraints />
    </ItemValidator>
    <ItemValidator>
      <TargetProperty>HB_OF_RESIDENCE_CYPHER</TargetProperty>
      <SecondaryConstraints>
        <SecondaryConstraint xsi:type=""NotNull"">
          <Name>not null</Name>
          <Consequence>InvalidatesRow</Consequence>
          <Rationale>Data must be attributed to a healthboard</Rationale>
        </SecondaryConstraint>
      </SecondaryConstraints>
    </ItemValidator>
  </ItemValidators>
</Validator>";
    }
}
