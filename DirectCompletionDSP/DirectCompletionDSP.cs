using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace DirectCompletionDSP
{
    [BepInPlugin("crecheng.DirectCompletionDSP", "DirectCompletionDSP", DirectCompletionDSP.Version)]
    public class DirectCompletionDSP:BaseUnityPlugin
    {
        public const string Version = "1.1.0";
        static bool isShow = false;
        static bool addr = false;
        static bool adds = false;
        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(DirectCompletionDSP), null);
        }

        private void OnGUI()
        {
            if (isShow)
            {
                rect = GUI.Window(1935598225, rect, mywindowfunction, "DC-DSP");
            }
        }
        static Rect rect = new Rect(300, 300, 100, 100);


        void mywindowfunction(int windowid)
        {
            if(GUI.Button(new Rect(10, 20, 80, 20), "添加火箭"+(addr?">":"||")) || Input.GetKey(KeyCode.Minus))
            {
                addr = !addr;
            }
            if (GUI.Button(new Rect(10, 45, 80, 20), "添加细胞" + (adds ? ">" : "||")) ||Input.GetKey(KeyCode.Equals))
            {
                adds = !adds;
            }
            if (GUI.Button(new Rect(10, 70, 80, 20), "移除太阳帆"))
            {
                if (GameMain.localPlanet != null)
                {
                    var dsp = GameMain.localPlanet.factory.dysonSphere;

                    if (dsp != null)
                    {
                        dsp.swarm.Clear();
                    }
                }
            }
            if (addr || adds)
            {
                if (GameMain.localPlanet != null)
                {
                    var dsp = GameMain.localPlanet.factory.dysonSphere;

                    if (dsp != null)
                    {
                        if (addr)
                            StartCoroutine(AddSp(dsp));
                    }
                    if (dsp != null)
                    {
                        if (adds)
                            StartCoroutine(AddCp(dsp));
                    }
                }
            }


            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        IEnumerator AddSp(DysonSphere dsp)
        {
            yield return new WaitForEndOfFrame();
            long count = 0;
            for (int i = 0; i < dsp.layersIdBased.Length; i++)
            {
                var shell = dsp.layersIdBased[i];
                if (shell != null)
                {
                    for (int j = 0; j < shell.nodeCursor; j++)
                    {
                        var node = shell.nodePool[j];
                        if (node != null)
                        {
                            count++;
                            node.ConstructSp();
                            //node.ConstructCp();
                        }
                    }
                }
            }
        }

        IEnumerator RemoveSail(DysonSphere dsp)
        {
            yield return new WaitForEndOfFrame();
            for (int i = 1; i < dsp.swarm.sailCapacity; i++)
            {
                dsp.swarm.RemoveSolarSail(i);
            }

        }

        IEnumerator AddCp(DysonSphere dsp)
        {
            yield return new WaitForEndOfFrame();
            long count = 0;
            for (int i = 0; i < dsp.layersIdBased.Length; i++)
            {
                var shell = dsp.layersIdBased[i];
                if (shell != null)
                {
                    for (int j = 0; j < shell.nodeCursor; j++)
                    {
                        var node = shell.nodePool[j];
                        if (node != null)
                        {
                            count++;
                            //node.ConstructSp();
                            node.ConstructCp();
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIDysonPanel), "_OnOpen")]
        static void DspShow(UIDysonPanel __instance)
        {
            isShow = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIDysonPanel), "_OnClose")]
        static void DspClose()
        {
            addr = false;
            adds = false;
            isShow = false;
        }

    }
}
