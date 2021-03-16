using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using crecheng.DSPModSave;

namespace AddFuelStar
{
    [BepInDependency("crecheng.DSPModSave", "1.0.2")]
    [BepInPlugin("crecheng.AddFuelStar", "AddFuelStar", Version)]
    public class AddFuelStar : BaseUnityPlugin, IModCanSave
    {
        const string Version = "1.0.0";
        static StarFuel[] star =null;
        static bool isRun=false;
        const double showDis = 1500;
        static bool isShow = false;
        const int Itemid = 1801;
         void Start()
        {
            Harmony.CreateAndPatchAll(typeof(AddFuelStar), null);
        }

        void OnGUI()
        {
            if (isShow)
            {
                if(GUI.Button(new Rect(300, 300, 40, 40), "Add"))
                {
                    if (GameMain.mainPlayer.inhandItemId == Itemid)
                    {
                        var index = GameMain.localStar.index;
                        star[index].addFuel += GameMain.mainPlayer.inhandItemCount;
                        GameMain.mainPlayer.SetHandItemCount_Unsafe(0);
                        GameMain.mainPlayer.SetHandItemId_Unsafe(0);
                        GameMain.localStar.luminosity = star[index].GetNewL();
                    }
                }
            }
        }
        public void Export(BinaryWriter w)
        {
            if (star != null)
            {
                w.Write(star.Length);
                for (int i = 0; i < star.Length; i++)
                {
                    star[i].Export(w);
                }
            }
        }

        public void Import(BinaryReader r)
        {
            star = null;
            isRun = false;
            int num = r.ReadInt32();
            star = new StarFuel[num];
            for (int i = 0; i < num; i++)
            {

                star[i] = new StarFuel();
                star[i].Import(r);
            }


        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData), "GameTick")]
        static void GameTick(GameData __instance)
        {
            if (!isRun)
            {
                var galaxy = __instance.galaxy;
                if (star == null) 
                {
                    star = new StarFuel[galaxy.starCount];

                    for (int i = 0; i < galaxy.starCount; i++)
                    {
                        star[i] = new StarFuel(i, galaxy.stars[i].luminosity);
                    }
                }
                else
                {
                    int count = Math.Min(galaxy.starCount, star.Length);
                    for (int i = 0; i < count; i++)
                    {
                        galaxy.stars[i].luminosity = star[i].GetNewL();
                      
                    }
                }
                isRun = true;
            }
            if (isRun)
            {
                if(GameMain.localStar != null)
                {
                    var dis = GameMain.localStar.uPosition.Distance(GameMain.mainPlayer.uPosition)/ GameMain.localStar.radius;
                    if (dis<showDis)
                    {
                        isShow = true;
                    }
                    else
                    {

                        isShow = false;
                    }
                }
            }
        }

        //[HarmonyPrefix]
        //[HarmonyPatch(typeof(EjectorComponent), "InternalUpdate")]
        //public void ModEjecter(EjectorComponent __instance, float power, DysonSwarm swarm, AstroPose[] astroPoses, AnimData[] animPool, int[] consumeRegister)
        //{
        //    Debug.Log(__instance.id+","+__instance.planetId);
        //}
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DysonSphere), "GameTick")]
        static void DspInit(DysonSphere __instance)
        {
            if (__instance != null )
            {
                var _this = __instance;
                int index = __instance.starData.index;
                double num5 = (double)_this.starData.dysonLumino;
                _this.energyGenPerSail = Configs.freeMode.solarSailEnergyPerTick;
                _this.energyGenPerNode = Configs.freeMode.dysonNodeEnergyPerTick;
                _this.energyGenPerFrame = Configs.freeMode.dysonFrameEnergyPerTick;
                _this.energyGenPerShell = Configs.freeMode.dysonShellEnergyPerTick;
                _this.energyGenPerSail = (long)((double)_this.energyGenPerSail * num5);
                _this.energyGenPerNode = (long)((double)_this.energyGenPerNode * num5);
                _this.energyGenPerFrame = (long)((double)_this.energyGenPerFrame * num5);
                _this.energyGenPerShell = (long)((double)_this.energyGenPerShell * num5);
            }
        }

        public void IntoOtherSave()
        {
            star = null;
            isRun = false;
        }

        class StarFuel
        {
            public int index;
            public float oldL;
            public long addFuel2;
            public long addFuel;
            public float newL;

            public StarFuel()
            {

            }
            public StarFuel(int index,float old)
            {
                this.index = index;
                this.oldL = old;
            }

            public float GetNewL()
            {
                if (addFuel == 0)
                    return oldL;
                if (oldL < 1f && addFuel < 1000)
                {
                    float num = 1f - oldL;
                    newL = oldL + num / 1000f;
                    return newL;
                }
                else
                {
                    long num = addFuel>>10;
                    var num1 = Math.Pow(num, 0.6);
                    float num2 = (float)num1;
                    newL = Math.Max(num2, oldL);
                    return newL;
                }
            }

            public void Export(BinaryWriter w)
            {
                w.Write(index);
                w.Write(oldL);
                w.Write(addFuel);
                w.Write(addFuel2);
                w.Write(newL);
            }

            public void Import(BinaryReader r)
            {
                index = r.ReadInt32();
                oldL = r.ReadSingle();
                addFuel = r.ReadInt64();
                addFuel2 = r.ReadInt64();
                newL = r.ReadSingle();
            }
        }
    }
}
