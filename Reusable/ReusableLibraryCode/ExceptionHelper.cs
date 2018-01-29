using System;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace ReusableLibraryCode
{
    /// <summary>
    /// Helper for unwrapping Exception.InnerExceptions and ReflectionTypeLoadExceptions.LoaderExceptions into a single flat message string of all errors.
    /// </summary>
    public class ExceptionHelper
    {
        [Pure]
        public static string ExceptionToListOfInnerMessages(Exception e, bool includeStackTrace=false)
        {
            string message = e.Message;
            if (includeStackTrace)
                message += Environment.NewLine + e.StackTrace;

            if (e is ReflectionTypeLoadException)
                foreach (Exception loaderException in ((ReflectionTypeLoadException) e).LoaderExceptions)
                    message += Environment.NewLine + ExceptionToListOfInnerMessages(loaderException, includeStackTrace);

            if (e.InnerException != null)
                message += Environment.NewLine + ExceptionToListOfInnerMessages(e.InnerException, includeStackTrace);

            return message;
        }
    }
}
