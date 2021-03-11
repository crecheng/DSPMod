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

namespace DSPHierarchicalDisplay
{
    [BepInPlugin("crecheng.DSPHierarchicalDisplay", "DSPHierarchicalDisplay", DSPHierarchicalDisplay.Version)]
    public class DSPHierarchicalDisplay : BaseUnityPlugin
    {
        const string Version = "1.0.2";
        public static bool isRun = false;
        static DysonSphereLayer[] tempDsp;
        static DysonSphere viewDsp;
        static bool isShow = false;
        static bool isStop = false;
        static bool[] shell = new bool[11];
        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(DSPHierarchicalDisplay), null);
            tempDsp = new DysonSphereLayer[11];
            AllTrue();
        }

        private void OnGUI()
        {
            if (isShow)
            {
                rect = GUI.Window(1935598215, rect, mywindowfunction,"DSP");
            }
        }
        static Rect rect = new Rect(300, 300, 100, 150);

        void mywindowfunction(int windowid)
        {
            for (int i = 1; i < 11; i++)
            {
                if (GUI.Button(new Rect(10+(i-1)/5*45, 20 + (i-1)%5 * 22, 40, 20), i.ToString()+(shell[i]?"": "X")))
                {
                    StartCoroutine(SwapDspDataTemp(i));
                    shell[i]=!shell[i];
                    GameMain.isFullscreenPaused = true;
                }
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        static void AllTrue()
        {
            for (int i = 0; i < shell.Length; i++)
            {
                shell[i] = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIDysonPanel), "_OnOpen")]
        static void DspShow(UIDysonPanel __instance)
        {
            viewDsp = __instance.viewDysonSphere;
            if (viewDsp != null)
            {
                for (int i = 0; i < viewDsp.layersIdBased.Length; i++)
                {
                    tempDsp[i] = null;
                }
                isShow = true;
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIDysonPanel), "_OnClose")]
        static void DspClose()
        {
            AddDspData();
            isShow = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIDysonPanel), "OnAddOkClick")]
        static bool CanAdd()
        {
            for (int i = 0; i < shell.Length; i++)
            {
                if (!shell[i])
                    return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIDysonPanel), "OnShellLayerAddClick")]
        static bool CanAdd1()
        {
            for (int i = 0; i < shell.Length; i++)
            {
                if (!shell[i])
                    return false;
            }
            return true;
        }


        IEnumerator SwapDspDataTemp(int index)
        {
            yield return new WaitForFixedUpdate();

            var temp = viewDsp.layersIdBased[index];
            viewDsp.layersIdBased[index] = tempDsp[index];
            tempDsp[index] = temp;
            viewDsp.modelRenderer.RebuildModels();
        }

        static void AddDspData()
        {
            //yield return new WaitForFixedUpdate();

            for (int i = 1; i < viewDsp.layersIdBased.Length; i++)
            {
                if (!shell[i])
                {
                    viewDsp.layersIdBased[i] = tempDsp[i];
                }
            }
            viewDsp.modelRenderer.RebuildModels();
            viewDsp = null;
            
            AllTrue();
        }


    }
}
