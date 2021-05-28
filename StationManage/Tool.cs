using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace StationManage
{
    public static class Tool
    {

        /// <summary>
        /// 获取物流站的key
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public static long GetKey(this StationComponent station)
        {
            if (station != null)
            {
                long key = station.planetId;
                key <<= 32;
                key += station.id;
                return key;
            }
            else
                return -1;
        }

        /// <summary>
        /// 获取物流塔的名字，
        /// 如果物流塔没名字，返回星球名字+物流站id
        /// </summary>
        /// <param name="station"></param>
        /// <returns></returns>
        public static string GetStationName(this StationComponent station)
        {
            if (station != null)
            {
                if (station.name != null)
                    return station.name;
                else
                {
                    string s = string.Empty;
                    var planet = GameMain.data.galaxy.PlanetById(station.planetId);
                    s += planet.displayName + "-" + station.id;
                    if (station.isCollector)
                        s += "-c";
                    return s;
                }

            }
            return "";
        }

        public static string GetStationPlanet(this StationComponent station)
        {
            if (station != null)
            {
                return GameMain.data.galaxy.PlanetById(station.planetId).displayName + "-" + 
                    station.id+(station.isCollector?"-"+"采集器".CreTranslate():"");
            }
            return "";
        }

        public static string GetTransportLogic(this ELogisticStorage eLogistic)
        {
            switch (eLogistic)
            {
                case ELogisticStorage.None:return "存";
                case ELogisticStorage.Supply:return "出";
                case ELogisticStorage.Demand:return "进";
                default:return "";
            }
        }

        static Dictionary<string, string> st = null;

        public static string CreTranslate (this string s)
        {

            if (Localization.language == Language.zhCN)
                return s;
            if (st == null)
            {
                st.Add("数量:", "count:");
                st.Add("最大:", "max:");
                st.Add("地", "Local");
                st.Add("宇", "galaxy");
                st.Add("存", "on");
                st.Add("进", "in");
                st.Add("出", "out");
                st.Add("采集器", "Collector");
                st.Add("添加", "add");
                st.Add("移除", "remove");
                st.Add("物流站信息", "Station Info");
                st.Add("物流站管理", "Station Manage");
                st.Add("物流站的运输船只会去以下运输站", "the ships will only go to the following stations");
            }


            if (st.ContainsKey(s))
                return st[s];
            else
                return s;
        }
    }
}
