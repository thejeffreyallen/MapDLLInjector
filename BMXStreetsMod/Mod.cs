using MapDLLInjector;
using MelonLoader;


[assembly: MelonInfo(typeof(Mod), "Map Map Injector", "1.0.0", "Lineryder")]

namespace MapDLLInjector
{
    public class Mod : MelonMod
    {
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();

            var harmony = new HarmonyLib.Harmony("Map.Map.Injector");
            harmony.PatchAll();
        }

    }

}

