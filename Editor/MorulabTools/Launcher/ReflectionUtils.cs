using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace MorulabTools.Launcher
{
    public static class ReflectionUtils
    {
        public static List<ToolCommandData> FindCommands(string rootPathFilter = "Morulab")
        {
            var commands = new List<ToolCommandData>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException) { continue; }

                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var method in methods)
                    {
                        var menuItemAttrs = method.GetCustomAttributes<MenuItem>(false);
                        var descAttr = method.GetCustomAttribute<MenuDescriptionAttribute>(false);

                        foreach (var menuItemAttr in menuItemAttrs)
                        {
                            string menuPath = menuItemAttr.menuItem;
                            if (menuItemAttr.validate) continue;

                            if (!string.IsNullOrEmpty(rootPathFilter) && !menuPath.StartsWith(rootPathFilter))
                            {
                                continue;
                            }

                            string relativePath = menuPath;
                            if (menuPath.StartsWith(rootPathFilter))
                            {
                                relativePath = menuPath.Substring(rootPathFilter.Length).TrimStart('/');
                            }

                            var parts = relativePath.Split('/');
                            string autoCategory = "General";
                            if (parts.Length > 1)
                            {
                                autoCategory = parts[0];
                            }

                            var cmd = new ToolCommandData
                            {
                                Path = menuPath,
                                Title = menuPath.Split('/').Last(),
                                TargetMethod = method,
                                Description = descAttr?.Description ?? "No description available.",
                                Category = descAttr?.Category ?? autoCategory,
                                IconName = descAttr?.IconName
                            };
                            commands.Add(cmd);
                        }
                    }
                }
            }
            return commands.OrderBy(c => c.Category).ThenBy(c => c.Title).ToList();
        }
    }
}
