using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace CodeGenTester
{
    /// <summary>
    /// Calls the generated code and executes it
    /// </summary>
    [Serializable]
    public class AssemblyRunner : MarshalByRefObject
    {
        /// <summary>
        /// Runs the specified type name.
        /// </summary>
        /// <param name="dllPath">The DLL path.</param>
        /// <param name="typeName">Name of the type.</param>
        /// <param name="methodName">Name of the method.</param>
        public static void Run(string dllPath, string typeName, string methodName)
        {
            //create a new domain with a config
            var setup = new AppDomainSetup();
            //directory where the dll and dependencies are
            setup.ApplicationBase = Path.GetDirectoryName(dllPath);
            if (File.Exists(dllPath + ".config"))
                setup.ConfigurationFile = dllPath + ".config";
            setup.ApplicationName = "TestRun";
            //Create the new domain
            var domain = AppDomain.CreateDomain("LoaderDomain", null, setup);
            try
            {
                //load this assembly and this type into the new domain
                var runner =
                    (AssemblyRunner)domain.CreateInstanceFromAndUnwrap(
                    Assembly.GetExecutingAssembly().Location,
                    typeof(AssemblyRunner).FullName);

                //other instance of this class in new domain loads dll
                runner.LoadDll(dllPath, typeName, methodName);
            }
            finally
            {
                //unload domain
                AppDomain.Unload(domain);
            }
        }

        private string _oldConfigPath;

        public void LoadDll(string filePath, string typeName, string methodName)
        {
            if (!File.Exists(filePath)) return;

            CheckForConfig(filePath);

            string location = Path.GetDirectoryName(filePath);
            AppDomain.CurrentDomain.AssemblyResolve +=
                delegate(object sender, ResolveEventArgs args)
                {
                    var findName = args.Name;
                    var assembly = LookInAppDomain(findName);
                    if (assembly != null)
                        return assembly;
                    var simpleName = new AssemblyName(findName).Name;
                    var assemblyPath = Path.Combine(location, simpleName) + ".dll";
                    if (File.Exists(assemblyPath))
                        return Assembly.LoadFrom(assemblyPath);
                    //can't find it
                    return null;
                };

            //load the assembly into bytes and load it
            var assemblyBytes = File.ReadAllBytes(filePath);
            var a = Assembly.Load(assemblyBytes);
            //find the type in the assembly
            var t = a.GetType(typeName, true);
            //find the method
            var run = t.GetMethod(methodName);
            try
            {
                //run it
                if (run.IsStatic)
                {
                    run.Invoke(null, new object[0]);
                }
                else
                {
                    var obj = Activator.CreateInstance(t);
                    run.Invoke(obj, new object[0]);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(_oldConfigPath))
                    SwapConfigFile(_oldConfigPath);
            }
        }

        private void CheckForConfig(string filePath)
        {
            var configFile = filePath + ".config";
            if (File.Exists(configFile))
            {
                var existingConfig = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
                if (!existingConfig.Equals(configFile, StringComparison.OrdinalIgnoreCase))
                {
                    _oldConfigPath = existingConfig;
                    SwapConfigFile(configFile);
                }
            }
        }

        private static void SwapConfigFile(string configFile)
        {
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", configFile);
            //reset the private state flag in ConfigurationManager
            var fiInit = typeof(ConfigurationManager).GetField(
                "s_initState",
                BindingFlags.NonPublic | BindingFlags.Static);
            if (fiInit != null)
                fiInit.SetValue(null, 0);
        }
        private static Assembly LookInAppDomain(string findName)
        {
            if (findName == Assembly.GetExecutingAssembly().FullName)
            {
                return Assembly.GetExecutingAssembly();
            }

            var currentAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (var i = 0; i < currentAssemblies.Length; i++)
            {
                if (currentAssemblies[i].FullName == findName)
                {
                    return currentAssemblies[i];
                }
            }
            return null;
        }

    }
}
