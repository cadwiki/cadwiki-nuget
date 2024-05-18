using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace cadwiki.NetUtils
{

    public class AssemblyUtils
    {
        public static Assembly GetCurrentlyExecutingAssembly()
        {
            return Assembly.GetExecutingAssembly();
        }

        public static Type[] GetTypesSafely(Assembly @assembly)
        {
            if (assembly is not null)
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Type[] foundTypes = ex.Types;
                    var validTypes = new List<Type>();
                    foreach (Type foundType in foundTypes)
                    {
                        if (foundType is not null)
                        {
                            validTypes.Add(foundType);
                        }
                    }
                    Type[] validTypeArray = validTypes.ToArray();
                    return validTypeArray;
                }
            }
            else
            {
                return null;
            }

        }

        public static Version GetVersion(Assembly @assembly)
        {
            var assemblyVersion = assembly.GetName().Version;
            return assemblyVersion;
        }


        public static string GetFolderLocationFromCodeBase(Assembly @assembly)
        {
            string fileLocation = GetFileLocationFromCodeBase(assembly);
            if (string.IsNullOrEmpty(fileLocation))
            {
                return null;
            }
            return Path.GetDirectoryName(fileLocation);
        }

        public static string GetFileLocationFromCodeBase(Assembly @assembly)
        {
            string location = assembly.Location;
            if (string.IsNullOrEmpty(location))
            {
                location = assembly.CodeBase;
                location = location.Replace("file:///", "");
            }
            return location;
        }

        public static string ReadEmbeddedResourceToString(Assembly @assembly, string searchPattern)
        {
            var reader = GetStreamReaderFromEmbeddedResource(assembly, searchPattern);
            if (reader is not null)
            {
                string templateString = reader.ReadToEnd();
                return templateString;
            }
            return null;
        }

        public static StreamReader GetStreamReaderFromEmbeddedResource(Assembly @assembly, string searchPattern)
        {
            string resourceName = @assembly.GetManifestResourceNames().FirstOrDefault(x => x.Contains(searchPattern));
            if (resourceName is not null)
            {
                var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream is not null)
                {
                    var reader = new StreamReader(stream, System.Text.Encoding.Default);
                    return reader;
                }
            }
            return null;
        }


    }
}