using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MVsToolkit.Preferences.Editor;
using UnityEngine;

namespace MVsToolkit.Commands
{
    public static class MVsCommandManager
    {
        static Dictionary<string, CommandAttribute> commands;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (!MVsPrefs<MVsCommandsValues>.Values.UseCommands) return;

            commands = new();

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => MVsPrefs<MVsCommandsValues>.Values.AssemblyNames
                    .Contains(a.GetName().Name))
                .ToArray();

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type type in assembly.GetTypes())
                {
                    MethodInfo[] methods = type.GetMethods(
                        BindingFlags.Public |
                        BindingFlags.NonPublic |
                        BindingFlags.Static |
                        BindingFlags.Instance);

                    foreach (MethodInfo method in methods)
                    {
                        CommandAttribute cmdAttr = method.GetCustomAttribute<CommandAttribute>();
                        if (cmdAttr == null)
                            continue;

                        if (commands.ContainsKey(cmdAttr.CmdName))
                        {
                            UnityEngine.Debug.LogWarning(
                                $"Command '{cmdAttr.CmdName}' already exists");
                            continue;
                        }

                        cmdAttr.SetMethod(method);
                        commands.Add(cmdAttr.CmdName, cmdAttr);
                    }
                }
            }
        }

        public static bool TryGetCmd(string cmdName, out CommandAttribute cmd)
        {
            if (commands.ContainsKey(cmdName))
            {
                cmd = commands[cmdName];
                return true;
            }
            cmd = null;
            return false;
        }

        public static void Execute(string cmdName)
        {
            if (!commands.TryGetValue(cmdName, out CommandAttribute cmd))
            {
                UnityEngine.Debug.LogWarning($"Command '{cmdName}' not found");
                return;
            }

            MethodInfo method = cmd.Method;

            object instance = null;

            if (!method.IsStatic)
            {
                Type type = method.DeclaringType;

                instance = UnityEngine.Object.FindAnyObjectByType(type);

                if (instance == null)
                {
                    UnityEngine.Debug.LogError(
                        $"No instance of '{type.Name}' found in scene for command '{cmdName}'. " +
                        $"The method '{method.Name}' is not static.");
                    return;
                }
            }

            method.Invoke(instance, null);
        }

        public static CommandAttribute[] QueryCommands(string query, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(query) || commands == null)
                return Array.Empty<CommandAttribute>();

            string q = query.ToLowerInvariant();

            var results = commands.Values
                .Where(cmd => cmd.CmdName.ToLowerInvariant().Contains(q))
                .ToArray();

            if (maxLength > 0 && results.Length > maxLength)
                return results.Take(maxLength).ToArray();

            return results;
        }

        public static CommandAttribute[] GetCommands()
        {
            return commands.Values.ToArray();
        }
    }
}