// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Repositories;
using ReusableLibraryCode;

namespace Rdmp.UI.Tests.DesignPatternTests.ClassFileEvaluation
{
    public class SuspiciousRelationshipPropertyUse
    {
        private readonly MEF mef;
        List<string>  _fails = new List<string>();

        public SuspiciousRelationshipPropertyUse(MEF mef)
        {
            this.mef = mef;
        }

        public void FindPropertyMisuse(List<string> csFilesFound)
        {
            //Find all the types that come from the database
            var types = typeof(Catalogue).Assembly.GetTypes().Where(t => typeof(DatabaseEntity).IsAssignableFrom(t));
            types = types.Union(typeof(Project).Assembly.GetTypes().Where(t => typeof(DatabaseEntity).IsAssignableFrom(t)))
                .ToArray();

            foreach (Type type in types)
            {
                //if it's a spont object ignore it
                if(typeof(SpontaneousObject).IsAssignableFrom(type) || type == typeof(SpontaneouslyInventedColumn) || type == typeof(SpontaneouslyInventedFilter))
                    continue;

                //Find the C sharp code for the class
                var relationshipProperties = type.GetProperties().Where(p => p.CanRead && !p.CanWrite);
                
                string expectedFileName = type.Name + ".cs";
                var files = csFilesFound.Where(f => f.EndsWith("\\" + expectedFileName,StringComparison.CurrentCultureIgnoreCase)).ToArray();

                if (files.Length == 0)
                {
                    _fails.Add("FAIL: Could not find a csFile called '" + expectedFileName + "'");
                    continue;
                }
                if(files.Length > 1)
                {
                    _fails.Add("FAIL: found multiple csFiles called '" + expectedFileName + "'");
                    continue;
                }

                //Find the #region Relationships bit should contain all the properties which get; database objects or enumerates database objects (These shouldn't have a set;)
                var classSourceCode = File.ReadAllText(files[0]);

                Regex r = new Regex("#region Relationships(.*)#endregion",RegexOptions.Singleline);

                string relationshipsRegion = null;

                var matches = r.Matches(classSourceCode);

                if (matches.Count == 1)
                    relationshipsRegion = matches[0].Groups[1].Value;
                
                if (matches.Count > 1)
                    _fails.Add("FAIL: Class " + type.FullName + " has multiple '#region Relationships' blocks" );
                
                foreach (PropertyInfo relationshipProperty in relationshipProperties)
                {
                    if (relationshipProperty.Name.Equals("ID"))
                        continue;

                    if (relationshipProperty.CustomAttributes.All(c => c.AttributeType != typeof (NoMappingToDatabase)))
                    {
                        _fails.Add("FAIL: Class " + type.FullName + " has readonly property " + relationshipProperty + " which is not decorated with NoMapping");
                        continue;
                    }

                    if(!IsRelationshipProperty(relationshipProperty.PropertyType))
                    {
                        Console.WriteLine("SKIPPED: Property " + relationshipProperty + " is ReadOnly and [NoMapping] but doesn't look like it serves up related objects");
                        continue;
                    }
                                        
                    if (relationshipsRegion == null)
                        _fails.Add("FAIL: Class "+ type.FullName+ " has no '#region Relationships' blocks but has a relationship style Property called " + relationshipProperty.Name );
                    else
                    {
                        string expectedString = MEF.GetCSharpNameForType(relationshipProperty.PropertyType) + " " + relationshipProperty.Name;

                        if (!relationshipsRegion.Contains(expectedString))
                            _fails.Add("FAIL: Class " + type.FullName + " has a property we expected to be '" + expectedString + "' but it did not appear in the '#region Relationships'");
                    }
                }


                var databaseProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanRead && p.CanWrite);

                string suggestedFieldDeclarations = "";
                string suggestedMethodWrappers = "";
                
                foreach (PropertyInfo p in databaseProperties)
                {
                    //its a NoMapping 
                    if (p.CustomAttributes.Any(c => c.AttributeType == typeof (NoMappingToDatabase))) 
                        continue;
                    
                    //special case, let this one pass, nobody should be changing it as a user anyway
                    if(p.Name.Equals("ID"))
                        continue;
                    

                    var setMethod = p.GetSetMethod(false);

                    if(setMethod == null)
                    {
                        Console.WriteLine("WARNING: Property " + p.Name + " on Type " + type.Name + " has no set or it is not public;");
                        continue;
                    }

                    if (MightBeCouldBeMaybeAutoGeneratedInstanceProperty(p))
                    {
                        
                        char firstLetter = p.Name.ToLower()[0];
                        string fieldName = "_" + firstLetter + p.Name.Substring(1);
                        string typeName = p.PropertyType.Name;
                        
                        if (typeName == "String")
                            typeName = "string";
                        if (typeName == "Int32")
                            typeName = "int";
                        if (typeName == "Boolean")
                            typeName = "bool";

                        if (p.PropertyType == typeof (int?))
                            typeName = "int?";

                        if (p.PropertyType == typeof (DateTime?))
                            typeName = "DateTime?";

                        suggestedFieldDeclarations += "private " + typeName + " " + fieldName + ";" + Environment.NewLine;

                        suggestedMethodWrappers += "public " + typeName + " " + p.Name + Environment.NewLine;
                        suggestedMethodWrappers += "{" + Environment.NewLine;
                        suggestedMethodWrappers += "\tget { return " + fieldName +";}" + Environment.NewLine;
                        suggestedMethodWrappers += "\tset { SetField(ref " + fieldName +",value);}" + Environment.NewLine;
                        suggestedMethodWrappers += "}" + Environment.NewLine;
                    }

                    if(!setMethod.IsAbstract)
                    {
                        /*var instructions = setMethod.GetInstructions();

                        bool foundINotify = false;
                    
                        foreach (Instruction instruction in instructions)
                        {
                            MethodInfo methodInfo = instruction.Operand as MethodInfo;

                            if (methodInfo != null)
                                if (methodInfo.Name.Equals("SetField") || methodInfo.Name.Equals("OnPropertyChanged"))
                                    foundINotify = true;
                        }

                        if(!foundINotify)
                            _fails.Add("FAIL:Set Method for property " + p.Name + " on Type " + type.Name + " does not include an Instruction calling method 'SetField' or 'OnPropertyChanged'");
                        
                        */
                    }
                }
                

                if(!string.IsNullOrWhiteSpace(suggestedMethodWrappers))
                {
                    Console.WriteLine("CODE SUGGESTION FOR " + type.Name);
                    Console.WriteLine("#region Database Properties");

                    Console.WriteLine(suggestedFieldDeclarations);
                    Console.WriteLine(suggestedMethodWrappers);

                     Console.WriteLine("#endregion");
                }

            }

            AnalyseRelationshipPropertyUsages();

            foreach (string fail in _fails)
                Console.WriteLine(fail);

            Assert.AreEqual(0, _fails.Count);
        }

        private void AnalyseRelationshipPropertyUsages()
        {
            foreach (var t in mef.GetAllTypes())
            {
                if (!t.IsClass)
                    continue;

                //don't worry about the ToString method on classes that are IInjectKnown
                var toStringMethod = t.GetMethod("ToString", new Type[0]);

                //it doesn't have any ToString methods!
                if (toStringMethod == null)
                    continue;

                if (toStringMethod.DeclaringType == typeof (System.Object))
                    continue;

                if (toStringMethod.DeclaringType == typeof (MarshalByRefObject))
                    continue;
                /*
                IList<Instruction> instructions = null;
                try
                {
                    instructions = toStringMethod.GetInstructions();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (instructions != null)
                    foreach (Instruction instruction in instructions)
                    {
                        MethodInfo methodInfo = instruction.Operand as MethodInfo;

                        if (methodInfo != null)
                        {
                            //is it a call to property
                            PropertyInfo prop;

                            if (RelationshipPropertyInfos.TryGetBySecond(methodInfo, out prop))
                            {

                                //It doesn't look injected but it is
                                if(t == typeof(JoinInfo))
                                    continue;

                                //if we are injectable for it
                        if( t.GetInterfaces().Any(x =>
                              x.IsGenericType &&
                              x.GetGenericTypeDefinition() == typeof(IInjectKnown<>) &&
                              x.GetGenericArguments()[0] == prop.PropertyType))
                            continue;

                                _fails.Add("FAIL: ToString method in Type " + t.FullName + " uses Relationship PropertyInfo " + prop.Name);
                            }
                        }
                    }*/
            }
        }


        private bool IsRelationshipProperty(Type propertyType)
        {
            //If the Property is a maps directly to database object type
            if(typeof(IMapsDirectlyToDatabaseTable).IsAssignableFrom(propertyType))
                return true;

            //If the Property is an array of them e.g. Catalogue[]
            if (propertyType.IsArray)
                return typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(propertyType.GetElementType());

            //or it's a generic collection of them e.g. IEnumerable<Catalogue>
            if (propertyType.IsGenericType &&
                propertyType.GetGenericArguments().Any(g => typeof (IMapsDirectlyToDatabaseTable).IsAssignableFrom(g)))
                return true;

            return false;
        }
        
        public bool MightBeCouldBeMaybeAutoGeneratedInstanceProperty(PropertyInfo info)
        {
            bool mightBe = info.GetGetMethod()
                               .GetCustomAttributes(
                                   typeof(CompilerGeneratedAttribute),
                                   true
                               )
                               .Any();
            if (!mightBe)
            {
                return false;
            }


            bool maybe = info.DeclaringType
                             .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                             .Where(f => f.Name.Contains(info.Name))
                             .Where(f => f.Name.Contains("BackingField"))
                             .Where(
                                 f => f.GetCustomAttributes(
                                     typeof(CompilerGeneratedAttribute),
                                     true
                                 ).Any()
                             )
                             .Any();

            return maybe;
        }
    }
}
