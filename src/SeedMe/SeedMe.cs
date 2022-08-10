using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace SeedMe
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class SeedMe : BaseUnityPlugin
    {
        // Mod Configuration
        private const string ModGuid = Author + "." + ModName;
        private const string ModName = nameof(SeedMe);
        private const string ModVersion = "1.0.0";
        private const string Author = "Quantumheart";

        private static readonly Dictionary<string, GameObject?>
            CropSeedObjDictionary = new()
            {
                { "Carrot", null },
                { "Turnip", null },
                { "Onion", null }
            };

        private readonly Harmony _harmony = new(ModGuid);
        private static readonly ManualLogSource LogSource = new(ModName);

        private void Awake()
        {
            BepInEx.Logging.Logger.Sources.Add(LogSource);
            var assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }

        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }

        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Drop))]
        static class PickableDropPatch
        {
            [HarmonyPrefix]
            [UsedImplicitly]
            // ReSharper disable once InconsistentNaming
            static void PostFix(Pickable prefab, int offset, int stack, Pickable __instance)
            {
                if (prefab == null)
                    return;
                if (!CropSeedObjDictionary.ContainsKey(prefab.name)) return;
                CropSeedObjDictionary[prefab.name] ??= ObjectDB.instance
                    .GetItemPrefab(
                        $"{prefab.name}Seeds");

                if (CropSeedObjDictionary[prefab.name] is not null)
                {
                    var seedStack = Random.Range(0, 3);
                    for (var i =0; i < seedStack; i++)
                    {
                        var seeds = CropSeedObjDictionary[prefab.name];
                        var position = __instance.transform.position;
                        Instantiate(seeds, position, Quaternion.identity);
                    }
                }
            }
        }
    }
}