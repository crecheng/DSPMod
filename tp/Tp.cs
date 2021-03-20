using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


[BepInPlugin("crecheng.Tp", "Tp", "2.0.0.0")]
public class Tp:BaseUnityPlugin
{
	void Start()
	{
		Harmony.CreateAndPatchAll(typeof(Tp), null);
		Tp.style.fontSize = 15;
		Tp.style.normal.textColor = new Color(255f, 255f, 255f);
		tempRect = new Rect(Screen.width / 2, 0, 0, 0);
	}

	private void OnGUI()
	{



		if (isShow)
		{
			rect = GUI.Window(1935598196, rect, mywindowfunction, "星际传送");
		}
		else
		{
			if (GUI.Button(new Rect(rect.x, rect.y,  40, 30), "▼T▼"))
			{
				isShow = !isShow;
				var temp = rect;
				rect = tempRect;
				tempRect = temp;
			}
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
		resPlanet.Clear();
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

	static Rect tempRect;

	void mywindowfunction(int windowid)
	{

		if (GUI.Button(new Rect(0, 0, 20, 20), "X"))
		{
			isShow = !isShow;
			var temp = rect;
			rect = tempRect;
			tempRect = temp;
		}

		GUI.Label(new Rect(10, 20, 300, 300), Tp.tipInfo + "\n" + info, Tp.style);
		fingStarNameAll = GUI.TextField(new Rect(10, 40, 100, 20), Tp.fingStarNameAll, 20);
		if (GUI.Button(new Rect(110, 40, 40, 20), "查找"))
		{
			var galaxyData = GameMain.data.galaxy;
			if (galaxyData != null)
			{
				findStar(galaxyData, fingStarNameAll);
			}
		}
		int i = 0;
		foreach (var d in resPlanet)
		{
			if (GUI.Button(new Rect(10 + i % 2 * 122, 100 + 20 * (i / 2), 120, 20), d.displayName))
			{
				isEnter = true;
				target = d;
			}
			i++;
		}
		i /= 2;
		foreach (var d in resDate)
		{
			GUI.Label(new Rect(10, 80 + 40 * i, 100, 20), d.Key);
			int j = 0;
			foreach (var p in d.Value.planets)
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
		resPlanet.Clear();
		foreach(var d in g.stars)
        {
            if (d.name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) > -1||d.displayName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase)>-1)
            {
				resDate.Add(d.displayName, d);
            }
            else
            {
                for (int i = 0; i < d.planetCount; i++)
                {
					if(d.planets[i].displayName.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) > -1)
                    {
						resPlanet.Add(d.planets[i]);
                    }
                }
            }
        }
		rect.height = 120 + resDate.Count * 40+resPlanet.Count/2*20;
    }

	private static Dictionary<String, StarData> resDate = new Dictionary<string, StarData>();
	private static List<PlanetData> resPlanet = new List<PlanetData>();

	private static PlanetData target = null;
	private static bool isEnter=false;
	private static string fingStarNameAll = string.Empty;

	private static string info = "";

	private static string tipInfo = "输入要传送的星球的名字(可以部分)\n";


	private static bool isShow = false;


	private static string findStarName = string.Empty;

	private static Rect rect = new Rect(330f, 30f, 250f, 200f);

	private static GUIStyle style = new GUIStyle();
}

