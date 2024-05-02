// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;

namespace Rdmp.Core.ReusableLibraryCode.Settings;

internal sealed class RDMPApplicationSettings
{
    private readonly IsolatedStorageFile store;
    private readonly object locker = new();

    public RDMPApplicationSettings()
    {
        try
        {
            store = IsolatedStorageFile.GetUserStoreForApplication();
        }
        catch (Exception)
        {
            store = IsolatedStorageFile.GetUserStoreForAssembly();
        }
    }

    /// <summary>
    ///     Add or Update
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    private bool AddOrUpdateValueInternal<T>(string key, T value)
    {
        if (value == null)
        {
            Remove(key);

            return true;
        }

        var type = value.GetType();

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            type = type.GenericTypeArguments.FirstOrDefault() ??
                   throw new ArgumentException($"Unable to retrieve type of Nullable {value}");


        if (type != typeof(string) &&
            type != typeof(decimal) &&
            type != typeof(double) &&
            type != typeof(float) &&
            type != typeof(DateTime) &&
            type != typeof(Guid) &&
            type != typeof(bool) &&
            type != typeof(int) &&
            type != typeof(long) &&
            type != typeof(byte)) throw new ArgumentException($"Value of type {type.Name} is not supported.");

        lock (locker)
        {
            string str;

            switch (value)
            {
                case decimal:
                    return AddOrUpdateValue(key,
                        Convert.ToString(Convert.ToDecimal(value), CultureInfo.InvariantCulture));
                case DateTime:
                    return AddOrUpdateValue(key,
                        Convert.ToString(-Convert.ToDateTime(value).ToUniversalTime().Ticks,
                            CultureInfo.InvariantCulture));
                default:
                    str = Convert.ToString(value, CultureInfo.InvariantCulture);
                    break;
            }

            string oldValue = null;

            if (store.FileExists(key))
            {
                using var stream = store.OpenFile(key, FileMode.Open);
                using var sr = new StreamReader(stream);
                oldValue = sr.ReadToEnd();
            }

            using (var stream = store.OpenFile(key, FileMode.Create, FileAccess.Write))
            {
                using var sw = new StreamWriter(stream);
                sw.Write(str);
            }

            return oldValue != str;
        }
    }

    /// <summary>
    ///     Get Value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    private T GetValueOrDefaultInternal<T>(string key, T defaultValue = default)
    {
        object value = null;
        lock (locker)
        {
            try
            {
                string str = null;

                // If the key exists, retrieve the value.
                if (store.FileExists(key))
                {
                    using var stream = store.OpenFile(key, FileMode.Open);
                    using var sr = new StreamReader(stream);
                    str = sr.ReadToEnd();
                }

                if (str == null)
                    return defaultValue;

                var type = typeof(T);

                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    type = type.GenericTypeArguments.FirstOrDefault();

                if (type == typeof(string))
                {
                    value = str;
                }

                else if (type == typeof(decimal))
                {
                    var savedDecimal = Convert.ToString(str);


                    value = Convert.ToDecimal(savedDecimal, CultureInfo.InvariantCulture);

                    return (T)value;
                }

                else if (type == typeof(double))
                {
                    value = Convert.ToDouble(str, CultureInfo.InvariantCulture);
                }

                else if (type == typeof(float))
                {
                    value = Convert.ToSingle(str, CultureInfo.InvariantCulture);
                }

                else if (type == typeof(DateTime))
                {
                    var ticks = Convert.ToInt64(str, CultureInfo.InvariantCulture);
                    //Old value, stored before update to UTC values
                    value = ticks >= 0
                        ? new DateTime(ticks)
                        :
                        //New value, UTC
                        new DateTime(-ticks, DateTimeKind.Utc);


                    return (T)value;
                }

                else if (type == typeof(Guid))
                {
                    if (Guid.TryParse(str, out var guid))
                        value = guid;
                }

                else if (type == typeof(bool))
                {
                    value = Convert.ToBoolean(str, CultureInfo.InvariantCulture);
                }

                else if (type == typeof(int))
                {
                    value = Convert.ToInt32(str, CultureInfo.InvariantCulture);
                }

                else if (type == typeof(long))
                {
                    value = Convert.ToInt64(str, CultureInfo.InvariantCulture);
                }

                else if (type == typeof(byte))
                {
                    value = Convert.ToByte(str, CultureInfo.InvariantCulture);
                }

                else
                {
                    throw new ArgumentException($"Value of type {type} is not supported.");
                }
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }

        return null != value ? (T)value : defaultValue;
    }

    /// <summary>
    ///     Remove key
    /// </summary>
    /// <param name="key">Key to remove</param>
    public void Remove(string key)
    {
        if (store.FileExists(key))
            store.DeleteFile(key);
    }

    /// <summary>
    ///     Clear all keys from settings
    /// </summary>
    public void Clear()
    {
        try
        {
            foreach (var file in store.GetFileNames()) store.DeleteFile(file);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to clear all defaults. Message: {ex.Message}");
        }
    }

    /// <summary>
    ///     Checks to see if the key has been added.
    /// </summary>
    /// <param name="key">Key to check</param>
    /// <returns>True if contains key, else false</returns>
    public bool Contains(string key)
    {
        return store.FileExists(key);
    }

    #region GetValueOrDefault

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public decimal GetValueOrDefault(string key, decimal defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public bool GetValueOrDefault(string key, bool defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public long GetValueOrDefault(string key, long defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public string GetValueOrDefault(string key, string defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public int GetValueOrDefault(string key, int defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public float GetValueOrDefault(string key, float defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public DateTime GetValueOrDefault(string key, DateTime defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public Guid GetValueOrDefault(string key, Guid defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    /// <summary>
    ///     Gets the current value or the default that you specify.
    /// </summary>
    /// <param name="key">Key for settings</param>
    /// <param name="defaultValue">default value if not set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>Value or default</returns>
    public double GetValueOrDefault(string key, double defaultValue, string fileName = null)
    {
        return GetValueOrDefaultInternal(key, defaultValue);
    }

    #endregion

    #region AddOrUpdateValue

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, decimal value)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, bool value)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, long value)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, string value)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, int value, string fileName = null)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, float value, string fileName = null)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, DateTime value, string fileName = null)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, Guid value, string fileName = null)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    /// <summary>
    ///     Adds or updates the value
    /// </summary>
    /// <param name="key">Key for setting</param>
    /// <param name="value">Value to set</param>
    /// <param name="fileName">Name of file for settings to be stored and retrieved </param>
    /// <returns>True of was added or updated and you need to save it.</returns>
    public bool AddOrUpdateValue(string key, double value, string fileName = null)
    {
        return AddOrUpdateValueInternal(key, value);
    }

    #endregion
}