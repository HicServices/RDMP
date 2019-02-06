// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Plugin
{
    public class PluginNugetClass
    {
        // This project exists to build the HIC.RDMP.Plugin nuget package
        //
        // Whenever a CatalogueManager database version bump occurs, the appropriate part of the version number in this project's AssemblyInfo.cs should be updated and committed.
        // Jenkins will rebuild the plugin and push to the production NuGet server. When the database version is bumped, zero the third and fourth version parts.
        //
        // Use the third version number part for when a new package should be built and released when the database has *not* been changed. Simply bump the number and commit, Jenkins will do the rest.
        // Use the fourth version number for when you are building local packages for development, e.g. building a plugin that needs some changes in the core RDMP code for some reason, such as additional/changed interfaces. When
        // local plugin development is ready to be committed, zero the fourth version number and bump the third version number.
        //
        // The csproj.user class contains the settings for where a local dev build will push the package. There is a config block for both a basic
        // local filesystem repository and a local server. It is recommended that for development of plugins a symbol server is used, ProGet (http://inedo.com/proget) is free (as in beer, not open-source), easy-to-use and functions as both a package and symbol server.
        // Configure visual studio to look at your local nuget package repository, putting it before the production package repository. This means that
        // you can, during development, make changes to the core Plugin code and create new packages in your local repository without having to commit
        // work-in-progress code (which will end up in the production package!). When you update the package in your development environment, it will use the local version.
    }
}
