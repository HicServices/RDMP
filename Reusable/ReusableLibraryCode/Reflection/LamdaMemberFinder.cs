using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ReusableLibraryCode.Reflection
{
    public class LamdaMemberFinder<T>
    {

        public string GetMethod(Expression<Func<T, object>> expression)
        {
            return GetMethod<T>(expression);
        }

        public string GetMethod<T>(Expression<Func<T, object>> expression)
        {
            return ((MethodCallExpression) expression.Body).Method.Name;
            var unaryExpression = (UnaryExpression) expression.Body;
            var methodCallExpression = (MethodCallExpression) unaryExpression.Operand;
            var methodInfoExpression = (ConstantExpression) methodCallExpression.Arguments.Last();
            var methodInfo = (MethodInfo) methodInfoExpression.Value;
            //return methodInfo;
        }

        public string GetProperty(Expression<Func<T, object>> expression)
        {
            return GetProperty<T>(expression);
        }

        public string GetProperty<T>(Expression<Func<T, object>> expression)
        {
            return ((System.Linq.Expressions.MemberExpression)expression.Body).Member.Name;
        }

        public string GetEvent(Expression<Func<T, object>> expression)
        {
            return GetEvent<T>(expression);
        }

        public string GetEvent<T>(Expression<Func<T, object>> expression)
        {
            return ((System.Linq.Expressions.MemberExpression)expression.Body).Member.Name;
        }
    }
}
