using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PostSharp.Aspects;
using PostSharp.Extensibility;
using PostSharp.Serialization;

namespace HIC.Logging
{
    [PSerializable]
    public class LoggingAspect : OnMethodBoundaryAspect
    {
        private LogManager logManager;
        private string[] parameterNames;

        public static Func<LogManager> DoInit { get; set; }  

        public override void CompileTimeInitialize(MethodBase method, AspectInfo aspectInfo)
        {
            parameterNames = method.GetParameters().Select(p => p.Name).ToArray();
        }

        public override void OnEntry(MethodExecutionArgs args)
        {
            Console.WriteLine("The {0} method has been entered.", args.Method.Name);
        }

        public override void OnSuccess(MethodExecutionArgs args)
        {
            if (logManager == null)
                logManager = DoInit();

            Console.WriteLine("The {0} method executed successfully.", args.Method.Name);
            logManager.AuditConfigChange("Success: ", args.Method.Name, args.GetArguments(parameterNames));
        }

        public override void OnExit(MethodExecutionArgs args)
        {
            Console.WriteLine("The {0} method has exited.", args.Method.Name);
        }

        public override void OnException(MethodExecutionArgs args)
        {
            Console.WriteLine("An exception was thrown in {0}.", args.Method.Name);
        }
        
        public override void RuntimeInitialize(MethodBase method)
        {
            if (DoInit != null)
                logManager = DoInit();
        }
    }

    public static class Extensions
    {
        public static IEnumerable<Tuple<string, object>> GetArguments(this MethodExecutionArgs method, string[] parameterNames)
        {
            for (int i = 0; i < method.Arguments.Count; i++)
                yield return Tuple.Create(parameterNames[i], method.Arguments[i] ?? "null");
        }
    }
}