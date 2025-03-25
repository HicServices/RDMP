// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories.Construction;

namespace Rdmp.Core.Repositories;

/// <summary>
///     Provides support for downloading Plugins out of the Catalogue Database, identifying Exports and building the
///     <see cref="SafeDirectoryCatalog" />.  It also includes methods for creating instances of the exported Types.
///     <para>
///         The class name MEF is a misnomer because historically we used the Managed Extensibility Framework (but now we
///         just grab everything with reflection)
///     </para>
/// </summary>
public static class MEF
{
    // TODO: Cache/preload this for AOT later; figure out generic support
    private static Lazy<ReadOnlyDictionary<string, Type>> _types;
    private static readonly ConcurrentDictionary<Type, Type[]> TypeCache = new();
    private static readonly Dictionary<string, Exception> badAssemblies = new();

    static MEF()
    {
        AppDomain.CurrentDomain.AssemblyLoad += Flush;
        Flush(null, null);
    }

    private static void Flush(object _1, AssemblyLoadEventArgs ale)
    {
        if (_types?.IsValueCreated != false)
            _types = new Lazy<ReadOnlyDictionary<string, Type>>(PopulateUnique,
                LazyThreadSafetyMode.ExecutionAndPublication);
        TypeCache.Clear();
    }

    //private static readonly Regex ExcludeAssembly = new(@"^(<|Interop\+|Microsoft|System|MongoDB|NPOI|SixLabors|NUnit|OracleInternal|Npgsql|Amazon|Castle|Newtonsoft|SharpCompress|Terminal|YamlDotNet|Moq|BrightIdeasSoftware|MySqlConnector|Azure|ZstdSharp|CommandLine|FAnsi|Internal|Mono|DnsClient|Oracle|MS|NuGet|Unix)", RegexOptions.Compiled|RegexOptions.CultureInvariant);
    private static ReadOnlyDictionary<string, Type> PopulateUnique()
    {
        Stopwatch.StartNew();
        var typeByName = new Dictionary<string, Type>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.FullName?.StartsWith("CommandLine", StringComparison.Ordinal) != false) continue;
            try
            {
                foreach (var type in assembly.GetTypes())
                foreach (var alias in new[]
                         {
                             Tail(type.FullName), type.FullName, Tail(type.FullName).ToUpperInvariant(),
                             type.FullName?.ToUpperInvariant()
                         }.Where(static x => x is not null).Distinct())
                    if (!typeByName.TryAdd(alias, type) &&
                        type.FullName?.StartsWith("Rdmp.Core", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Simple hack so Rdmp.Core types like ColumnInfo take precedence over others like System.Data.Select+ColumnInfo
                        typeByName.Remove(alias);
                        typeByName.Add(alias, type);
                    }
            }
            catch (Exception e)
            {
                lock (badAssemblies)
                {
                    badAssemblies.TryAdd(assembly.FullName, e);
                }

                Console.WriteLine(e);
            }
        }

        return new ReadOnlyDictionary<string, Type>(typeByName);
    }

    private static string Tail(string full)
    {
        var off = full.LastIndexOf(".", StringComparison.Ordinal) + 1;
        return off == 0 ? full : full[off..];
    }


    /// <summary>
    ///     Looks up the given Type in all loaded assemblies (during <see cref="Startup.Startup" />).  Returns null
    ///     if the Type is not found.
    ///     <para>
    ///         This method supports both fully qualified Type names and Name only (although this is slower).  Answers
    ///         are cached.
    ///     </para>
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static Type GetType(string typeName)
    {
        ArgumentException.ThrowIfNullOrEmpty(typeName);

        // Try for exact match, then caseless match, then tail match, then tail caseless match
        if (_types.Value.TryGetValue(typeName, out var type)) return type;
        if (_types.Value.TryGetValue(typeName.ToUpperInvariant(), out type)) return type;
        if (_types.Value.TryGetValue(Tail(typeName), out type)) return type;

        return _types.Value.TryGetValue(Tail(typeName).ToUpperInvariant(), out type) ? type : null;
    }

    public static Type GetType(string type, Type expectedBaseClass)
    {
        var t = GetType(type);

        return !expectedBaseClass.IsAssignableFrom(t)
            ? throw new Exception(
                $"Found Type {t?.FullName} for '{type}' did not implement expected base class/interface '{expectedBaseClass}'")
            : t;
    }

    public static IReadOnlyDictionary<string, Exception> ListBadAssemblies()
    {
        lock (badAssemblies)
        {
            return new ReadOnlyDictionary<string, Exception>(badAssemblies);
        }
    }

    /// <summary>
    ///     <para>
    ///         Turns the legit C# name:
    ///         DataLoadEngine.DataFlowPipeline.IDataFlowSource`1[[System.Data.DataTable, System.Data, Version=4.0.0.0,
    ///         Culture=neutral, PublicKeyToken=b77a5c561934e089]]
    ///     </para>
    ///     <para>
    ///         Into a proper C# code:
    ///         IDataFlowSource&lt;DataTable&gt;
    ///     </para>
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public static string GetCSharpNameForType(Type t)
    {
        if (!t.IsGenericType) return t.Name;

        if (t.GenericTypeArguments.Length != 1)
            throw new NotSupportedException(
                "Generic type has more than 1 token (e.g. T1,T2) so no idea what MEF would call it");

        var genericTypeName = t.GetGenericTypeDefinition().Name;

        Debug.Assert(genericTypeName.EndsWith("`1", StringComparison.Ordinal));
        genericTypeName = genericTypeName[..^"`1".Length];

        var underlyingType = t.GenericTypeArguments.Single().Name;
        return $"{genericTypeName}<{underlyingType}>";
    }

    public static IEnumerable<Type> GetTypes<T>()
    {
        return GetTypes(typeof(T));
    }

    /// <summary>
    ///     Returns MEF exported Types which inherit or implement <paramref name="type" />.  E.g. pass IAttacher to see
    ///     all exported implementers
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private static IEnumerable<Type> GetTypes(Type type)
    {
        return TypeCache.GetOrAdd(type,
            static target => _types.Value.Values.Where(t => !t.IsInterface && !t.IsAbstract)
                .Where(target.IsAssignableFrom).Distinct().ToArray());
    }

    /// <summary>
    ///     Returns all MEF exported classes decorated with the specified generic export e.g.
    /// </summary>
    /// <param name="genericType"></param>
    /// <param name="typeOfT"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetGenericTypes(Type genericType, Type typeOfT)
    {
        var target = genericType.MakeGenericType(typeOfT);
        return _types.Value.Values.Where(t => !t.IsAbstract && !t.IsGenericType && target.IsAssignableFrom(t))
            .Distinct();
    }

    public static IEnumerable<Type> GetAllTypes()
    {
        return _types.Value.Values.Distinct().AsEnumerable();
    }

    /// <summary>
    ///     Creates an instance of the named class with the provided constructor arguments
    /// </summary>
    /// <typeparam name="T">The base/interface of the Type you want to create e.g. IAttacher</typeparam>
    /// <returns></returns>
    public static T CreateA<T>(string typeToCreate, params object[] args)
    {
        var typeToCreateAsType = GetType(typeToCreate) ?? throw new Exception($"Could not find Type '{typeToCreate}'");

        //can we cast to T?
        if (typeToCreateAsType.IsAssignableFrom(typeof(T)))
            throw new Exception(
                $"Requested typeToCreate '{typeToCreate}' was not assignable to the required Type '{typeof(T).Name}'");

        var instance = (T)ObjectConstructor.ConstructIfPossible(typeToCreateAsType, args) ??
                       throw new ObjectLacksCompatibleConstructorException(
                           $"Could not construct a {typeof(T)} using the {args.Length} constructor arguments");
        return instance;
    }

    public static void AddTypeToCatalogForTesting(Type p0)
    {
        if (!_types.Value.ContainsKey(p0.FullName ?? throw new ArgumentNullException(nameof(p0))))
            throw new Exception($"Type {p0.FullName} was not preloaded");
    }
}