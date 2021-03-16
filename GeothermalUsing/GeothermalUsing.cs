using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace GeothermalUsing
{
    [BepInPlugin("crecheng.GeothermalUsing", "GeothermalUsing", Version)]
    public class GeothermalUsing:BaseUnityPlugin
    {
        const string Version = "1.0.0";
        const int waterId = 1000;
        const int emtryB = 2206;
        const int fullB = 2207;
        const int ice = 1011;
        const int cmx = 1123;

        private void Start()
        {
             Harmony.CreateAndPatchAll(typeof(GeothermalUsing), null);
        }

        static long frame = 0;
        void Update()
        {
            frame++;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlanetFactory), "GameTick")]
        static void Geothermal(PlanetFactory __instance)
        {
            var factory = __instance;
            if (frame % 3 != 0)
                return;
            if (factory.planet.type != EPlanetType.Vocano||factory.planet.waterItemId!=-1)
                return;

            GameStatData statistics = GameMain.statistics;
            FactoryProductionStat factoryProductionStat = statistics.production.factoryStatPool[factory.factorySystem.factory.index];
            int[] productRegister = factoryProductionStat.productRegister;
            int[] consumeRegister = factoryProductionStat.consumeRegister;
            for (int i = 0; i < factory.transport.stationCursor; i++)
            {
                var sc = factory.transport.stationPool[i];
                if (sc != null && sc.storage != null)
                {

                    int item1 = -1;
                    int item2 = -1;
                    int item3 = -1;
                    int item4 = -1;
                    int item5 = -1;
                    for (int j = 0; j < sc.storage.Length; j++)
                    {
                        var st = sc.storage[j];
                        if (st.itemId > 0) {
                            if (st.itemId == ice )
                            {
                                item1 = j;
                            }
                            else if (st.itemId == waterId)
                            {
                                item2 = j;
                            }
                            else if (st.itemId == cmx)
                            {
                                item3 = j;
                            }
                            else if (st.itemId == emtryB)
                            {
                                item4 = j;
                            }
                            else if (st.itemId == fullB)
                            {
                                item5 = j;
                            }
                        }
                    }
                    if (item1 > -1)
                    {
                        if (item3 > -1&& sc.storage[item1].count>0)
                        {
                            if(sc.storage[item3].count< sc.storage[item3].max)
                            {
                                if (sc.energy < sc.energyMax - 100000)
                                    sc.energy += 100000;

                                sc.storage[item1].count--;
                                consumeRegister[item1]++;
                                sc.storage[item3].count++;
                                productRegister[item1]++;
                                if(item4>-1&&item5>-1&&sc.storage[item4].count>0&&sc.storage[item5].max> sc.storage[item5].count)
                                {
                                    if (frame % 20 == 0)
                                    {
                                        sc.storage[item4].count--;
                                        consumeRegister[item4]++;
                                        productRegister[item5]++;
                                        sc.storage[item5].count++;
                                    }
                                }
                            }
                        }

                    }
                    if (item2 > -1)
                    {
                        if (sc.storage[item2].count > 0 && item4 > -1 && item5 > -1 && sc.storage[item4].count > 0 && sc.storage[item5].max > sc.storage[item5].count)
                        {
                            if (sc.energy < sc.energyMax - 100000)
                                sc.energy += 100000;

                            sc.storage[item2].count--;
                            consumeRegister[item2]++;
                            if (frame % 20 == 0)
                            {
                                sc.storage[item4].count--;
                                consumeRegister[item4]++;
                                productRegister[item5]++;
                                sc.storage[item5].count++;
                            }
                        }

                    }

                }
            }
        }


    }
}
