// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Reflection;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Repositories;
using Rdmp.Core.Repositories.Construction;
using Rdmp.UI.LoadExecutionUIs;
using Rdmp.UI.ProjectUI;
using Rdmp.UI.Raceway;

namespace ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;

/// <summary>
/// Translates persistence strings into DeserializeInstructions for restoring the RDMP main application window layout after application close/restart.
/// </summary>
public class PersistenceDecisionFactory
{
    public PersistenceDecisionFactory()
    {
        //ensure dashboard UI assembly is loaded
        Assembly.Load(typeof(RacewayRenderAreaUI).Assembly.FullName);
        //ensure data export UI assembly is loaded
        Assembly.Load(typeof(ExtractionConfigurationUI).Assembly.FullName);
        //ensure DLE UI assembly is loaded
        Assembly.Load(typeof(ExecuteLoadMetadataUI).Assembly.FullName);
    }

    /// <summary>
    /// If <paramref name="persistString"/> describes the persisted control state
    /// as described by the basic <see cref="RDMPSingleControlTab"/> class (rather
    /// than a more specialised class like <see cref="RDMPSingleControlTab"/>) then
    /// we return a new instruction of what Type of control to create
    /// </summary>
    /// <param name="persistString"></param>
    /// <param name="repositoryLocator"></param>
    /// <returns></returns>
    public static DeserializeInstruction ShouldCreateBasicControl(string persistString,
        IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        if (!persistString.StartsWith(RDMPSingleControlTab.BasicPrefix))
            return null;

        //return BasicPrefix + s + Control.GetType().Name
        var tokens = persistString.Split(PersistStringHelper.Separator);

        if (tokens.Length != 2)
            throw new PersistenceException(
                $"Unexpected number of tokens ({tokens.Length}) for Persistence of Type {RDMPSingleControlTab.BasicPrefix}");

        var controlType = GetTypeByName(tokens[1], typeof(Control), repositoryLocator);

        return new DeserializeInstruction(controlType);
    }

    public static RDMPCollection? ShouldCreateCollection(string persistString)
    {
        if (!persistString.StartsWith(PersistableToolboxDockContent.Prefix))
            return null;

        return PersistableToolboxDockContent.GetToolboxFromPersistString(persistString);
    }

    public static DeserializeInstruction ShouldCreateSingleObjectControl(string persistString,
        IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        if (!persistString.StartsWith(PersistableSingleDatabaseObjectDockContent.Prefix))
            return null;

        //return Prefix + s + _control.GetType().Name + s + _databaseObject.Repository.GetType() +  s + _databaseObject.GetType().Name + s + _databaseObject.ID;
        var tokens = persistString.Split(PersistStringHelper.Separator);

        if (tokens.Length != 5)
            throw new PersistenceException(
                $"Unexpected number of tokens ({tokens.Length}) for Persistence of Type {PersistableSingleDatabaseObjectDockContent.Prefix}");

        var controlType = GetTypeByName(tokens[1], typeof(Control), repositoryLocator);
        var o = repositoryLocator.GetArbitraryDatabaseObject(tokens[2], tokens[3], int.Parse(tokens[4]));

        return new DeserializeInstruction(controlType, o);
    }

    public static DeserializeInstruction ShouldCreateObjectCollection(string persistString,
        IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        if (!persistString.StartsWith(PersistableObjectCollectionDockContent.Prefix))
            return null;

        if (!persistString.Contains(PersistStringHelper.ExtraText))
            throw new PersistenceException($"Persistence string did not contain '{PersistStringHelper.ExtraText}");

        //Looks something like this  RDMPObjectCollection:MyCoolControlUI:MyControlUIsBundleOfObjects:[CatalogueRepository:AggregateConfiguration:105,CatalogueRepository:AggregateConfiguration:102,CatalogueRepository:AggregateConfiguration:101]###EXTRA_TEXT###I've got a lovely bunch of coconuts
        var tokens = persistString.Split(PersistStringHelper.Separator);

        var uiType = GetTypeByName(tokens[1], typeof(Control), repositoryLocator);
        var collectionType = GetTypeByName(tokens[2], typeof(IPersistableObjectCollection), repositoryLocator);

        var collectionInstance = (IPersistableObjectCollection)ObjectConstructor.Construct(collectionType);
                
        if(collectionInstance.DatabaseObjects == null)
            throw new PersistenceException(
                $"Constructor of Type '{collectionType}' did not initialise property DatabaseObjects");
            
        var allObjectsString = PersistStringHelper.MatchCollectionInString(persistString);

        collectionInstance.DatabaseObjects.AddRange(PersistStringHelper.GetObjectCollectionFromPersistString(allObjectsString,repositoryLocator));

        var extraText = PersistStringHelper.GetExtraText(persistString);
        collectionInstance.LoadExtraText(extraText);

        return new DeserializeInstruction(uiType, collectionInstance);
    }

    private static Type GetTypeByName(string s, Type expectedBaseClassType,
        IRDMPPlatformRepositoryServiceLocator repositoryLocator)
    {
        var toReturn = repositoryLocator.CatalogueRepository.MEF.GetType(s) ??
                       throw new TypeLoadException($"Could not find Type called '{s}'");
        if (expectedBaseClassType != null)
            if (!expectedBaseClassType.IsAssignableFrom(toReturn))
                throw new TypeLoadException(
                    $"Persistence string included a reference to Type '{s}' which we managed to find but it did not match an expected base Type ({expectedBaseClassType})");

        return toReturn;
    }
}