using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;
using Autodesk.AutoCAD.Runtime;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;

namespace cadwiki.DllReloader.AutoCAD
{
    public class AcadAssemblyUtils
    {
        public static Dictionary<CommandMethodAttribute, MethodInfo> GetCommandMethodDictionarySafely(Type[] types)
        {
            var commandMethodAttributesToMethodInfos = new Dictionary<CommandMethodAttribute, MethodInfo>();
            foreach (Type @type in types)
            {
                MethodInfo[] methodInfos = type.GetMethods();

                foreach (MethodInfo methodInfo in methodInfos)
                {
                    var commandMethodAttributeObject = DoesMethodInfoHaveAutoCADCommandAttribute(methodInfo);
                    if (commandMethodAttributeObject is not null)
                    {
                        CommandMethodAttribute commandMethodAttribute = (CommandMethodAttribute)commandMethodAttributeObject;
                        commandMethodAttributesToMethodInfos.Add(commandMethodAttribute, methodInfo);
                    }

                }
            }
            return commandMethodAttributesToMethodInfos;
        }
        public static object GetAppObjectSafely(Type[] types)
        {
            if (types is not null)
            {
                foreach (Type @type in types)
                {
                    var interfaceList = @type.GetInterfaces().ToList();
                    bool isClassIExtensionApplication = false;

                    if (interfaceList.Contains(typeof(IExtensionApplication)))
                    {
                        isClassIExtensionApplication = true;
                    }

                    if (type.IsClass & isClassIExtensionApplication)
                    {
                        var appObject = Activator.CreateInstance(type);
                        return appObject;
                    }
                }
            }
            return null;
        }


        public static Assembly GetNewestAssembly(Assembly[] assemblies, string assemblyName, string dllLocation)
        {
            Assembly newestAsm = null;
            Assembly match = null;
            foreach (Assembly domainAssembly in assemblies)
            {
                var domainAssemblyName = domainAssembly.GetName();
                if (domainAssemblyName.Name.ToLower().Equals(assemblyName.ToLower()))
                {
                    match = domainAssembly;
                    string matchVersionNumber = GetAssemblyVersionFromFullName(domainAssembly.FullName);
                    if (newestAsm is null)
                    {
                        newestAsm = match;
                    }
                    else
                    {
                        string newestVersionNumber = GetAssemblyVersionFromFullName(newestAsm.FullName);
                        int comparisonResult = CompareFileVersion(matchVersionNumber, newestVersionNumber);
                        if (comparisonResult == 1)
                        {
                            newestAsm = domainAssembly;
                        }
                    }
                }
            }

            return newestAsm;
        }

        public static string GetAssemblyVersionFromFullName(string fullName)
        {
            string[] strArry = Strings.Split(fullName, ", ");
            string version = strArry[1];
            string[] verArry = Strings.Split(version, "=");
            string versionNumber = verArry[1];
            return versionNumber;
        }

        public static int CompareFileVersion(string strFileVersion1, string strFileVersion2)
        {
            // -1 = File Version 1 is less than File Version 2
            // 0  = Versions are the same
            // 1  = File version 1 is greater than File Version 2
            int intResult = 0;
            string[] strAryFileVersion1 = Strings.Split(strFileVersion1, ".");
            string[] strAryFileVersion2 = Strings.Split(strFileVersion2, ".");
            int i;
            var loopTo = Information.UBound(strAryFileVersion1);
            for (i = 0; i <= loopTo; i++)
            {
                int num1 = Conversions.ToInteger(strAryFileVersion1[i]);
                int num2 = Conversions.ToInteger(strAryFileVersion2[i]);
                if (num1 > num2)
                {
                    intResult = 1;
                }
                else if (num1 < num2)
                {
                    intResult = -1;
                }
                // If we have found that the result is not > or <, no need to proceed
                if (intResult != 0)
                    break;
            }
            return intResult;
        }



        private static object DoesMethodInfoHaveAutoCADCommandAttribute(MethodInfo methodInfo)
        {
            object[] objectAttributes = methodInfo.GetCustomAttributes(true);
            foreach (object objectAttribute in objectAttributes)
            {
                if (ReferenceEquals(objectAttribute.GetType(), typeof(CommandMethodAttribute)))
                {
                    return objectAttribute;
                }
            }
            return null;
        }




    }


}