using System.Collections.Generic;
using System.Linq;
using MapsDirectlyToDatabaseTable;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Repositories;
using Tests.Common;

namespace Rdmp.Core.Tests.DesignPatternTests
{
    public class DatabaseEntityConventionTests:UnitTests
    {
        /// <summary>
        /// This test checks that constructors of <see cref="DatabaseEntity"/> classes know what repo they are supposed to be written into
        /// e.g. <see cref="ICatalogueRepository"/> or <see cref="IDataExportRepository"/> (or a plugin one?) but definitely not just
        /// <see cref="IRepository"/>
        /// </summary>
        [Test]
        public void AllDatabaseEntitiesHaveTypedIRepository()
        {
            SetupMEF();

            List<string> problems = new List<string>();

            foreach (var type in MEF.GetAllTypes().Where(t => typeof(DatabaseEntity).IsAssignableFrom(t)))
            {
                foreach (var constructorInfo in type.GetConstructors())
                {
                    var parameters = constructorInfo.GetParameters();
                    
                    if (parameters.Any(p=>p.ParameterType == typeof(IRepository)))
                    {
                        problems.Add($"Constructor found on Type {type} that takes {nameof(IRepository)}, it should take either {nameof(IDataExportRepository)} or {nameof(ICatalogueRepository)}");
                    }
                }
            }

            foreach (var problem in problems)
                TestContext.Out.WriteLine(problem);

            Assert.IsEmpty(problems);
        }
    }
}