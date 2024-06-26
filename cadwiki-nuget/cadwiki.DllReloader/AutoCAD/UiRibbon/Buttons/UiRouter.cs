using System.Reflection;

namespace cadwiki.DllReloader.AutoCAD.UiRibbon.Buttons
{
    public class UiRouter
    {
        public string FullClassName;
        public string MethodName;
        public object[] Parameters;
        public string AssemblyName;
        public AutoCADAppDomainDllReloader NetReloader;
        public Assembly IExtensionAppAssembly;

        public UiRouter(string assemblyName, string fullClassName, string methodName, object[] parameters, AutoCADAppDomainDllReloader netReloader, Assembly iExtensionAppAssembly)
        {
            AssemblyName = assemblyName;
            FullClassName = fullClassName;
            MethodName = methodName;
            Parameters = parameters;
            NetReloader = netReloader;
            IExtensionAppAssembly = iExtensionAppAssembly;
        }
    }
}