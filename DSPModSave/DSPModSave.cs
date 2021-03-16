using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BepInEx;
using HarmonyLib;
using UnityEngine;


namespace crecheng.DSPModSave
{
	[BepInPlugin("crecheng.DSPModSave", "DSPModSave", Version)]
	public class DSPModSave : BaseUnityPlugin
	{
		const string Version = "1.0.2";
		const int version = 1;
		static Dictionary<string, IModCanSave> AllModData;

		public static readonly string saveExt = ".moddsv";
		public static readonly string AutoSaveTmp = "_autosave_tmp";
		public static readonly string AutoSave0 = "_autosave_0";
		private static readonly string AutoSave1 = "_autosave_1";
		private static readonly string AutoSave2 = "_autosave_2";
		private static readonly string AutoSave3 = "_autosave_3";
		public static readonly string LastExit = "_lastexit_";

		private void Start()
		{
			Harmony.CreateAndPatchAll(typeof(DSPModSave), null);
			AllModData = new Dictionary<string, IModCanSave>();
			Init();
		}

		void Init()
		{
			foreach (var d in BepInEx.Bootstrap.Chainloader.PluginInfos)
			{
				if (d.Value.Instance is IModCanSave)
				{
					AllModData.Add(d.Value.Metadata.GUID, (IModCanSave)d.Value.Instance);
				}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameSave), "SaveCurrentGame")]
		static void SaveCurrentGame(bool __result, string saveName)
		{
			if (__result)
			{
				if (DSPGame.Game == null)
				{
					Debug.LogError("No game to save");
					return;
				}
				if (AllModData.Count == 0)
					return;
				saveName = saveName.ValidFileName();
				string path = GameConfig.gameSaveFolder + saveName + saveExt;
				try
				{
					using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
					{
						SaveData(fileStream);
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameSave), "AutoSave")]
		static void AutoSave(bool __result)
        {
            if (__result)
            {
				string text = GameConfig.gameSaveFolder + GameSave.AutoSaveTmp + saveExt;
				string text2 = GameConfig.gameSaveFolder + AutoSave0 + saveExt;
				string text3 = GameConfig.gameSaveFolder + AutoSave1 + saveExt;
				string text4 = GameConfig.gameSaveFolder + AutoSave2 + saveExt;
				string text5 = GameConfig.gameSaveFolder + AutoSave3 + saveExt;
				if (File.Exists(text))
				{
					if (File.Exists(text5))
					{
						File.Delete(text5);
					}
					if (File.Exists(text4))
					{
						File.Move(text4, text5);
					}
					if (File.Exists(text3))
					{
						File.Move(text3, text4);
					}
					if (File.Exists(text2))
					{
						File.Move(text2, text3);
					}
					File.Move(text, text2);
				}
			}
        }

		[HarmonyPostfix]
		[HarmonyPatch(typeof(UIGalaxySelect), "EnterGame")]
		static void EnterGame()
        {
			Debug.Log("Enter New Game");
			foreach (var d in AllModData)
			{
				d.Value.IntoOtherSave();
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(GameSave), "LoadCurrentGame")]
		static void LoadCurrentGame(bool __result, string saveName)
        {
            if (__result)
            {
				if (DSPGame.Game == null)
				{
					Debug.LogError("No game to load");
					return ;
				}
				string path = GameConfig.gameSaveFolder + saveName + saveExt;
				if (!File.Exists(path))
				{
					Debug.Log(saveName+ ": Game mod save not exist");
					foreach(var d in AllModData)
                    {
						d.Value.IntoOtherSave();
                    }
					return ;
				}
				try
				{
					using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read))
					{
						LoadData(fileStream);
					}
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
				}
			}
        }

		static void SaveData(FileStream fileStream)
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(fileStream))
			{
				Dictionary<string, long> pos = new Dictionary<string, long>();
				binaryWriter.Write('M');
				binaryWriter.Write('O');
				binaryWriter.Write('D');
				binaryWriter.Write(version);
				binaryWriter.Write(0L);
				binaryWriter.Write(0L);
				binaryWriter.Write(0L);
				binaryWriter.Write(0L);
				binaryWriter.Write(0L);
				binaryWriter.Write(AllModData.Count);

				foreach (var name in AllModData)
				{
					binaryWriter.Write(name.Key);
					pos.Add(name.Key, fileStream.Position);
					binaryWriter.Write(0L);
					binaryWriter.Write(0L);
				}
				foreach (var data in AllModData)
				{
					var name = data.Key;
					if (pos.ContainsKey(name))
					{
						long begin = fileStream.Position;
                        try 
						{ 
							using(MemoryStream ms=new MemoryStream())
                            {
								using(BinaryWriter binary =new BinaryWriter(ms))
                                {
									data.Value.Export(binary);
									var dataByte = ms.ToArray();
									fileStream.Write(dataByte,0,dataByte.Length);
								}
                            }
						}
						catch (Exception ex)
						{
							Debug.LogError(data.Key + " :mod data export error!");
							Debug.LogError(ex.Message + "\n" + ex.StackTrace);

						}
						long end = fileStream.Position;
						fileStream.Seek(pos[name], SeekOrigin.Begin);
						binaryWriter.Write(begin);
						binaryWriter.Write(end);
						//Debug.Log($"{name},{begin},{end}");
						fileStream.Seek(0, SeekOrigin.End);
					}
				}
			}
		}

		static void LoadData(FileStream fileStream)
		{
			using (BinaryReader binaryReader = new BinaryReader(fileStream))
			{
				Dictionary<string, MySaveData> data = new Dictionary<string, MySaveData>();
				bool flag = true;
				flag = (flag && binaryReader.ReadChar() == 'M');
				flag = (flag && binaryReader.ReadChar() == 'O');
				flag = (flag && binaryReader.ReadChar() == 'D');
				int dataVersion = binaryReader.ReadInt32();
				binaryReader.ReadInt64();
				binaryReader.ReadInt64();
				binaryReader.ReadInt64();
				binaryReader.ReadInt64();
				binaryReader.ReadInt64();
				int count = binaryReader.ReadInt32();
				
				for (int i = 0; i < count; i++)
				{
					string name = binaryReader.ReadString();
					long begin = binaryReader.ReadInt64();
					long end = binaryReader.ReadInt64();
					data.Add(name, new MySaveData(name, begin, end));
				}

				foreach(var d in AllModData)
                {
                    if (data.ContainsKey(d.Key))
                    {
						var e = data[d.Key];
						fileStream.Seek(e.begin, SeekOrigin.Begin);
						//Debug.Log($"{AllModData.Count},{e.name},{e.begin},{e.end}");
						byte[] b = new byte[e.end - e.begin];
						fileStream.Read(b, 0, b.Length);
						try
						{
							using (MemoryStream temp = new MemoryStream(b))
							{
								using (BinaryReader binary = new BinaryReader(temp))
								{
									d.Value.Import(binary);
								}
							}
                        }
                        catch(Exception ex)
                        {
							Debug.LogError(d.Key + " :mod data import error!");
							Debug.LogError(ex.Message + "\n" + ex.StackTrace);

                        }
                    }
                }
			}
		}

		class MySaveData
        {
			public string name;
			public long begin;
			public long end;
			public MySaveData(string name,long begin,long end)
            {
				this.name = name;
				this.begin = begin;
				this.end = end;
            }
        }
	}
}
