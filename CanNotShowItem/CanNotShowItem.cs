using System;
using HarmonyLib;
using UnityEngine;
using BepInEx;
using System.IO;
using System.Collections.Generic;

namespace CanNotShowItem
{
	[BepInPlugin("crecheng.CanNotShowItem", "CanNotShowItem","1.0.0")]

	public class CanNotShowItem:BaseUnityPlugin
    {
		void Start()
		{

			Harmony.CreateAndPatchAll(typeof(CanNotShowItem), null);
			rect.height = Screen.height - 400;
		}

		void OnGUI()
		{
			rect = GUI.Window(1935598205, rect, mywindowfunction, "");
			rect.x = Screen.width - rect.width;
			if (GUI.Button(new Rect(rect.x - 20,rect.y , 20, 40), "<"))
			{
				isShowW = !isShowW;
				if (!isShowW)
				{
					rect.width = 0;
					rect.height = 0;
				}
				else
				{
					rect.width = 200;
					rect.height = Screen.height - 400;

				}
			}
		}
		float t = 0;
		static bool isShowW = true;
		void mywindowfunction(int windowid)
		{


			float max = Math.Max(100 + 20 * (showModel.Count + 4)-rect.height,0);
			t = GUI.VerticalSlider(new Rect(180, 30, 20, rect.height - 40), t, 0, max);
			showItem[0] = GUI.Toggle(new Rect(20, 40-t, 100, 20), showItem[0], ST.货物);
			showItem[1] = GUI.Toggle(new Rect(20, 60-t, 100, 20), showItem[1], ST.传送带);
			showItem[2] = GUI.Toggle(new Rect(20, 80-t, 100, 20), showItem[2], ST.小飞机);
			showItem[3] = GUI.Toggle(new Rect(20, 100 - t , 100, 20), showItem[3], ST.全部实体);
			int i = 0;
			if (isGetModelData)
			{
				foreach (var d in showModel)
				{
					showModel[d.Key].b = GUI.Toggle(new Rect(20 , 120 + 20 *i-t, 200, 20), d.Value.b, d.Value.name + ":" + d.Key);
					i++;
				}
			}
			GUI.DragWindow(new Rect(0, 0, 10000, 10000));
		}

		static bool[] showItem = new bool[4] { false, false, false, false };
		static bool[] showItemT = new bool[4];
		static bool isGetModelData = false;
		static Dictionary<int, B> showModel = new Dictionary<int, B>();
		static Dictionary<int, ObjectRenderer> tempModelData = new Dictionary<int, ObjectRenderer>();
		static void GetModelData()
		{
			Debug.Log("GetModelData");
			var data = LDB.items.dataArray;
			foreach (var d in data)
			{
				if (!showModel.ContainsKey(d.ModelIndex))
					showModel.Add(d.ModelIndex, new B(false, d.name));
			}
			var data1 = LDB.models.dataArray;
			foreach(var d in data1)
            {
				if (!showModel.ContainsKey(d.ID))
					showModel.Add(d.ID, new B(false, d.name));
			}
		}

		static void RemoveShowModelData(ObjectRenderer[] or)
		{
			if (or != null)
			{
				tempModelData.Clear();
				for (int i = 0; i < or.Length; i++)
				{
					if (or[i] != null)
					{
						var d = or[i];
						if (showModel.ContainsKey(d.modelId) && showModel[d.modelId].b)
						{
							tempModelData.Add(i, d);
							or[i] = null;
						}
					}
				}
			}
		}

		static void SetShowModelData(ObjectRenderer[] or)
		{
			if (or != null)
			{
				foreach (var d in tempModelData)
				{
					or[d.Key] = d.Value;
				}
			}
		}


		[HarmonyPrefix]
		[HarmonyPatch(typeof(FactoryModel), "DrawInstancedBatches")]
		static void oth4(FactoryModel __instance)
		{
			if (!isGetModelData)
			{
				GetModelData();
				isGetModelData = true;
			}
			var _this = __instance;
			showItemT[0] = _this.disableCargos;
			showItemT[1] = _this.disableTraffics;
			showItemT[2] = _this.disableLogisticDrones;
			showItemT[3] = _this.disableFactoryEntities;
			_this.disableCargos = showItem[0];
			_this.disableTraffics = showItem[1];
			_this.disableLogisticDrones = showItem[2];
			_this.disableFactoryEntities = showItem[3];
			var entityData = _this.gpuiManager.objectRenderers;
			if (!_this.disableFactoryEntities)
			{
				RemoveShowModelData(entityData);
			}
		}

		[HarmonyPostfix]
		[HarmonyPatch(typeof(FactoryModel), "DrawInstancedBatches")]
		static void oth5(FactoryModel __instance)
		{
			var _this = __instance;
			var entityData = _this.gpuiManager.objectRenderers;
			if (!_this.disableFactoryEntities)
			{
				SetShowModelData(entityData);
			}
			_this.disableCargos = showItemT[0];
			_this.disableTraffics = showItemT[1];
			_this.disableLogisticDrones = showItemT[2];
			_this.disableFactoryEntities = showItemT[3];
		}

		//static int dspSpeed = 0;
		//static int dspKSpeed = 0;
		static Rect rect = new Rect(50, 50, 200, 50);

		class B
		{
			public B(bool b, string name)
			{
				this.b = b;
				this.name = name;
			}
			public string name;
			public bool b;
		}
	}
}
