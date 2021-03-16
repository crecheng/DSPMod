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


namespace ChangeSun
{
    [BepInDependency("crecheng.DSPModSave", "1.0.2")]
    [BepInPlugin("crecheng.ChangeSun", "ChangeSun", ChangeSun.Version)]
    public class ChangeSun:BaseUnityPlugin,IModCanSave
    {

        public const string Version = "1.0.0";
        System.Random random = new System.Random();
        static ChangeSun instance = null;
        static bool isRun = false;
        private static bool isShow = true;
        private static Rect rect = new Rect(330f, 30f, 250f, 200f);
        //已经更换的星系数据
        static Dictionary<int, SData> data = new Dictionary<int, SData>();

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(ChangeSun), null);
        }

        public void OnGUI()
        {
            if (instance == null && BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("crecheng.ChangeSun"))
            {
                instance = (ChangeSun)BepInEx.Bootstrap.Chainloader.PluginInfos["crecheng.ChangeSun"].Instance;
            }
            rect = GUI.Window(1935598235, rect, mywindowfunction, "偷天换日");
        }

        //随机一个获取并更换一个星系
        IEnumerator RadomSun(StarData[] stars,int index,EStarType type,ESpectrType type1= ESpectrType.O)
        {
            yield return new WaitForFixedUpdate();//等待帧结束
            var star = stars[index];
            //暂存原始星系数据
            var oldstar = star;
            int seed = random.Next();
            if (data.ContainsKey(index))
            {
                data[index].seed = seed;
                data[index].type = type;
                data[index].spectrType = type1;
                //生成新的星系
                stars[index] = StarGen.CreateStar(GameMain.galaxy, oldstar.position, index + 1,seed , type, type1);
                //还给一些原始数据
                SwapStar(oldstar, stars[index]);

                if (selectData != null)
                    selectData = stars[index];
            }
            else
            {
                data.Add(index, new SData(index, seed, type, type1));
                //生成新的星系
                stars[index] = StarGen.CreateStar(GameMain.galaxy, oldstar.position, index + 1, seed, type,type1);
                //还给一些原始数据
                SwapStar(oldstar, stars[index]);
                selectData = stars[index];
            }
            SwapFinally();

        }

        static string fingStarNameAll = string.Empty;
        private static Dictionary<String, StarData> resDate = new Dictionary<string, StarData>();
        private StarData selectData = null;
        private Rect temp=new Rect(330f, 30f, 250f, 200f);
        void mywindowfunction(int windowid)
        {
            if(GUI.Button(new Rect(0, 0, 20, 20), "X"))
            {
                isShow = !isShow;
                if (isShow)
                {
                    rect = temp;
                }
                else
                {
                    rect.width = 30;
                    rect.height = 20;
                    temp.x = rect.x;
                    temp.y = rect.y;
                }
            }
            GUI.Label(new Rect(10, 20, 300, 300),ST.Tip);
            fingStarNameAll = GUI.TextField(new Rect(10, 40, 100, 20), fingStarNameAll, 20);
            if (GUI.Button(new Rect(110, 40, 40, 20), ST.查找))
            {
                var galaxyData = GameMain.data.galaxy;
                if (galaxyData != null)
                {
                    findStar(galaxyData, fingStarNameAll);
                }
            }
            if (selectData != null)
            {
                GUI.Label(new Rect(10, 60, 100, 20), selectData.name);
            }
            if (GUI.Button(new Rect(110, 60, 40, 20), ST.更换))
            {
                if(selectData!=null)
                    StartCoroutine(RadomSun(GameMain.data.galaxy.stars, selectData.id-1,(EStarType)type1,(ESpectrType)type2));
            }
            input1= GUI.TextField(new Rect(160, 40, 40, 20), type1.ToString(), 1);
            GUI.Label(new Rect(160, 60, 60, 100), "0-母星\n1-巨星\n2-白矮\n3-中子\n4-黑洞");
            input2= GUI.TextField(new Rect(210, 40, 30, 20), type2.ToString(), 1);
            GUI.Label(new Rect(210, 60, 30, 140), "0-M\n1-K\n2-G\n3-F\n4-A\n5-B\n6-O\n7-X");
            if (int.TryParse(input1,out type1))
            {
                if (type1 < 0 || type1 > 4)
                    type1 = 0;
            }
            if (int.TryParse(input2, out type2))
            {
                if (type2 < 0 || type2 > 7)
                    type2 = 0;
            }

            int i = 0;
            foreach (var d in resDate)
            {
                if(GUI.Button(new Rect(10, 80 + 22 * i, 100, 20), d.Key))
                {
                    selectData = d.Value;
                }
                i++;
            }
            GUI.DragWindow(new Rect(0, 0, 10000, 10000));
        }

        private static void findStar(GalaxyData g, string s)
        {
            resDate.Clear();
            foreach (var d in g.stars)
            {
                if (d.name.IndexOf(s, StringComparison.InvariantCultureIgnoreCase) > -1)
                {
                    resDate.Add(d.name, d);
                }
            }
            rect.height = 220 + resDate.Count * 20;
        }
        static string input1 = string.Empty;
        static string input2 = string.Empty;
        static int type1;
        static int type2;

        void RadomSun(StarData[] stars, int index, EStarType type, int seed )
        {
            var star = stars[index];
            //暂存原始星系数据
            var oldstar = star;
            if (seed == -1)
                seed = random.Next();
            if (data.ContainsKey(index))
            {
                data[index].seed = seed;
                //生成新的星系
                stars[index] = StarGen.CreateStar(GameMain.galaxy, oldstar.position, index + 1, seed, type, ESpectrType.O);
                //还给一些原始数据
                SwapStar(oldstar, stars[index]);
                SwapFinally();
            }
            else
            {
                data.Add(index, new SData(index, seed, type, ESpectrType.O));
            }

        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameData), "GameTick")]
        static void GameTick(GameData __instance)
        {
            if (!isRun)
            {
                var galaxy = __instance.galaxy;
                if (data.Count>0 )
                {
                    foreach(var d in data)
                    {
                        instance.RadomSun(galaxy.stars, d.Key, d.Value.type, d.Value.seed);
                    }
                }
                GameMain.data.galaxy = galaxy;
                instance.SwapFinally();
                isRun = true;
            }
        }
        

        //对更新后的数据更换模型和ui数据
        public void SwapFinally()
        {
            //获取游戏数据
            var galaxy = GameMain.data.galaxy;

            foreach (var d in data)
            {
                int i = d.Key;
                //更新ui数据
                var StarUI = UIRoot.instance.uiGame.starmap.starUIs[i];
                StarUI._Free();
                //设置为新数据
                StarUI._Init(galaxy.stars[i]);

                //更新宇宙模型渲染数据
                GameMain.universeSimulator.starSimulators[i].SetStarData(galaxy.stars[i]);
                GameMain.universeSimulator.starSimulators[i].gameObject.name = galaxy.stars[i].displayName;
            }
        }

        public void SwapStar(StarData oldStar, StarData newStar)
        {
            newStar.planetCount = oldStar.planetCount;
            newStar.planets = oldStar.planets;
            newStar.uPosition = oldStar.uPosition;
            for (int i = 0; i < newStar.planets.Length; i++)
            {
                //将原始星球的行星移到新行星上
                newStar.planets[i].star = newStar;
                //更换行星名字
                newStar.planets[i].star.name.Replace(oldStar.name, newStar.name);
            }
        }


        public void Export(BinaryWriter w)
        {
            w.Write(data.Count);
            foreach (var d in data)
            {
                d.Value.Export(w);
            }
        }

        public void Import(BinaryReader r)
        {
            isRun = false;
            data.Clear();
            int num = r.ReadInt32();
            for (int i = 0; i < num; i++)
            {
                SData temp = new SData();
                temp.Import(r);
                data.Add(temp.index, temp);
            }
        }

        public void IntoOtherSave()
        {
            data.Clear();
        }

        class SData
        {
            public int seed;
            public EStarType type;
            public ESpectrType spectrType;
            public int index;
            public int other1;
            public int other2;
            public int other3;
            public SData(int index, int seed,EStarType type,ESpectrType eSpectr)
            {
                this.seed = seed;
                this.type = type;
                this.spectrType = eSpectr;
                this.index = index;
            }

            public SData()
            {

            }

            public void Export(BinaryWriter w)
            {
                w.Write(index);
                w.Write(seed);
                w.Write((int)type);
                w.Write((int)spectrType);
                w.Write(other1);
                w.Write(other2);
                w.Write(other3);
            }

            public void Import(BinaryReader r)
            {
                index = r.ReadInt32();
                seed = r.ReadInt32();
                type = (EStarType)r.ReadInt32();
                spectrType = (ESpectrType)r.ReadInt32();
                other1 = r.ReadInt32();
                other2 = r.ReadInt32();
                other3 = r.ReadInt32();
            }
        }
    }
}
