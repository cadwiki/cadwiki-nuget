using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Microsoft.VisualBasic;

namespace cadwiki.DllReloader.AutoCAD
{
    public class CommandRemover
    {

        [DllImport("cadwiki.AcRemoveCmdGroup.dll", EntryPoint = "removeCommand", CharSet = CharSet.Auto)]
        public static extern int RemoveCommand([MarshalAs(UnmanagedType.LPWStr)] StringBuilder groupName, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder commandGlobalName);

        public static void RemoveAllCommandsFromiExtensionAppAssembly(Document doc, Assembly iExtensionAppAssembly, string dllPath)
        {
            Type[] currentTypes = NetUtils.AssemblyUtils.GetTypesSafely(iExtensionAppAssembly);
            var commandMethodAttributesToMethodInfos = AcadAssemblyUtils.GetCommandMethodDictionarySafely(currentTypes);
            if (doc is not null)
            {
                if (commandMethodAttributesToMethodInfos.Count == 0)
                {
                    doc.Editor.WriteMessage(Environment.NewLine + "No commands found to remove.");
                }
                else
                {
                    doc.Editor.WriteMessage(Environment.NewLine + "Removing all commands from current assembly.");
                    RemoveCommands(doc, dllPath, commandMethodAttributesToMethodInfos);
                }
            }

        }

        private static void RemoveCommands(Document doc, string dllPath, Dictionary<CommandMethodAttribute, MethodInfo> commandMethodAttributesToMethodInfos)
        {
            foreach (KeyValuePair<CommandMethodAttribute, MethodInfo> dictionaryItem in commandMethodAttributesToMethodInfos)
            {
                var commandMethodAttribute = dictionaryItem.Key;
                var methodInfo = dictionaryItem.Value;
                string commandLineMethodString = commandMethodAttribute.GlobalName;
                string commandGroupName = commandMethodAttribute.GroupName;
                var groupName = new StringBuilder(256);
                groupName.Append(commandGroupName);
                var commandGlobalName = new StringBuilder(256);
                commandGlobalName.Append(commandLineMethodString);
                string dllFolder = Path.GetDirectoryName(dllPath);
                int wasCommandUndefined = RemoveCommand(groupName, commandGlobalName);
                if (wasCommandUndefined == 0)
                {
                    doc.Editor.WriteMessage(Environment.NewLine + "Command successfully removed: " + commandGlobalName.ToString());
                }
                else
                {
                    doc.Editor.WriteMessage(Environment.NewLine + "Remove command failed: " + commandLineMethodString + ".");
                }
            }
        }
    }
}