using System;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;

[BepInPlugin("crecheng.MultipMachineSpeed", "MultipMachineSpeed", "1.3.4.0")]
public class MultipMachineSpeed : BaseUnityPlugin
{
	void Start()
	{
		Harmony.CreateAndPatchAll(typeof(MultipMachineSpeed), null);
		MultipMachineSpeed.style.fontSize = 15;
		MultipMachineSpeed.style.normal.textColor = new Color(255f, 255f, 255f);
	}

	private void OnGUI()
	{
		
        if (Input.GetKeyDown(KeyCode.I))
		{
			islock++;
            if (islock > 5)
            {
				isShow = !isShow;
				islock = 0;
            }


		}
		if (isShow)
		{
			rect = GUI.Window(1935598195, rect, mywindowfunction, "机器超频");
			
		}
	}
	void mywindowfunction(int windowid)
	{
		GUI.Label(new Rect(10,20,90,180), "当前机器速度\n" + p + "\n电力消耗倍率\n" + (float)pp, MultipMachineSpeed.style);
		GUI.Label(new Rect(10, 110, 90, 20), "按i开关界面", MultipMachineSpeed.style);
		inputString = GUI.TextField(new Rect(10, 90, 90, 20), inputString, 8);
		if (inputString.Length > 0)
		{
			if (float.TryParse(inputString, out p))
			{
				if (p <= 0)
				{
					p = 1;
				}
				pp = Math.Pow(p, 1.5f);
			}

		}
		//定义窗体可以活动的范围
		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}


	// Token: 0x06000003 RID: 3 RVA: 0x000020D4 File Offset: 0x000002D4
	[HarmonyPrefix]
	[HarmonyPatch(typeof(PlanetFactory), "CreateEntityLogicComponents")]
	public static void addData(PlanetFactory __instance, int entityId, PrefabDesc desc, int prebuildId)
	{
		MultipMachineSpeed.info = new StringBuilder();
		var s = MultipMachineSpeed.info;
		if (desc.isAssembler)
		{
			speed1 = desc.assemblerSpeed;
			power1 = desc.workEnergyPerTick;
			pp = Math.Pow(p, 1.5f);
			desc.assemblerSpeed = (int)(desc.assemblerSpeed* p);
			desc.workEnergyPerTick = (long)(desc.workEnergyPerTick * pp);
			isModAssembler = true;
		}
        if (desc.isSilo)
        {
			speed1 = desc.siloChargeFrame;
			speed2 = desc.siloColdFrame;
			power1 = desc.workEnergyPerTick;
			pp = Math.Pow(p, 1.5f);

			desc.siloChargeFrame= (int)(desc.siloChargeFrame/p);
			desc.siloColdFrame = (int)(desc.siloColdFrame / p);
			desc.workEnergyPerTick = (long)(desc.workEnergyPerTick * pp);
			isModSilo = true;
		}
        if (desc.isEjector)
        {
			speed1 = desc.ejectorChargeFrame;
			speed2 = desc.ejectorColdFrame;
			power1 = desc.workEnergyPerTick;
			pp = Math.Pow(p, 1.5f);

			desc.ejectorChargeFrame = (int)(desc.ejectorChargeFrame / p);
			desc.ejectorColdFrame = (int)(desc.ejectorColdFrame / p);
			desc.workEnergyPerTick = (long)(desc.workEnergyPerTick * pp);
			isModEjector = true;
		}
        if (desc.isPowerGen&&desc.useFuelPerTick>0)
        {
			speedl1= desc.genEnergyPerTick;
			power1= desc.useFuelPerTick;
			pp = Math.Pow(p, 1.5f);
			desc.genEnergyPerTick = (int)(desc.genEnergyPerTick * p);
			desc.useFuelPerTick = (long)(desc.useFuelPerTick * pp);
			isModPowerGen = true;
		}
		if(desc.minerType != EMinerType.None && desc.minerPeriod > 0)
        {
			power1 = desc.workEnergyPerTick;
			pp = Math.Pow(p, 1.5f);
			desc.workEnergyPerTick = (long)(desc.workEnergyPerTick * pp);

			isModMiner = true;
		}
        if (desc.isInserter)
        {
			power1 = desc.workEnergyPerTick;
			pp = Math.Pow(p, 1.5f);
			desc.workEnergyPerTick = (long)(desc.workEnergyPerTick * pp);

			isModInserter = true;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(PlanetFactory), "CreateEntityDisplayComponents")]
	public static void reData(PlanetFactory __instance, int entityId, PrefabDesc desc, short modelIndex)
	{

		MultipMachineSpeed.info = new StringBuilder();
		var s = MultipMachineSpeed.info;
		if (isModAssembler)
		{
			desc.assemblerSpeed = speed1;
			desc.workEnergyPerTick = power1;
			isModAssembler = false;
		}
        if (isModSilo)
        {
			desc.siloChargeFrame = speed1;
			desc.siloColdFrame = speed2;
			desc.workEnergyPerTick = power1;
			isModSilo = false;
		}
        if (isModEjector)
        {
			desc.ejectorChargeFrame=speed1 ;
			desc.ejectorColdFrame=speed2;
			desc.workEnergyPerTick=power1;
			isModEjector = false;
		}
		if (isModPowerGen)
		{
			desc.genEnergyPerTick = speedl1;
			desc.useFuelPerTick = power1;
			isModPowerGen = false;
		}
        if (isModMiner)
        {
			desc.workEnergyPerTick = power1;
			isModMiner = false;
        }
        if (isModInserter)
        {
			desc.workEnergyPerTick = power1;
			isModInserter = false;
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FactorySystem), "NewMinerComponent")]
	public static void modMiner(FactorySystem __instance,ref int __result)
    {
		if (isModMiner)
		{
			__instance.minerPool[__result].speed = (int)(10000 * p);
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FactorySystem), "NewInserterComponent")]
	public static void modInserter(FactorySystem __instance, ref int __result)
	{
		if (isModInserter)
		{
			__instance.inserterPool[__result].speed = (int)(10000 * p);
		}
	}




	private static int islock =0;
	private static int speed2;
	private static long speedl1;
	private static bool isShow = true;
	private static float p=1;
	private static double pp=1;
	private static int speed1;
	private static long power1;
	private static bool isModSilo = false;
	private static bool isModAssembler = false;
	private static bool isModEjector = false;
	private static bool isModPowerGen = false;
	private static bool isModMiner = false;
	private static bool isModInserter = false;

	private static string inputString = string.Empty;

	// Token: 0x04000001 RID: 1
	private static StringBuilder info = null;

	// Token: 0x04000002 RID: 2
	private static Rect rect = new Rect(100, 100, 110, 140);

	// Token: 0x04000003 RID: 3
	private static GUIStyle style = new GUIStyle();
}

