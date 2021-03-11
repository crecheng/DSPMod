using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace closeError
{
    [BepInPlugin("crecheng.CloseError", "CloseError", "1.0.0")]
    public class CloseError : BaseUnityPlugin
    {
        public void Start()
        {
            Harmony.CreateAndPatchAll(typeof(CloseError), null);
            CloseError.style.fontSize = 15;
            CloseError.style.normal.textColor = new Color(255f, 255f, 255f);
            rect.x = UnityEngine.Screen.width - 35;
            rect.y = 10;
        }

        public void OnGUI()
        {
            if (isError)
            {
                
                if (GUI.Button(rect, "X"))
                {
                    ManualBehaviour m = UIFatalErrorTip.instance;
                    if (m != null)
                    {
                        m._Close();
                    }
                    isError = false;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UIFatalErrorTip), "_OnOpen")]
        static void errorFalse(UIFatalErrorTip __instance)
        {
            isError = true;
        }
        private void Update()
        {

        }

        private static Rect rect = new Rect(0,0,20,20);
        private static bool isError = false;
        private static GUIStyle style = new GUIStyle();
    }
}
