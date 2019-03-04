using MethodDecorator.Fody.Interfaces;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace CSharpRegexTools4Npp.Utils
{

    // Any attribute which provides OnEntry/OnExit/OnException with proper args
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Assembly | AttributeTargets.Module)]
    public class InterceptorAttribute : Attribute, IMethodDecorator
    {
        private object instance;
        private string method;
        private object[] args;

        // instance, method and args can be captured here and stored in attribute instance fields
        // for future usage in OnEntry/OnExit/OnException
        public void Init(object instance, MethodBase method, object[] args)
        {
            this.instance = instance;
            this.method = method.Name;
            this.args = args;
        }

        public void OnEntry()
        {
            File.AppendAllText(@"C:\logs.log", $"Entry in {method} on {instance} with {string.Join(",", args)}");
        }

        public void OnExit()
        {
            File.AppendAllText(@"C:\logs.log", $"Exit in {method} on {instance} with {string.Join(",", args)}");
        }

        public void OnException(Exception exception)
        {
            File.AppendAllText(@"C:\logs.log", $"Exception in {method} on {instance} with {string.Join(",", args)}");
        }
    }
}
