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

namespace TidalLocked
{
    [BepInDependency("crecheng.DSPModSave", "1.0.2")]
    [BepInPlugin("crecheng.TidalLocked", "TidalLocked", Version)]
    public class TidalLocked : BaseUnityPlugin, IModCanSave
    {
        const string Version = "1.0.0";
        static bool isRun = false;
        //已经潮汐锁定的数据
        static Dictionary<long, PData> data = new Dictionary<long, PData>();
        static bool isLock = false;
        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(TidalLocked), null);
        }

        private void Update()
        {
            
            if (Input.GetKey(KeyCode.LeftControl)&&Input.GetKeyDown(KeyCode.L))
            {
                if (!isLock)
                {
                    if (GameMain.localPlanet != null)
                    {
                        var p = GameMain.localPlanet;
                        long id = p.star.index;
                        id<<= 32;
                        id += p.index;
                        Debug.Log(p.star.index + "|" + p.index);
                        if (!data.ContainsKey(id))
                        {
                            UIRealtimeTip.Popup("Lock Planet", true);
                            var old = GameMain.localPlanet.rotationPeriod;
                            //设置潮汐锁定
                            GameMain.localPlanet.rotationPeriod = GameMain.localPlanet.orbitalPeriod;
                            data.Add(id,new PData(old));
                        }
                        else
                        {
                            UIRealtimeTip.Popup("UnLock Planet", true);
                            GameMain.localPlanet.rotationPeriod = data[id].oldR;
                            data.Remove(id);
                        }
                    }
                }
            }
            else
            {
                isLock = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData), "GameTick")]
        static void GameTick(GameData __instance)
        {
            if (!isRun)
            {
                var galaxy = __instance.galaxy;
                if (data.Count > 0)
                {
                    foreach (var d in data)
                    {
                        int sIndex = (int)(d.Key >> 32);
                        int pIndex = (int)((d.Key << 32) >> 32);
                        Debug.Log(sIndex + "|" + pIndex);
                        d.Value.Swap(galaxy.stars[sIndex].planets[pIndex]);
                    }
                }
                GameMain.data.galaxy = galaxy;
                isRun = true;
            }
        }

        public void Export(BinaryWriter w)
        {
            w.Write(data.Count);
            foreach (var d in data)
            {
                w.Write(d.Key);
                d.Value.Export(w);
            }
        }

        public void Import(BinaryReader r)
        {
            isRun = false;
            data.Clear();
            int num = r.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                long key = r.ReadInt64();
                PData d = new PData();
                d.Import(r);
                data.Add(key, d);
            }
        }

        public void IntoOtherSave()
        {
            data.Clear();
            isRun = false;
        }

        static void WriteVL3(BinaryWriter w,VectorLF3 v)
        {
            w.Write(v.x);
            w.Write(v.y);
            w.Write(v.z);
        }

        static VectorLF3 ReadVL3(BinaryReader r)
        {
            return new VectorLF3(r.ReadDouble(), r.ReadDouble(), r.ReadDouble());
        }

        static void WriteV3(BinaryWriter w, Vector3 v)
        {
            w.Write(v.x);
            w.Write(v.y);
            w.Write(v.z);
        }

        static Vector3 ReadV3(BinaryReader r)
        {
            return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        }

        static void WriteQ(BinaryWriter w, Quaternion q)
        {
            w.Write(q.x);
            w.Write(q.y);
            w.Write(q.z);
            w.Write(q.w);
        }

        static Quaternion ReadQ(BinaryReader r)
        {
            return new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        }

        class PData
        {
            public double oldR;
            public PData()
            {

            }

            public PData(double old)
            {
                oldR = old;
            }

            public void Export(BinaryWriter w)
            {
                w.Write(oldR);
            }

            public void Swap(PlanetData planet)
            {
                planet.rotationPeriod = planet.orbitalPeriod;
            }

            public void Import(BinaryReader r)
            {
                oldR = r.ReadDouble();
            }
        }
    }
}
