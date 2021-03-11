using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace InfiniteBox
{
    [BepInPlugin("crecheng.InfiniteBox", "InfiniteBox", "1.0.0.0")]
    public class InfiniteBox:BaseUnityPlugin
    {
        void Start()
        {
            Harmony.CreateAndPatchAll(typeof(InfiniteBox), null);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FactoryStorage), "GameTick")]
        static void InfiniteBoxF(FactoryStorage __instance)
        {
            var _this = __instance;
            if (_this!=null&&_this.storagePool != null)
            {
                foreach (var st in _this.storagePool)
                {
                    if (st!= null&&st.bans>0 && st.size > 20&& (st.size -st.bans)==1&&!GetStorageIsFullHalf(st))
                    {
                        int tempItemId =st.grids[0].itemId;
                        int tempstack = st.grids[0].stackSize;
                        if (tempItemId > 0)
                        {
                            Debug.Log("FullItem" + tempItemId);
                            st.AddItem(tempItemId, st.bans * tempstack/4);
                        }
                    }
                }
            }
        }

        static bool GetStorageIsFullHalf(StorageComponent sc)
        {
            for (int i = 0; i < sc.size/2; i++)
            {
                if (sc.grids[i].itemId == 0 || sc.grids[i].count < sc.grids[i].stackSize)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
