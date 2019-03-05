using MethodDecorator.Fody.Interfaces;
using System;
using System.Reflection;

namespace CSharpRegexTools4Npp.Utils
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class InterceptorAttribute : Attribute, IMethodDecorator
    {
        // instance, method and args can be captured here and stored in attribute instance fields
        // for future usage in OnEntry/OnExit/OnException
        public void Init(object instance, MethodBase method, object[] args)
        {
        }

        public void OnEntry()
        {
        }

        public void OnExit()
        {
        }

        public void OnException(Exception exception)
        {
        }
    }
}
