using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


[BepInPlugin("crecheng.Tp", "Tp", "1.3.0.0")]
public class Tp:BaseUnityPlugin
{
	void Start()
	{
		Harmony.CreateAndPatchAll(typeof(Tp), null);
		Tp.style.fontSize = 15;
		Tp.style.normal.textColor = new Color(255f, 255f, 255f);
	}

	private void OnGUI()
	{

		ShowFun();
		if (Input.GetKeyDown(KeyCode.O)&&isShow)
        {
			isEnterCount++;
            if (isEnterCount > 5)
            {
				isEnter = true;
				if (fingStarNameAll.Length > 0)
				{
					var s = fingStarNameAll.Split('-');
					findStarName = s[0];
					if (s.Length > 1)
					{
						int.TryParse(s[1], out planetIndex);
						planetIndex -= 1;
					}
				}
			}
        }
		if (isShow)
		{
			rect = GUI.Window(1935598196, rect, mywindowfunction, "星际传送");
		}
	}

	private void Update()
    {
		if (isEnter)
		{
			if (target != null)
			{

				//__instance.ArrivePlanet(target);
				//if (target.type == EPlanetType.Gas)
				//	__instance.mainPlayer.position = new Vector3(0, 850, 0);
				//else
				//	__instance.mainPlayer.position = new Vector3(0, 210, 0);
				//isEnter = false;
				//player.transform.localScale = Vector3.one;
				//errorCount = 900;
				GameMain.data.ArriveStar(target.star);
				StartCoroutine(SendPlayer(target));
				GameMain.data.mainPlayer.movementState = EMovementState.Sail;
				isEnter = false;
			}
		}
	}

    [HarmonyPostfix]
    [HarmonyPatch(typeof(UIFatalErrorTip), "_OnOpen")]

    static void errorFalse2(UIFatalErrorTip __instance)
    {
		//if (errorCount > 0)
		//{
		//	errorCount--;
		//	ManualBehaviour m = UIFatalErrorTip.instance;
		//	if (m != null)
		//	{
		//		m._Close();
		//	}
		//}
	}

    [HarmonyPostfix]
	[HarmonyPatch(typeof(GameMain), "End")]
	static void GameEnd()
    {
		resDate.Clear();
    }

	IEnumerator SendPlayer(object target)
    {
		yield return new WaitForEndOfFrame();//等待帧结束
											 
		if (target is PlanetData)
		{
			Debug.Log("SendPlanet");
			GameMain.mainPlayer.uPosition = ((PlanetData)target).uPosition + VectorLF3.unit_z * (((PlanetData)target).realRadius);
		}
		else if (target is VectorLF3)
		{
			Debug.Log("SendVectorLF3");
			GameMain.mainPlayer.uPosition = (VectorLF3)target;
		}
		else if (target is string && (string)target == "resize")
		{
			Debug.Log("SendSV");
			GameMain.mainPlayer.transform.localScale = Vector3.one;
		}
		if (!(target is string) || (string)target != "resize")
		{
			Debug.Log("SendResize");
			StartCoroutine(SendPlayer("resize"));
		}
	}

	void mywindowfunction(int windowid)
	{
		GUI.Label(new Rect(10,20,300,300), Tp.tipInfo+"\n"+info, Tp.style);
		fingStarNameAll = GUI.TextField(new Rect(10, 40, 100, 20), Tp.fingStarNameAll, 20);
		if(GUI.Button(new Rect(110, 40, 40, 20), "查找"))
        {
			var galaxyData = GameMain.data.galaxy;
			if (galaxyData != null)
            {
				findStar(galaxyData, fingStarNameAll);
            }
        }
		int i = 0;
		foreach(var d in resDate)
        {
			GUI.Label(new Rect(10, 80 + 40 * i, 100, 20), d.Key);
			int j = 0;
			foreach(var p in d.Value.planets)
            {
				if (GUI.Button(new Rect(10 + j * 22, 100 + 40 * i, 20, 20), "" + (j + 1)))
				{
					isEnter = true;
					target = p;
				}
				j++;
            }
			i++;
		}
		GUI.DragWindow(new Rect(0, 0, 10000, 10000));
	}

	private static void findStar(GalaxyData g, string s)
    {
		Debug.Log("findStar");
		resDate.Clear();
		foreach(var d in g.stars)
        {
            if (d.name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) > -1)
            {
				resDate.Add(d.name, d);
            }
        }
		rect.height = 120 + resDate.Count * 40;
    }

	private static void ShowFun()
    {
		if (Input.GetKeyDown(KeyCode.I))
		{
			isShowCount++;
			if (isShowCount > 5)
			{
				isShow = !isShow;
				isShowCount = 0;
			}
		}
	}

	private static Dictionary<String, StarData> resDate = new Dictionary<string, StarData>();

	private static PlanetData target = null;
	private static bool isEnter=false;
	private static int isEnterCount = 0;
	private static int planetIndex=0;
	private static string fingStarNameAll = string.Empty;

	private static string info = "";

	private static string tipInfo = "输入要传送的星系名字(可以部分)\n";


	//private static StringBuilder debugInfo = new StringBuilder();

	private static int errorCount = 0;

	private static bool isShow = true;

	private static int isShowCount = 0;

	private static string findStarName = string.Empty;

	private static Rect rect = new Rect(330f, 30f, 250f, 200f);

	private static GUIStyle style = new GUIStyle();
}

