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
using crecheng.DSPModSave;

namespace StationManage
{
    [BepInDependency("crecheng.DSPModSave", "1.0.2")]
    [BepInPlugin("crecheng.StationManage", "StationManage", StationManage.Version)]
    public class StationManage : BaseUnityPlugin, IModCanSave
    {
        const string Version = " 0.1.0";
        static bool isShow = false;
        static bool isRun = false;

        /// <summary>
        /// 物流站对应信息
        /// </summary>
        static Dictionary<long, StationComponent[]> Data;
        static Dictionary<long, List<StationComponent>> DataList;
        void Start()
        {
            Harmony.CreateAndPatchAll(typeof(StationManage), null);
            Data = new Dictionary<long, StationComponent[]>();
            DataList = new Dictionary<long, List<StationComponent>>();

        }

        private void OnGUI()
        {
            if (isShow)
            {
                rect = GUI.Window(1935598240, rect, mywindowfunction, "物流站管理".CreTranslate());
                if (SelectStation != null)
                {
                    rect1 = GUI.Window(1935598241, rect1, StationInfo, "物流站信息".CreTranslate());
                }
            }
            else
            {
                if (GUI.Button(new Rect(Screen.width - 40, Screen.height / 7*5, 40, 40), "M"))
                {
                    isShow = !isShow;
                }
            }
        }
        static Rect rect = new Rect(300, 300, 900, 400);
        static Rect rect1 = new Rect(300, 100, 200, 400);


        [HarmonyPrefix]
        [HarmonyPatch(typeof(StationComponent), "InternalTickRemote")]
        static void StationGameTick(StationComponent __instance, int timeGene, double dt, float shipSailSpeed, float shipWarpSpeed,
            int shipCarries, StationComponent[] gStationPool, AstroPose[] astroPoses, VectorLF3 relativePos,
            Quaternion relativeRot, bool starmap, int[] consumeRegister)
        {

            if (__instance == null)
                return;
            //获取物流站的特征码
            long key = __instance.GetKey();
            //如果数据中该物流站
            if (Data.ContainsKey(key))
            {
                gStationPool = Data[key];
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlanetFactory), "GameTick")]
        static void GameTick(PlanetFactory __instance)
        {
            if (!isRun)
            {
                isRun = true;
            }
        }

        float t = 0;
        float t1 = 0;
        float t2 = 0;
        static bool isSelect = false;
        static StationComponent SelectStation = null;
        static StationComponent SelectStationAdd = null;
        static bool isAdd = false;
        static bool isClear = false;
        static int clearType = 0;
        Rect selectRect = new Rect(0, 0, 80, 80);
        void mywindowfunction(int windowid)
        {
            if(GUI.Button(new Rect(0, 0, 20, 20), "X"))
            {
                isShow = !isShow;
            }

            int i = 0;
            int count = 0;
            if (isRun)
            {
                if (GameMain.data != null && GameMain.data.galacticTransport != null && GameMain.data.galacticTransport.stationPool != null)
                {
                    var scs = GameMain.data.galacticTransport.stationPool;
                    for (i = 1; i < GameMain.data.galacticTransport.stationCursor;)
                    {
                        var sc = scs[i];
                        if (sc != null && sc.gid == i)
                        {
                            long key = sc.GetKey();
                            if (!DataList.ContainsKey(key))
                            {
                                float y = 100 + 20 * count - t;
                                if (y > 100)
                                {
                                    if (GUI.Button(new Rect(10, y, 150, 20), i + ":" + sc.GetStationName()))
                                    {
                                        SelectNull();
                                        isSelect = true;
                                        //selectRect.x = 160;
                                        //selectRect.y = 10 + 20 * i - t;
                                        SelectStation = sc;
                                        isAdd = true;
                                        isClear = false;
                                    }
                                }
                                count++;
                            }
                            if (SelectStation != null&&clearType==1)
                            {
                                if (GUI.Button(new Rect(710, 100 + 20 * i - t2, 150, 20), i + ":" + sc.GetStationName()))
                                {
                                    AddStationToList(SelectStation.GetKey(), sc);
                                    SelectNull();
                                }
                            }
                        }
                        i++;
                    }
                    
                }


                int num1 = 0;
                foreach (var sc in DataList)
                {
                    var station = GetStation(sc.Key);
                    if (station != null)
                    {
                        float y = 20 + num1 * 120 - t1;
                        if (GUI.Button(new Rect(210, y, 150, 20), "◆" + station.GetStationName()))
                        {
                            SelectNull();
                            SelectStation = station;
                            isSelect = true;
                            isClear = true;
                            clearType = 1;
                        }
                        GUI.Label(new Rect(210, y+20, 500, 20), station.GetStationName()+":" + "物流站的运输船只会去以下运输站".CreTranslate());
                        var list = sc.Value;
                        int num2 = 0;
                        foreach (var l in list)
                        {
                            if (l != null)
                            {
                                if (GUI.Button(new Rect(210 +  num2 % 4*111, 40+y +  num2 / 4* 20 , 110, 20), "▪" + l.GetStationName()))
                                {
                                    SelectNull();
                                    SelectStation = l;
                                    SelectStationAdd = station;
                                    isSelect = true;
                                    isClear = true;
                                    clearType = 2;

                                }
                                num2++;
                            }
                        }
                        num1++;
                    }
                }

            }

            if (isSelect && SelectStation != null)
            {
                GUI.Label(new Rect(10, 20, 150, 40),
                    SelectStation.GetStationPlanet()+"\n"+ SelectStation.GetStationName());
                if (isAdd)
                {
                    if (GUI.Button(new Rect(10, 55, 80, 20), "添加".CreTranslate()))
                    {
                        AddNewStation(SelectStation);
                        SelectNull();
                    }
                }
                if (isClear)
                {
                    if (GUI.Button(new Rect(30, 75, 80, 20), "移除".CreTranslate()))
                    {
                        if (clearType == 1)
                        {
                            RemoveStation(SelectStation);
                            SelectNull();
                        }
                        else  if(clearType==2)
                        {
                            RemoveStaionToList(SelectStationAdd.GetKey(), SelectStation);
                            SelectNull();
                        }
                    }
                }

            }

            int max = Math.Max(0, 20 * count + 30 - 200);
            t = GUI.VerticalSlider(new Rect(180, 30, 20, rect.height - 40), t, 0, max);


            int max1 = Math.Max(0, 120 * (DataList.Count + 1) + 30 - 500);
            t1 = GUI.VerticalSlider(new Rect(680, 30, 20, rect.height - 40), t1, 0, max1);

            int max2 = Math.Max(0, 20 * i + 30 - 200);
            t2 = GUI.VerticalSlider(new Rect(880, 30, 20, rect.height - 40), t2, 0, max2);

            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        /// <summary>
        /// 显示所选的物流站信息
        /// </summary>
        /// <param name="windowid"></param>
        void StationInfo(int windowid)
        {
            if (SelectStation != null)
            {
                if (SelectStation.storage != null)
                {
                    //显示物流站名字，允许重命名
                    SelectStation.name = GUI.TextArea(new Rect(20, 20, 160, 20), SelectStation.GetStationName());
                    //根据格子改变显示长度，适应mod
                    rect1.height = SelectStation.storage.Length * 57 + 60;
                    //显示物流站的物品栏
                    for (int i = 0; i < SelectStation.storage.Length; i++)
                    {
                        var s = SelectStation.storage[i];
                        //如果选择了物品
                        if (s.itemId > 0)
                        {
                            //获取游戏中该物品的资源
                            var item = LDB.items.Select(s.itemId);
                            //显示物品图片
                            GUI.Label(new Rect(10, 50 + i * 55, 40, 40), item.iconSprite.texture);
                            //显示物品名字
                            GUI.Label(new Rect(10, 90 + i * 55, 190, 20), item.name);
                            //显示物品数量和容量
                            GUI.Label(new Rect(55, 50 + i * 55, 100, 40), $"{"数量".CreTranslate()}:{s.count}\n{"最大".CreTranslate()}:{s.max}");
                            //显示物品的运输逻辑选项
                            GUI.Label(new Rect(160, 50 + i * 55, 30, 40),
                                $"{"地".CreTranslate()}-{s.localLogic.GetTransportLogic().CreTranslate()}\n" +
                                $"{"宇".CreTranslate()}-{s.remoteLogic.GetTransportLogic().CreTranslate()}");
                        }
                    }
                }
            }
                
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        public void SelectNull()
        {
            isSelect = false;
            SelectStation = null;
            SelectStationAdd = null;
            isAdd = false;
            isClear = false;
            clearType = 0;
        }

        /// <summary>
        /// 添加新的物流站到管理系统
        /// </summary>
        /// <param name="station"></param>
        public void AddNewStation(StationComponent station)
        {
            if (station != null)
            {
                var key = station.GetKey();
                if (key < 0)
                    return;
                Data.Add(key, null);
                DataList.Add(key, new List<StationComponent>());
            }
        }

        public void RemoveStation(StationComponent station)
        {
            if (station != null)
            {
                var key = station.GetKey();
                if (DataList.ContainsKey(key))
                {
                    DataList.Remove(key);
                    Data[key] = null;
                    Data.Remove(key);
                }
            }
        }

        public void AddStationToList(long key,StationComponent station)
        {
            if (station != null)
            {
                if (DataList.ContainsKey(key))
                {
                    if (DataList[key].Contains(station))
                        return;
                    if (station.GetKey() < 0)
                        return;
                    DataList[key].Add(station);
                    Data[key] = null;
                    Data[key] = DataList[key].ToArray();
                    GC.Collect();
                }
            }
        }

        public void RemoveStaionToList(long key,StationComponent station)
        {
            if (station != null)
            {
                if (DataList.ContainsKey(key))
                {
                    DataList[key].Remove(station);
                    Data[key] = null;
                    Data[key] = DataList[key].ToArray();
                    GC.Collect();
                }
            }
        }

        public void Export(BinaryWriter w)
        {
            w.Write(0L);
            w.Write(0L);
            w.Write(0L);
            w.Write(0L);
            w.Write(DataList.Count);
            foreach (var d in DataList)
            {
                w.Write(d.Key);
                w.Write(d.Value.Count);
                foreach (var sc in d.Value)
                {
                    w.Write(sc.GetKey());
                }
            }
        }

        public void Import(BinaryReader r)
        {
            isRun = false;
            Data.Clear();
            DataList.Clear();
            r.ReadInt64();
            r.ReadInt64();
            r.ReadInt64();
            r.ReadInt64();
            int num = r.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                long key = r.ReadInt64();
                int list = r.ReadInt32();
                DataList.Add(key,new List<StationComponent>());
                Data.Add(key, null);
                for (int j = 0; j < list; j++)
                {
                    var sc = GetStation(r.ReadInt64());
                    if (sc != null)
                        DataList[key].Add(sc);
                }
                Data[key] = DataList[key].ToArray();
            }
        }

        public void IntoOtherSave()
        {
            isRun = false;
            Data.Clear();
            DataList.Clear();
        }

        /// <summary>
        /// 通过key赖获得物流站数据
        /// </summary>
        /// <param name="key">key识别码</param>
        /// <returns></returns>
        public static StationComponent GetStation(long key) 
        {
            StationComponent station = null;
            int planetId = (int)(key >> 32);
            int stationId = (int)((key << 32) >> 32);
            if (GameMain.data != null && GameMain.data.galaxy != null)
            {
                var planet = GameMain.data.galaxy.PlanetById(planetId);
                if (planet.factory.transport.stationPool.Length > stationId)
                {
                    station= planet.factory.transport.stationPool[stationId];
                }
            }
            return station;
        }
    }
}
