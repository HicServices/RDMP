using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace CatalogueLibraryTests.UserInterfaceTests
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    class UITimeoutAttribute : NUnitAttribute, IWrapTestMethod
    {
        private readonly TimeSpan _timeout;

        /// <summary>
        /// Allows <paramref name="timeout"/> for the test to complete before calling <see cref="Process.CloseMainWindow"/> and failing the test
        /// </summary>
        /// <param name="timeout">timeout in milliseconds</param>
        public UITimeoutAttribute(int timeout)
        {
            this._timeout = TimeSpan.FromMilliseconds(timeout);
        }

        /// <inheritdoc/>
        public TestCommand Wrap(TestCommand command)
        {
            return new TimeoutCommand(command, this._timeout);
        }

        private class TimeoutCommand : DelegatingTestCommand
        {
            private readonly TimeSpan _timeout;

            public TimeoutCommand(TestCommand innerCommand, TimeSpan timeout): base(innerCommand)
            {
                _timeout = timeout;
            }

            public override TestResult Execute(TestExecutionContext context)
            {
                var t = Task.Run(() => this.innerCommand.Execute(context));

                try
                {
                    if (!Debugger.IsAttached)
                    {
                        Task.WaitAny(Task.Delay(_timeout), t);
                        if(!t.IsCompleted)
                        {
                            Process.GetCurrentProcess().CloseMainWindow();
                            Assert.Fail("UI test did not complete after timeout");
                        }
                    }

                    return t.Result;
                }
                catch (AggregateException ae)
                {
                    throw ae.InnerException;
                }
            }
        }
    }
}
