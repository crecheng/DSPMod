using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace BiggerSeed
{
    [BepInPlugin("crecheng.BiggerSeed", "BiggerSeed", Version)]
    public class BiggerSeed:BaseUnityPlugin
    {
        const string Version = "1.0.0";

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(BiggerSeed), null);
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIGalaxySelect), "OnSeedInputSubmit")]
        static bool BigSeed(UIGalaxySelect __instance)
        {
            var _this = __instance;
            long num1 = -1;
            InputField seedInput = Traverse.Create(__instance).Field("seedInput").GetValue<InputField>();
            GameDesc gameDesc = Traverse.Create(__instance).Field("gameDesc").GetValue<GameDesc>();
            if (long.TryParse(seedInput.text, out num1))
            {
                num1 = Math.Abs(num1);
                if (num1 > int.MaxValue)
                {
                    num1 = int.MaxValue;
                }
                gameDesc.galaxySeed = (int)num1;
            }
            _this.SetStarmapGalaxy();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIGalaxySelect), "Rerand")]
        static bool SeedRadom(UIGalaxySelect __instance)
        {
            GameDesc gameDesc = Traverse.Create(__instance).Field("gameDesc").GetValue<GameDesc>();
            System.Random random = Traverse.Create(__instance).Field("random").GetValue<System.Random>();
            gameDesc.galaxySeed = random.Next(int.MaxValue);
            __instance.SetStarmapGalaxy();
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIGalaxySelect), "OnSeedInputValueChange")]
        static bool MaxLenth(UIGalaxySelect __instance)
        {
            InputField seedInput = Traverse.Create(__instance).Field("seedInput").GetValue<InputField>();
            if (seedInput.isFocused && seedInput.text.Length > 12)
            {
                long num = -1;
                if (long.TryParse(seedInput.text, out num))
                {
                    if (num < 0)
                    {
                        num = -num;
                    }
                    if (num > int.MaxValue)
                    {
                        num = int.MaxValue;
                    }
                    seedInput.text = num.ToString();
                }
                seedInput.text = seedInput.text.Substring(0, 12);
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIGalaxySelect), "UpdateUIDisplay")]
        static void func(UIGalaxySelect __instance, GalaxyData galaxy)
        {

            InputField seedInput = Traverse.Create(__instance).Field("seedInput").GetValue<InputField>();
            seedInput.characterLimit = 12;
            seedInput.text = galaxy.seed.ToString("00 0000 0000");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIGalaxySelect), "UpdateUIDisplay")]
        static void func1(UIGalaxySelect __instance, GalaxyData galaxy)
        {
            InputField seedInput = Traverse.Create(__instance).Field("seedInput").GetValue<InputField>();
            seedInput.text = galaxy.seed.ToString("00 0000 0000");
        }
    }
}
