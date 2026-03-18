using System;
using System.Reflection;

namespace MVsToolkit.Commands
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public readonly string CmdName;
        public readonly string CmdDescription;

        public MethodInfo Method;

        public CommandAttribute(string name, string description = "")
        {
            CmdName = name;
            CmdDescription = description;
        }

        public void SetMethod(MethodInfo method)
        {
            Method = method;
        }
    }
}