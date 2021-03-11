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

namespace StationAutoCollect
{
    [BepInPlugin("crecheng.StationAutoCollet", "StationAutoCollet", StationAutoCollect.Version)]
    public class StationAutoCollect:BaseUnityPlugin
    {
        public const string Version = "1.0.1";

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(StationAutoCollect), null);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetFactory), "GameTick")]
        static void fun(PlanetFactory __instance, long time)
        {

            var factory = __instance;
            Dictionary<int, Queue<MinerHave>> Miner = new Dictionary<int, Queue<MinerHave>>();
            try
            {

                for (int i = 1; i < factory.factorySystem.minerCursor; i++)
                {
                    var ed = factory.factorySystem.minerPool[i];
                    if (ed.entityId > 0)
                    {
                        int itemid = ed.productId;
                        if (!Miner.ContainsKey(itemid))
                            Miner.Add(itemid, new Queue<MinerHave>());
                        Miner[itemid].Enqueue(new MinerHave(i, itemid, ed.productCount));
                    }
                }
            }
            catch
            {
                Debug.LogError("111");
            }


            for (int i = 1; i < factory.transport.stationCursor; i++)
            {
                var sc = factory.transport.stationPool[i];
                if (sc != null && Miner.Count > 0 && sc.storage != null)
                {
                    for (int j = 0; j < sc.storage.Length; j++)
                    {
                        var d = sc.storage[j];
                        if (d.localLogic == ELogisticStorage.Demand)
                        {
                            if (d.max > d.count && Miner.ContainsKey(d.itemId))
                            {
                                int need = 0;
                                do
                                {
                                    var miner = Miner[d.itemId].Peek();
                                    need = sc.storage[j].max - sc.storage[j].count;
                                    if (need > 0)
                                    {
                                        if (need >= miner.have)
                                        {
                                            TakeItemFromMiner(factory.factorySystem, miner.mid, miner.item, miner.have);
                                            sc.storage[j].count += miner.have;
                                            Miner[d.itemId].Dequeue();
                                        }
                                        else
                                        {
                                            TakeItemFromMiner(factory.factorySystem, miner.mid, miner.item, need);
                                            sc.storage[j].count += need;
                                            miner.have -= need;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                    if (Miner[d.itemId].Count < 1)
                                    {
                                        Miner.Remove(d.itemId);
                                        break;
                                    }
                                } while (true);
                            }
                        }
                    }
                }
            }
        }

        static void TakeItemFromMiner(FactorySystem factory, int mid,int itemid,int count)
        {
            if (mid == 0)
            {
                return;
            }
            var ed = factory.minerPool[mid];
            if (ed.id == mid && ed.productId ==itemid && ed.productCount >=count)
            {
                factory.minerPool[mid].productCount -=count;
            }
        }

        

        class MinerHave
        {
            public int mid;
            public int item;
            public int have;
            public MinerHave(int i,int item,int h)
            {
                mid = i;
                this.item = item;
                have = h;
            }
        }
    }
}
