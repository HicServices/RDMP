// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Curation.Data;

/// <summary>
/// Type manager which supports loading assemblies from both the bin directory and plugin directories.  Types discovered are indexed
/// according to name so they can be built on demand later on.  
/// 
/// <para>Handles assembly resolution problems, binding redirection and partial assembly loading (e.g. if only some of the Types in the 
/// assembly could be resolved).</para>
/// </summary>
public class SafeDirectoryCatalog
{
    /// <summary>
    /// These assemblies do not load correctly and should be ignored (they produce warnings on Startup)
    /// </summary>
    public static readonly HashSet<string> Ignore = new() {

"0harmony.dll",
"accessibility.dll",
"api-ms-win-core-console-l1-1-0.dll",
"api-ms-win-core-console-l1-2-0.dll",
"api-ms-win-core-datetime-l1-1-0.dll",
"api-ms-win-core-debug-l1-1-0.dll",
"api-ms-win-core-errorhandling-l1-1-0.dll",
"api-ms-win-core-fibers-l1-1-0.dll",
"api-ms-win-core-file-l1-1-0.dll",
"api-ms-win-core-file-l1-2-0.dll",
"api-ms-win-core-file-l2-1-0.dll",
"api-ms-win-core-handle-l1-1-0.dll",
"api-ms-win-core-heap-l1-1-0.dll",
"api-ms-win-core-interlocked-l1-1-0.dll",
"api-ms-win-core-libraryloader-l1-1-0.dll",
"api-ms-win-core-localization-l1-2-0.dll",
"api-ms-win-core-memory-l1-1-0.dll",
"api-ms-win-core-namedpipe-l1-1-0.dll",
"api-ms-win-core-processenvironment-l1-1-0.dll",
"api-ms-win-core-processthreads-l1-1-0.dll",
"api-ms-win-core-processthreads-l1-1-1.dll",
"api-ms-win-core-profile-l1-1-0.dll",
"api-ms-win-core-rtlsupport-l1-1-0.dll",
"api-ms-win-core-string-l1-1-0.dll",
"api-ms-win-core-synch-l1-1-0.dll",
"api-ms-win-core-synch-l1-2-0.dll",
"api-ms-win-core-sysinfo-l1-1-0.dll",
"api-ms-win-core-timezone-l1-1-0.dll",
"api-ms-win-core-util-l1-1-0.dll",
"api-ms-win-crt-conio-l1-1-0.dll",
"api-ms-win-crt-convert-l1-1-0.dll",
"api-ms-win-crt-environment-l1-1-0.dll",
"api-ms-win-crt-filesystem-l1-1-0.dll",
"api-ms-win-crt-heap-l1-1-0.dll",
"api-ms-win-crt-locale-l1-1-0.dll",
"api-ms-win-crt-math-l1-1-0.dll",
"api-ms-win-crt-multibyte-l1-1-0.dll",
"api-ms-win-crt-private-l1-1-0.dll",
"api-ms-win-crt-process-l1-1-0.dll",
"api-ms-win-crt-runtime-l1-1-0.dll",
"api-ms-win-crt-stdio-l1-1-0.dll",
"api-ms-win-crt-string-l1-1-0.dll",
"api-ms-win-crt-time-l1-1-0.dll",
"api-ms-win-crt-utility-l1-1-0.dll",
"archive.dll",
"autoupdater.net.dll",
"autoupdater.net.resources.dll",
"azure.core.dll",
"azure.identity.dll",
"badmedicine.core.dll",
"bouncycastle.crypto.dll",
"clretwrc.dll",
"clrjit.dll",
"commandline.dll",
"consolecontrol.dll",
"consolecontrolapi.dll",
"coreclr.dll",
"csvhelper.dll",
"d3dcompiler_47_cor3.dll",
"dbgshim.dll",
"directwriteforwarder.dll",
"excelnumberformat.dll",
"fansi.dll",
"fansi.implementations.microsoftsql.dll",
"fansi.implementations.mysql.dll",
"fansi.implementations.oracle.dll",
"fansi.implementations.postgresql.dll",
"hostfxr.dll",
"hostpolicy.dll",
"hunspellx64.dll",
"hunspellx86.dll",
"icsharpcode.sharpziplib.dll",
"lexilla.dll",
"libarchive.net.dll",
"mapsdirectlytodatabasetable.dll",
"mathnet.numerics.dll",
"microsoft.bcl.asyncinterfaces.dll",
"microsoft.csharp.dll",
"microsoft.data.sqlclient.dll",
"microsoft.data.sqlclient.sni.dll",
"microsoft.diasymreader.native.amd64.dll",
"microsoft.identity.client.dll",
"microsoft.identity.client.extensions.msal.dll",
"microsoft.identitymodel.abstractions.dll",
"microsoft.identitymodel.jsonwebtokens.dll",
"microsoft.identitymodel.logging.dll",
"microsoft.identitymodel.protocols.dll",
"microsoft.identitymodel.protocols.openidconnect.dll",
"microsoft.identitymodel.tokens.dll",
"microsoft.sqlserver.server.dll",
"microsoft.visualbasic.core.dll",
"microsoft.visualbasic.dll",
"microsoft.visualbasic.forms.dll",
"microsoft.visualbasic.forms.resources.dll",
"microsoft.web.webview2.core.dll",
"microsoft.web.webview2.winforms.dll",
"microsoft.web.webview2.wpf.dll",
"microsoft.win32.primitives.dll",
"microsoft.win32.registry.accesscontrol.dll",
"microsoft.win32.registry.dll",
"microsoft.win32.systemevents.dll",
"mono.cecil.dll",
"mono.cecil.mdb.dll",
"mono.cecil.pdb.dll",
"mono.cecil.rocks.dll",
"monomod.common.dll",
"mscordaccore.dll",
"mscordaccore_amd64_amd64_6.0.1122.52304.dll",
"mscordbi.dll",
"mscorlib.dll",
"mscorrc.dll",
"msquic.dll",
"mysqlconnector.dll",
"netstandard.dll",
"newtonsoft.json.dll",
"nhunspell.dll",
"nlog.dll",
"npgsql.dll",
"npoi.dll",
"npoi.ooxml.dll",
"npoi.openxml4net.dll",
"npoi.openxmlformats.dll",
"objectlistview.dll",
"objectlistview2022net.dll",
"oracle.manageddataaccess.dll",
"penimc_cor3.dll",
"plugin.settings.abstractions.dll",
"plugin.settings.dll",
"presentationcore.dll",
"presentationcore.resources.dll",
"presentationframework-systemcore.dll",
"presentationframework-systemdata.dll",
"presentationframework-systemdrawing.dll",
"presentationframework-systemxml.dll",
"presentationframework-systemxmllinq.dll",
"presentationframework.aero.dll",
"presentationframework.aero2.dll",
"presentationframework.aerolite.dll",
"presentationframework.classic.dll",
"presentationframework.dll",
"presentationframework.luna.dll",
"presentationframework.resources.dll",
"presentationframework.royale.dll",
"presentationnative_cor3.dll",
"presentationui.dll",
"presentationui.resources.dll",
"rdmp.core.dll",
"rdmp.core.resources.dll",
"rdmp.ui.dll",
"reachframework.dll",
"reachframework.resources.dll",
"renci.sshnet.dll",
"researchdatamanagementplatform.dll",
"reusablelibrarycode.dll",
"scintilla.dll",
"scintilla.net.dll",
"scintillanet.dll",
"sixlabors.fonts.dll",
"sixlabors.imagesharp.dll",
"sixlabors.imagesharp.drawing.dll",
"spectre.console.dll",
"sshnet.security.cryptography.dll",
"system.appcontext.dll",
"system.buffers.dll",
"system.codedom.dll",
"system.collections.concurrent.dll",
"system.collections.dll",
"system.collections.immutable.dll",
"system.collections.nongeneric.dll",
"system.collections.specialized.dll",
"system.componentmodel.annotations.dll",
"system.componentmodel.composition.dll",
"system.componentmodel.dataannotations.dll",
"system.componentmodel.dll",
"system.componentmodel.eventbasedasync.dll",
"system.componentmodel.primitives.dll",
"system.componentmodel.typeconverter.dll",
"system.configuration.configurationmanager.dll",
"system.configuration.dll",
"system.console.dll",
"system.core.dll",
"system.data.common.dll",
"system.data.datasetextensions.dll",
"system.data.dll",
"system.design.dll",
"system.diagnostics.contracts.dll",
"system.diagnostics.debug.dll",
"system.diagnostics.diagnosticsource.dll",
"system.diagnostics.eventlog.dll",
"system.diagnostics.eventlog.messages.dll",
"system.diagnostics.fileversioninfo.dll",
"system.diagnostics.performancecounter.dll",
"system.diagnostics.process.dll",
"system.diagnostics.stacktrace.dll",
"system.diagnostics.textwritertracelistener.dll",
"system.diagnostics.tools.dll",
"system.diagnostics.tracesource.dll",
"system.diagnostics.tracing.dll",
"system.directoryservices.dll",
"system.directoryservices.protocols.dll",
"system.dll",
"system.drawing.common.dll",
"system.drawing.design.dll",
"system.drawing.dll",
"system.drawing.primitives.dll",
"system.dynamic.runtime.dll",
"system.formats.asn1.dll",
"system.globalization.calendars.dll",
"system.globalization.dll",
"system.globalization.extensions.dll",
"system.identitymodel.tokens.jwt.dll",
"system.io.compression.brotli.dll",
"system.io.compression.dll",
"system.io.compression.filesystem.dll",
"system.io.compression.native.dll",
"system.io.compression.zipfile.dll",
"system.io.dll",
"system.io.filesystem.accesscontrol.dll",
"system.io.filesystem.dll",
"system.io.filesystem.driveinfo.dll",
"system.io.filesystem.primitives.dll",
"system.io.filesystem.watcher.dll",
"system.io.isolatedstorage.dll",
"system.io.memorymappedfiles.dll",
"system.io.packaging.dll",
"system.io.pipes.accesscontrol.dll",
"system.io.pipes.dll",
"system.io.unmanagedmemorystream.dll",
"system.linq.dll",
"system.linq.expressions.dll",
"system.linq.parallel.dll",
"system.linq.queryable.dll",
"system.memory.data.dll",
"system.memory.dll",
"system.net.dll",
"system.net.http.dll",
"system.net.http.json.dll",
"system.net.httplistener.dll",
"system.net.mail.dll",
"system.net.nameresolution.dll",
"system.net.networkinformation.dll",
"system.net.ping.dll",
"system.net.primitives.dll",
"system.net.quic.dll",
"system.net.requests.dll",
"system.net.security.dll",
"system.net.servicepoint.dll",
"system.net.sockets.dll",
"system.net.webclient.dll",
"system.net.webheadercollection.dll",
"system.net.webproxy.dll",
"system.net.websockets.client.dll",
"system.net.websockets.dll",
"system.numerics.dll",
"system.numerics.vectors.dll",
"system.objectmodel.dll",
"system.printing.dll",
"system.private.corelib.dll",
"system.private.datacontractserialization.dll",
"system.private.uri.dll",
"system.private.xml.dll",
"system.private.xml.linq.dll",
"system.reflection.dispatchproxy.dll",
"system.reflection.dll",
"system.reflection.emit.dll",
"system.reflection.emit.ilgeneration.dll",
"system.reflection.emit.lightweight.dll",
"system.reflection.extensions.dll",
"system.reflection.metadata.dll",
"system.reflection.primitives.dll",
"system.reflection.typeextensions.dll",
"system.resources.extensions.dll",
"system.resources.reader.dll",
"system.resources.resourcemanager.dll",
"system.resources.writer.dll",
"system.runtime.caching.dll",
"system.runtime.compilerservices.unsafe.dll",
"system.runtime.compilerservices.visualc.dll",
"system.runtime.dll",
"system.runtime.extensions.dll",
"system.runtime.handles.dll",
"system.runtime.interopservices.dll",
"system.runtime.interopservices.runtimeinformation.dll",
"system.runtime.intrinsics.dll",
"system.runtime.loader.dll",
"system.runtime.numerics.dll",
"system.runtime.serialization.dll",
"system.runtime.serialization.formatters.dll",
"system.runtime.serialization.json.dll",
"system.runtime.serialization.primitives.dll",
"system.runtime.serialization.xml.dll",
"system.security.accesscontrol.dll",
"system.security.claims.dll",
"system.security.cryptography.algorithms.dll",
"system.security.cryptography.cng.dll",
"system.security.cryptography.csp.dll",
"system.security.cryptography.encoding.dll",
"system.security.cryptography.openssl.dll",
"system.security.cryptography.pkcs.dll",
"system.security.cryptography.primitives.dll",
"system.security.cryptography.protecteddata.dll",
"system.security.cryptography.x509certificates.dll",
"system.security.cryptography.xml.dll",
"system.security.dll",
"system.security.permissions.dll",
"system.security.principal.dll",
"system.security.principal.windows.dll",
"system.security.securestring.dll",
"system.servicemodel.web.dll",
"system.serviceprocess.dll",
"system.text.encoding.codepages.dll",
"system.text.encoding.dll",
"system.text.encoding.extensions.dll",
"system.text.encodings.web.dll",
"system.text.json.dll",
"system.text.regularexpressions.dll",
"system.threading.accesscontrol.dll",
"system.threading.channels.dll",
"system.threading.dll",
"system.threading.overlapped.dll",
"system.threading.tasks.dataflow.dll",
"system.threading.tasks.dll",
"system.threading.tasks.extensions.dll",
"system.threading.tasks.parallel.dll",
"system.threading.thread.dll",
"system.threading.threadpool.dll",
"system.threading.timer.dll",
"system.transactions.dll",
"system.transactions.local.dll",
"system.valuetuple.dll",
"system.web.dll",
"system.web.httputility.dll",
"system.windows.controls.ribbon.dll",
"system.windows.controls.ribbon.resources.dll",
"system.windows.dll",
"system.windows.extensions.dll",
"system.windows.forms.datavisualization.dll",
"system.windows.forms.design.dll",
"system.windows.forms.design.editors.dll",
"system.windows.forms.design.resources.dll",
"system.windows.forms.dll",
"system.windows.forms.primitives.dll",
"system.windows.forms.primitives.resources.dll",
"system.windows.forms.resources.dll",
"system.windows.input.manipulations.dll",
"system.windows.input.manipulations.resources.dll",
"system.windows.presentation.dll",
"system.xaml.dll",
"system.xaml.resources.dll",
"system.xml.dll",
"system.xml.linq.dll",
"system.xml.readerwriter.dll",
"system.xml.serialization.dll",
"system.xml.xdocument.dll",
"system.xml.xmldocument.dll",
"system.xml.xmlserializer.dll",
"system.xml.xpath.dll",
"system.xml.xpath.xdocument.dll",
"typeguesser.dll",
"ucrtbase.dll",
"uiautomationclient.dll",
"uiautomationclient.resources.dll",
"uiautomationclientsideproviders.dll",
"uiautomationclientsideproviders.resources.dll",
"uiautomationprovider.dll",
"uiautomationprovider.resources.dll",
"uiautomationtypes.dll",
"uiautomationtypes.resources.dll",
"universaltypeconverter.dll",
"vcruntime140_cor3.dll",
"vpksoft.scintillalexers.net.dll",
"webview2loader.dll",
"wecantspell.hunspell.dll",
"weifenluo.winformsui.docking.dll",
"weifenluo.winformsui.docking.themevs2015.dll",
"windowsbase.dll",
"windowsbase.resources.dll",
"windowsformsintegration.dll",
"windowsformsintegration.resources.dll",
"wpfgfx_cor3.dll",
"yamldotnet.dll"
    };

    /// <summary>
    /// Assemblies successfully loaded
    /// </summary>
    public readonly ConcurrentDictionary<string, Assembly> GoodAssemblies = new ();
    public readonly ConcurrentDictionary<Assembly,Type[]> TypesByAssembly = new ();

    private object oTypesLock = new object();
    public HashSet<Type> Types = new HashSet<Type>();
    public ConcurrentDictionary<string,Type> TypesByName = new ();

    /// <summary>
    /// The number of ignored dlls that were skipped because another copy was already seen
    /// with the same major/minor/build version
    /// </summary>
    public int DuplicateDllsIgnored { get; set; } = 0;

    /// <summary>
    /// Assemblies which could not be loaded
    /// </summary>
    public Dictionary<string,Exception> BadAssembliesDictionary { get; set; }
        
    /// <summary>
    /// Delegate for skipping certain dlls
    /// </summary>
    public static Func<FileInfo,bool> IgnoreDll { get; set; }

    /// <summary>
    /// Creates a new list of MEF plugin classes from the dlls/files in the directory list provided
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="directories"></param>
    public SafeDirectoryCatalog(ICheckNotifier listener, params string[] directories)
    {
        BadAssembliesDictionary = new Dictionary<string, Exception>();

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            TypesByAssembly.TryAdd(assembly, assembly.GetTypes());
            foreach (var type in assembly.GetTypes())
                AddType(type);
        }

        var files = new HashSet<FileInfo>();
                       
        foreach (var directory in directories)
        {
            if (directory is null)
                continue;
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory); //empty directory 

            foreach(var f in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
            {
                var newOne = new FileInfo(f);
                var existing = files.SingleOrDefault(d => d.Name.Equals(newOne.Name));

                // don't load the cli dir
                if (IgnoreDll?.Invoke(newOne) == true)
                    continue;

                if (existing == null)
                {
                    files.Add(newOne);
                    continue;
                }


                // Need to resolve duplicate/conflict:
                var existingOneVersion = FileVersionInfo.GetVersionInfo(existing.FullName);
                var newOneVersion = FileVersionInfo.GetVersionInfo(newOne.FullName);

                FileInfo winner;

                // if we already have a copy of this exact dll we don't care about loading it
                if (FileVersionsAreEqual(newOneVersion, existingOneVersion))
                {
                    // no need to spam user with warnings about duplicated dlls
                    DuplicateDllsIgnored++;
                    continue;
                }

                if (FileVersionGreaterThan(newOneVersion, existingOneVersion))
                {
                    files.Remove(existing);
                    files.Add(newOne);
                    winner = newOne;
                }
                else
                {
                    winner = existing;
                }

                listener?.OnCheckPerformed(new CheckEventArgs(
                    $"Found 2 copies of {newOne.Name}.  They were {existing.FullName} ({existingOneVersion.FileVersion}) and {newOne.FullName} ({newOneVersion.FileVersion}).  Only {winner.FullName} will be loaded",
                    CheckResult.Success));
            }
        }

        // Find and load all the DLLs which are not ignored
        foreach(var file in files)
            LoadDll(file,listener);
    }

    private void LoadDll(FileInfo f, ICheckNotifier listener)
    {
        Assembly ass = null;
        if (Ignore.Contains(f.Name.ToLowerInvariant()))
          return;
            
        try
        {
            ass = AssemblyResolver.LoadFile(f);
            AddTypes(f, ass, ass.GetTypes(), listener);
        }
        catch (ReflectionTypeLoadException ex)
        {
            //if we loaded the assembly and some types
            if (ex.Types.Any() && ass != null)
            {
                listener?.OnCheckPerformed(new CheckEventArgs(
                    ErrorCodes.CouldOnlyHalfLoadDll,
                    ex,null,
                    ex.Types.Count(t => t != null),
                    ex.Types.Length,
                    f.Name));

                AddTypes(f, ass, ex.Types, listener); //the assembly is bad but at least some of the Types were legit
            }
            else
                AddBadAssembly(f, ex, listener); //the assembly could not be loaded properly
        }
        catch (BadImageFormatException)
        {
            listener?.OnCheckPerformed(new CheckEventArgs($"Did not load '{f}' because it is not a dotnet assembly", CheckResult.Success));
        }
        catch (Exception ex)
        {
            AddBadAssembly(f, ex, listener);
        }
            
    }

    /// <summary>
    /// Returns true if the two versions have the same FileMajorPart, FileMinorPart and FileBuildPart version numbers
    /// </summary>
    /// <param name="newOneVersion"></param>
    /// <param name="existingOneVersion"></param>
    /// <returns></returns>
    private static bool FileVersionsAreEqual(FileVersionInfo newOneVersion, FileVersionInfo existingOneVersion)
    {
        return newOneVersion.FileMajorPart == existingOneVersion.FileMajorPart &&
               newOneVersion.FileMinorPart == existingOneVersion.FileMinorPart &&
               newOneVersion.FileBuildPart == existingOneVersion.FileBuildPart;
    }

    /// <summary>
    /// Returns true if the <paramref name="newOneVersion"/> is a later version than <paramref name="existingOneVersion"/>.
    /// Does not consider private build part e.g. 1.0.0-alpha1 and 1.0.0-alpha2 are not considered different
    /// </summary>
    /// <param name="newOneVersion"></param>
    /// <param name="existingOneVersion"></param>
    /// <returns></returns>
    private static bool FileVersionGreaterThan(FileVersionInfo newOneVersion, FileVersionInfo existingOneVersion)
    {
        if (newOneVersion.FileMajorPart > existingOneVersion.FileMajorPart)
            return true;
        // This is needed to ensure that 1.2.0 is seen as older than 2.0.0
        if (newOneVersion.FileMajorPart < existingOneVersion.FileMajorPart)
            return false;

        // First part equal, so use second as tie-breaker:
        if (newOneVersion.FileMinorPart > existingOneVersion.FileMinorPart)
            return true;
        if (newOneVersion.FileMinorPart < existingOneVersion.FileMinorPart)
            return false;

        if (newOneVersion.FileBuildPart > existingOneVersion.FileBuildPart)
            return true;

        return false;
    }

    private void AddBadAssembly(FileInfo f, Exception ex,ICheckNotifier listener)
    {
        if (BadAssembliesDictionary.ContainsKey(f.FullName)) return;    // Only report each failure once
        BadAssembliesDictionary.Add(f.FullName, ex);
        listener?.OnCheckPerformed(new CheckEventArgs(ErrorCodes.CouldNotLoadDll, null,ex,f.FullName));
    }

    private void AddTypes(FileInfo f, Assembly ass, Type[] types, ICheckNotifier listener)
    {
        types = types.Where(t => t != null).ToArray();
        TypesByAssembly.TryAdd(ass,types);
            
        foreach(var t in types)
            if(t.FullName != null && !TypesByName.ContainsKey(t.FullName))
                AddType(t.FullName,t);

        GoodAssemblies.TryAdd(f.FullName, ass);

        //tell them as we go how far we are through
        listener?.OnCheckPerformed(new CheckEventArgs($"Successfully loaded Assembly {f.FullName} into memory", CheckResult.Success));
    }

    internal void AddType(Type type)
    {
        AddType(type.FullName,type);
    }

    internal void AddType(string typeNameOrAlias, Type type)
    {
        //only add it if it is novel
        if (!TypesByName.ContainsKey(typeNameOrAlias))
            TypesByName.TryAdd(typeNameOrAlias, type);

        lock (oTypesLock)
        {
            Types.Add(type);
        }
    }

    public IEnumerable<Type> GetAllTypes()
    {
        lock(oTypesLock)
            return Types;
    }
}