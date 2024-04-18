using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace MapDLLInjector
{
    /// <summary>
    /// A Harmony patch for the UGCSceneSO.Load method, which is responsible for dynamically loading
    /// assemblies based on the scene's bundle path. This class enhances the mod's capability to load
    /// custom DLLs for specific maps at runtime.
    /// </summary>
    /// <remarks>
    /// This class maintains a dictionary of loaded assemblies to prevent reloading of assemblies
    /// during the runtime of the application. It uses a Harmony prefix to intercept the loading process
    /// of UGC scenes and inject custom DLLs where necessary.
    /// </remarks>
    [HarmonyPatch(typeof(UGCSceneSO), nameof(UGCSceneSO.Load))]
    class UGCSceneSOPatch
    {
        public static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        /// <summary>
        /// Harmony prefix method that logs the loading attempt and calls the injection method for the DLL.
        /// </summary>
        /// <param name="__instance">The instance of UGCSceneSO being loaded.</param>
        /// <returns>Always returns true to continue with the original method execution.</returns>
        [HarmonyPrefix]
        static bool Prefix(UGCSceneSO __instance)
        {
            Logger.Log($"Searching for map DLL at {__instance._bundlePath}");
            try
            {
                InjectDLLForMap(__instance._bundlePath);
            }
            catch (Exception ex)
            {
                Logger.LogError($"An Error occured when loading DLL for map: {ex.Message} : {ex.StackTrace}");
            }
            return true;
        }

        /// <summary>
        /// Injects a DLL based on the bundle path of the map, loading it if it hasn't been loaded yet.
        /// </summary>
        /// <param name="bundlePath">The path to the bundle for which the DLL needs to be injected.</param>
        public static void InjectDLLForMap(string bundlePath)
        {
            var pathArray = bundlePath.Split('\\');
            var mapName = pathArray[pathArray.Length - 1];
            var path = GetDLLPath(mapName.Split('.')[0]); // Remove .bundle from the name if it exists
            if (!assemblies.ContainsKey(path))
            {
                LoadDLL(path);
                return;
            }
            Logger.Log($"Assembly {path} is already loaded");
        }

        /// <summary>
        /// Constructs the path for the DLL associated with the map name.
        /// </summary>
        /// <param name="mapName">The name of the map for which the DLL path is required.</param>
        /// <returns>The full path to the DLL.</returns>
        private static string GetDLLPath(string mapName)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string dllDirectory = Path.Combine(documentsPath, "BMX Streets", "Maps", mapName);
            var retVal = Path.Combine(dllDirectory, $"{mapName}.dll");
            Logger.Log($"DLL path to check: {retVal}");
            return retVal;
        }

        /// <summary>
        /// Loads the DLL from the specified path and registers all suitable types within it.
        /// </summary>
        /// <param name="path">The path from which the DLL is loaded.</param>
        private static void LoadDLL(string path)
        {
            if (!File.Exists(path))
            {
                var arr = path.Split('\\');
                Logger.Log($"No DLL found for map {arr[arr.Length - 1].Replace(".dll", "")}");
                return;
            }
            try
            {
                Logger.Log($"Loading DLL: {path}");
                Assembly loadedAssembly = Assembly.LoadFrom(path);
                RegisterAllInAssembly(loadedAssembly);
                Logger.Log($"DLL loaded successfully: {loadedAssembly.FullName}");
                assemblies.Add(path, loadedAssembly);
            }
            catch (Exception e)
            {
                Logger.LogError("Failed to load DLL: " + e.Message);
            }
        }

        /// <summary>
        /// Registers all MonoBehaviour-derived types in the loaded assembly within the IL2CPP domain.
        /// </summary>
        /// <param name="assembly">The assembly containing the types to register.</param>
        private static void RegisterAllInAssembly(Assembly assembly)
        {
            Logger.Log($"Attempting to register all types in: {assembly.FullName}");
            assembly.GetTypes().ToList().ForEach(T =>
            {
                Logger.Log($"Attempting to register type: {T.Name}");
                if (T.IsSubclassOf(typeof(MonoBehaviour)))
                {
                    if (T.BaseType.IsSubclassOf(typeof(MonoBehaviour)))
                    {
                        RegisterMonoBehaviourInIl2Cpp(T.BaseType);
                    }
                    RegisterMonoBehaviourInIl2Cpp(T);
                }
            });
        }

        /// <summary>
        /// Registers a single MonoBehaviour type in the IL2CPP domain if not already registered.
        /// </summary>
        /// <param name="customType">The type to register.</param>
        public static void RegisterMonoBehaviourInIl2Cpp(Type customType)
        {
            Logger.Log($"Registering type: [{customType?.FullName}]");
            if (!ClassInjector.IsTypeRegisteredInIl2Cpp(customType))
            {
                MethodInfo methodInfo = typeof(ClassInjector).GetMethod("RegisterTypeInIl2Cpp", new Type[] { typeof(Type) });
                if (methodInfo != null)
                {
                    methodInfo.Invoke(null, new object[] { customType });
                    Logger.Log("Registered " + customType.FullName + " in IL2CPP");
                }
                else
                {
                    Logger.Log("Error: Method 'RegisterTypeInIl2Cpp' not found.");
                }
            }
            else
            {
                Logger.Log("Type already registered: " + customType.FullName);
            }
        }
    }

}

