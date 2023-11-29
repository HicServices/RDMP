// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.Validation;
using Tests.Common;

namespace Rdmp.Core.Tests.Validation.ValidationPluginTests;

public class LegacySerializationTest : DatabaseTests
{
    [Test]
    public void TestLegacyDeserialization()
    {
        Validator.LocatorForXMLDeserialization = RepositoryLocator;
        var v = Validator.LoadFromXml(LegacyXML);
        Assert.That(v, Is.Not.Null);
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