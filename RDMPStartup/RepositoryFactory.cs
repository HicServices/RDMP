using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Runtime.Remoting.Messaging;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Proxies;

namespace RDMPStartup
{
    public static class RepositoryFactory
    {
        public static CatalogueRepository CreateCatalogueRepository(DbConnectionStringBuilder sqlConnectionStringBuilder)
        {

            var catalogueRepository = new CatalogueRepository(sqlConnectionStringBuilder);
            return catalogueRepository;
            var dynamicProxy = new DynamicProxy<CatalogueRepository>(catalogueRepository);
            var lm = catalogueRepository.GetDefaultLogManager();

            dynamicProxy.BeforeExecute += (s, e) => lm.AuditConfigChange("Before Executing", e.MethodName, e.GetArguments());
            dynamicProxy.AfterExecute += (s, e) => lm.AuditConfigChange("After Executing", e.MethodName, e.GetArguments());
            dynamicProxy.ErrorExecuting += (s, e) => lm.AuditConfigChange("Error Executing", e.MethodName, e.GetArguments());

            dynamicProxy.Filter = _ => false;

            return dynamicProxy.GetTransparentProxy() as CatalogueRepository;
        }

        public static IEnumerable<Tuple<string,object>> GetArguments(this IMethodCallMessage method)
        {
            for (int i = 0; i < method.ArgCount; i++)
                yield return Tuple.Create(method.GetArgName(i), method.GetArg(i));
        }
    }
}