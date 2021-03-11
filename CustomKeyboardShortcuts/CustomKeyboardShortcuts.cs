using System;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using System.IO;

namespace CustomKeyboardShortcuts
{
	[BepInPlugin("crecheng.CustomKeyboardShortcuts", "CustomKeyboardShortcuts","1.1.1.0")]
    public class CustomKeyboardShortcuts:BaseUnityPlugin
    {
		void Start()
		{
			Harmony.CreateAndPatchAll(typeof(CustomKeyboardShortcuts), null);
			CustomKeyboardShortcuts.style.fontSize = 15;
			CustomKeyboardShortcuts.style1.fontSize = 10;
			CustomKeyboardShortcuts.style.normal.textColor = new Color(255f, 255f, 255f);
			CustomKeyboardShortcuts.style1.normal.textColor = new Color(255f, 255f, 255f);
			path=System.Environment.CurrentDirectory+ "\\BepInEx\\config\\KeyboardShortcuts.json";

			if (File.Exists(path))
			{
				var fs = File.ReadAllLines(path);
                if (fs.Length > 0)
                {
					var s = fs[0].Split(' ');
					for(int i = 0; i < 10; i++)
                    {
						int.TryParse(s[i], out itemID[i]);
                    }
                }
				if (fs.Length > 1)
				{
					var s = fs[1].Split(' ');
					float temp = 0;
					if (float.TryParse(s[0], out temp))
                    {
						rect.x = temp;
                    }

					if (float.TryParse(s[1], out temp))
					{
						rect.y = temp;
					}
				}
            }
            else
            {
				var fs= File.Create(path);
				var sw = new StreamWriter(fs);
				sw.Write("0 0 0 0 0 0 0 0 0 0");
				sw.Close();
				fs.Close();
			}
		}

		void OnGUI()
		{
			
			//GUI.Label(new Rect(200, 200, 50, 50), path, style);
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
				rect = GUI.Window(1935598198, rect, mywindowfunction, "自定义快捷键 ");
			}
			if(isError)
            {
                if (ErrorCount > 0)
                {
					ErrorCount--;
                }
                else
                {
					tryCount++;
					Init();
                }
				if (tryCount > 100)
					isError = false;
			}
		}

		static void Init()
        {
			Debug.Log("crecheng.CustomKeyboardShortcuts.Init()");
			itemGC = new GUIContent[10];
			isError = false;
			for(int i = 0; i < 10; i++)
            {
				itemGC[i] = new GUIContent();
				if (itemID[i] > 0)
                {

					var t = LDB.items.Select(itemID[i]);
					if (t != null)
					{
						ItemName[i] = t.name;
						itemGC[i].tooltip = ItemName[i];
						itemGC[i].image = t.iconSprite.texture;
					}
                    else
                    {
						isError = true;
						ErrorCount = 200*(tryCount+1);
                    }
                }
            }

			canRun = true;

        }

		static void writeFile()
        {
			if (File.Exists(path))
			{
				string s = string.Empty;
				for (int i = 0; i < 10; i++)
				{
					s += itemID[i] + " ";
				}
				s += "\n" + rect.x + " " + rect.y;
				File.WriteAllText(path,s);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameData), "GameTick")]
		public static void takeItem(GameData __instance)
		{
			
			int getkey = Keyboard();

			if (getkey > -1)
            {
				takeItemID = itemID[getkey];
				isMod = true;
            }
			gameData = __instance;
			if (gameData.mainPlayer != null)
			{
				Player player = __instance.mainPlayer;
				if (isMod&&takeItemID>0)
				{
					player.SetHandItems(takeItemID,0);
					isMod = false;
				}
			}

		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameMain), "Begin")]
		public static void begin()
        {
            if (!canRun)
            {
				Init();
            }
        }


		void mywindowfunction(int windowid)
		{
			if (GUI.Button(new Rect(5, 20, 20, 40), info, style))
			{
				if (isWrite)
				{
					writeFile();
				}
				isWrite = !isWrite;
				info = isWrite ? "完\n成\n√" : "编\n辑\nW";

			}
			if (canRun)
			{
				for (int i = 0; i < 10; i++)
				{
					GUI.Label(new Rect(55 + 55 * i, 70, 10, 10), new GUIContent(info,
						"点击编辑，然后手上拿物品\n" +
						"填充按钮，完成后点击完成\n" +
						"之后使用数字或者点击按钮\n" +
						"都能将手上物品成为按钮物品"));
					if (GUI.Button(new Rect(30 + 55 * i, 20, buttonWidth, buttonheight), itemGC[i]))
					{
						if (isWrite)
						{
							int id = getPlayerInHandItem();
							if (id >= 0)
							{
								itemID[i] = id;
							}
							if (id == 0)
							{
								ItemName[i] = "";
							}
							else if (id > 0)
							{
								var d = LDB.items.Select(id);
								ItemName[i] = d.name;
								itemGC[i].tooltip = d.name;

								itemGC[i].image = d.iconSprite.texture;

							}
						}
						else
						{
							isMod = true;
							takeItemID = itemID[i];
						}
					}

				}
			}
			//定义窗体可以活动的范围
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		public int getPlayerInHandItem()
        {
            if (gameData != null)
            {
                if (gameData.mainPlayer != null)
                {
					return gameData.mainPlayer.inhandItemId > 0 ? gameData.mainPlayer.inhandItemId : 0;
				}
            }
			return -1;
        }

		public static int Keyboard()
        {
			if (Input.GetKeyDown(KeyCode.Alpha1))
				return 0;
			if (Input.GetKeyDown(KeyCode.Alpha2))
				return 1;
			if (Input.GetKeyDown(KeyCode.Alpha3))
				return 2;
			if (Input.GetKeyDown(KeyCode.Alpha4))
				return 3;
			if (Input.GetKeyDown(KeyCode.Alpha5))
				return 4;
			if (Input.GetKeyDown(KeyCode.Alpha6))
				return 5;
			if (Input.GetKeyDown(KeyCode.Alpha7))
				return 6;
			if (Input.GetKeyDown(KeyCode.Alpha8))
				return 7;
			if (Input.GetKeyDown(KeyCode.Alpha9))
				return 8;
			if (Input.GetKeyDown(KeyCode.Alpha0))
				return 9;
			return -1;
		}
		private static int tryCount = 0;
		private static int ErrorCount = 0;
		private static bool isError = false;
		private static bool canRun = false;
		private static GUIContent[] itemGC;
		private static string path;
		private static int takeItemID=0;
		private static bool isMod = false;
		private static GameData gameData = null;
		private static bool isWrite = false;
		private static int buttonWidth = 50;
		private static int buttonheight = 50;
		private static int[] itemID = new int[10];
		private static string[] ItemName = new string[10] {"","","","", "", "","", "", "",""};
		private static string info = "编\n辑\nW";
		private static int islock = 0;
		private static bool isShow = true;
		private static Rect rect = new Rect(100, 100, 600, 90);
		// Token: 0x04000003 RID: 3
		private static GUIStyle style = new GUIStyle();
		private static GUIStyle style1 = new GUIStyle();
	}
}
