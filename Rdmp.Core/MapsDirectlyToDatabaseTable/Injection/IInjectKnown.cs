// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

namespace Rdmp.Core.MapsDirectlyToDatabaseTable.Injection;

public interface IInjectKnown
{
    /// <summary>
    ///     Informs the implementing class that it should forget about all values provided by any InjectKnown calls
    /// </summary>
    void ClearAllInjections();
}

/// <summary>
///     Defines that the implementing class has an expensive operation for fetching a T but that a known instance might
///     already be
///     available (e.g. in a cache) which can be injected into it.
/// </summary>
/// <example>
///     <code>
/// public class Bob:IInjectKnown&lt;byte[]&gt;
/// {
///     private Lazy&lt;byte[]&gt; _knownBytes;
/// 
///     public Bob()
///     {
///         ClearAllInjections();
///     }
/// 
///     public void InjectKnown(byte[] instance)
///     {
///         _knownBytes = new Lazy&lt;byte[]&gt;(instance);
///     }
/// 
///     public void ClearAllInjections()
///     {
///         _knownBytes = new Lazy&lt;byte[]&gt;(FetchBytesExpensive);
///     }
/// 
///     private byte[] FetchBytesExpensive()
///     {
///         return new byte[10000];
///     }
/// }
/// 
/// </code>
/// </example>
/// <typeparam name="T"></typeparam>
public interface IInjectKnown<in T> : IInjectKnown
{
    /// <summary>
    ///     Records the known state of T so that it doesn't have to be fetched by an expensive operation e.g. going to the
    ///     database and fetching it.
    /// </summary>
    /// <param name="instance"></param>
    void InjectKnown(T instance);
}