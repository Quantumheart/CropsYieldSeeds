using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

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
        private static readonly Dictionary<string, ItemDrop?> CropSeedObjDictionary = new()
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
            // config = new ModConfig(Config, new ServerSync.ConfigSync(ModGuid) { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = "1.0.0" });
            // if (config.EnableLocalization)
            // LoadLocalizedStrings();
            LogSource.LogInfo("SeedMe is awake!");
            var assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);

        }


        private void OnDestroy()
        {
            _harmony.UnpatchSelf();
        }
        
        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        public static class ZNetSceneAwake
        {
            public static void Postfix(ZNetScene __instance)
            {
                FinalInit();
            }

            private static void FinalInit()
            {
                CropSeedObjDictionary.Add("Carrot", null);
                CropSeedObjDictionary.Add("Turnip", null);
                CropSeedObjDictionary.Add("Onion", null);
                List<Object> result =  Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(d =>
                    CropSeedObjDictionary.Keys.Contains(d.name)).ToList();
                foreach (var itemDrop in result)
                {
                    LogSource.LogInfo(itemDrop.name);
                }
                foreach (var key in CropSeedObjDictionary.Keys)
                {
                    LogSource.LogInfo($"enabled {ObjectDB.instance.enabled}");
                    var seeds = ObjectDB.instance
                                        .GetItemPrefab($"{key}Seeds")
                                        .GetComponent<ItemDrop>();
                    LogSource.LogFatal($"SEEDS => {seeds}");
                    CropSeedObjDictionary[key] = seeds;
                }

            }
        }


        [HarmonyPatch(typeof(Pickable), nameof(Pickable.Drop))]
        static class PickableDropPatch
        {

            [HarmonyPostfix]
            static void PostFix(Pickable prefab, int offset, int stack)
            {
                if (prefab == null)
                    return;
                if (!CropSeedObjDictionary.ContainsKey(prefab.name)) return;
                // if valid vegetable load the prefab and add it to the dictionary 
                LogSource.LogInfo($"prefab name => {prefab.name}");
                var seeds = ObjectDB.instance
                                    .GetItemPrefab($"{prefab.name}Seeds")
                                    .GetComponent<ItemDrop>();
                CropSeedObjDictionary[prefab.name] = seeds;
                var item = CropSeedObjDictionary[prefab.name];
                if (item is not null)
                {
                    if (item.m_itemData is null)
                    {
                        return;
                    }
                    var randomNumber = Random.Range(0, 5);
                    CropSeedObjDictionary[prefab.name]!.m_itemData.m_stack = randomNumber;
                    var pos = prefab.transform.Find(prefab.name);
                    Instantiate(CropSeedObjDictionary[prefab.name], pos);
                    LogSource.LogInfo("success adding random!");
                    LogSource.LogInfo($"random stack number! {CropSeedObjDictionary[prefab.name]!.m_itemData.m_stack.ToString()}");
                };
                
            }
        }

        private static ItemDrop GetOrAddItemDropComponent(GameObject go) => go.GetComponent<ItemDrop>() ?? go.AddComponent<ItemDrop>();

        
    }
}