// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

/*
// Helpers/Settings.cs This file was automatically added when you installed the Settings Plugin. If you are not using a PCL then comment this file back in to use it.
using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace Rdmp.Core.ReusableLibraryCode.Helpers;

/// <summary>
/// This is the Settings static class that can be used in your Core solution or in any
/// of your client applications. All settings are laid out the same exact way with getters
/// and setters. 
/// </summary>
public static class Settings
{
	private static ISettings AppSettings
	{
		get
		{
			return CrossSettings.Current;
		}
	}

	#region Setting Constants

	private const string SettingsKey = "settings_key";
	private static readonly string SettingsDefault = string.Empty;

	#endregion


	public static string GeneralSettings
	{
		get
		{
			return AppSettings.GetValueOrDefault(SettingsKey, SettingsDefault);
		}
		set
		{
			AppSettings.AddOrUpdateValue(SettingsKey, value);
		}
	}
}*/
