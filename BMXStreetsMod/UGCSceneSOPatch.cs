using HarmonyLib;
using Il2Cpp;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace MapDLLInjector
{
    [HarmonyPatch(typeof(UGCSceneSO), nameof(UGCSceneSO.Load))]
    class UGCSceneSOPatch
    {
        public static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();

        [HarmonyPrefix]
        static bool Prefix(UGCSceneSO __instance)
        {
            Logger.Log($"Loading DLL for Map at {__instance._bundlePath}");
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

        public static void InjectDLLForMap(string bundlePath)
        {
            var pathArray = bundlePath.Split('\\');
            var mapName = pathArray[pathArray.Length - 1];
            var path = GetDLLPath(mapName);
            if (!assemblies.ContainsKey(path))
            {
                LoadDLL(path);
                return;
            }
            Logger.Log($"Assembly {path} is loaded");
        }

        private static string GetDLLPath(string mapName)
        {
            // Get the path to the current user's Documents folder
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Construct the path to where the DLLs are stored
            string dllDirectory = Path.Combine(documentsPath, "BMX Streets", "Maps", mapName);
            // Combine the directory with the DLL's filename to get the full path
            var retVal = Path.Combine(dllDirectory, $"{mapName}.dll");

            Logger.Log($"DLL path to check: {retVal}");

            return retVal;
        }

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
                // Load the DLL
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

