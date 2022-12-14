using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace CropsYieldSeeds
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class CropsYieldSeeds : BaseUnityPlugin
    {
        // Mod Configuration
        private const string ModGuid = Author + "." + ModName;
        private const string ModName = nameof(CropsYieldSeeds);
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
        static class PickablePrefixDropPatch
        {
            [HarmonyPrefix]
            [UsedImplicitly]
            private static void SetCropSeed(Pickable prefab, int offset, int stack)
            {
                if (prefab == null)
                    return;
                if (!CropSeedObjDictionary.ContainsKey(prefab.name)) return;
                CropSeedObjDictionary[prefab.name] ??= ObjectDB.instance
                    .GetItemPrefab(
                        $"{prefab.name}Seeds");
            }
        }

        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Drop))]
        static class PickablePostfixDropPatch
        {
            [HarmonyPrefix]
            [UsedImplicitly]
            // ReSharper disable once InconsistentNaming
            private static void SpawnCropSeeds(Pickable prefab, int offset, int stack, Pickable __instance)
            {
                if (!CropSeedObjDictionary.ContainsKey(prefab.name)) return;
                var seedStack = Random.Range(0, 3);
                for (var i = 0; i < seedStack; i++)
                {
                    var seeds = CropSeedObjDictionary[prefab.name];
                    var position = __instance.transform.position;
                    Instantiate(seeds, position, Quaternion.identity);
                }
            }
        }
    }
}