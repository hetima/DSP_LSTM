//using System;
//using System.Text;

using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSTMMod
{
    public interface TrafficLogDelegate
    {
        //ShutAllFunctionWindow でなにかチェックしたい場合
        void TrafficLogReseted();
        //void TrafficLogAdded(TrafficLogData logData);
    }

    public class TrafficLogData
    {
        public int fromPlanet;
        public int fromStationGid;
        public int fromIndex;
        public int toPlanet;
        public int toStationGid;
        public int toIndex;
        public int itemId;
        public float realtimeSinceStartup;
        //public string timeString;
        public bool isFromDemand;

        public string fetchedTime;

        public PlanetData fromPlanetData
        {
            get
            {
                return GameMain.galaxy.PlanetById(fromPlanet);
            }
        }

        public PlanetData toPlanetData
        {
            get
            {
                return GameMain.galaxy.PlanetById(toPlanet);
            }
        }

        public string fromPlanetName
        {
            get
            {
                return GameMain.galaxy.PlanetById(fromPlanet).displayName;
            }
        }

        public string toPlanetName
        {
            get
            {
                return GameMain.galaxy.PlanetById(toPlanet).displayName;
            }
        }

        public string distanceString
        {
            get
            {
                float d = LSTMStarDistance.StarDistance(fromPlanet / 100, toPlanet / 100);
                if (d <= 0)
                {
                    return " ";
                }
                return d.ToString("F1") + "ly";
            }
        }

        public string time
        {
            get
            {
                //return timeString;
                float dur = Time.realtimeSinceStartup - realtimeSinceStartup;
                if (dur < 60.0)
                {
                    return dur.ToString("F0") + "s";
                }
                else if (dur < 3600.0)
                {
                    return (dur / 60.0).ToString("F0") + "m";
                }
                return (dur / 3600.0).ToString("F1") + "h";
            }
        }

        public string Info()
        {
            return "[" + time + "]" + fromPlanet + "/" + fromIndex + "->" + toPlanet + "/" + toIndex + ":" + itemId; 
        }
    }

    public class TrafficLog
    {
        public static TrafficLogDelegate trafficLogDelegate;
        public static TrafficLogData[] trafficLogs = null;
        public static int trafficLogsCursor = 0;
        public static bool keepLog = true;
        public static int trafficLogsSize = 9999;

        public static void ResetLog()
        {
            trafficLogs = new TrafficLogData[trafficLogsSize];
            trafficLogsCursor = 0;
            trafficLogDelegate?.TrafficLogReseted();
        }
        public static void AddLog(TrafficLogData logData)
        {
            lock (trafficLogs) {
                if (trafficLogs == null)
                {
                    ResetLog();
                }
                if (keepLog)
                {
                    trafficLogs[trafficLogsCursor] = logData;
                    trafficLogsCursor++;
                    if (trafficLogsCursor >= trafficLogsSize)
                    {
                        trafficLogsCursor = 0;
                    }
                }
                //trafficLogDelegate.TrafficLogAdded(logData);
            }
        }

        public static void Test()
        {
            LSTM.Log("LogTest:Cursor==" + trafficLogsCursor);

            foreach (var item in AllTrafficLogData())
            {
                LSTM.Log(item.Info());
            }
        }
        public static IEnumerable<TrafficLogData> AllTrafficLogData()
        {
            if (trafficLogs == null || !keepLog)
            {
                yield break;
            }
            lock (trafficLogs)
            {
                int i = trafficLogsCursor;

                while (true)
                {
                    i--;
                    if (i < 0)
                    {
                        i = trafficLogsSize - 1;
                    }
                    if (i == trafficLogsCursor || trafficLogs[i] == null)
                    {
                        break;
                    }

                    yield return trafficLogs[i];
                }
            }
        }

        public static IEnumerable<TrafficLogData> GetTrafficLogData(int starId, int planetId, int itemId, int filterStationGid)
        {
            if (trafficLogs == null || !keepLog)
            {
                yield break;
            }
            lock (trafficLogs)
            {
                int i = trafficLogsCursor;

                while (true)
                {
                    i--;
                    if (i < 0)
                    {
                        i = trafficLogsSize - 1;
                    }
                    if (i == trafficLogsCursor || trafficLogs[i] == null)
                    {
                        break;
                    }

                    TrafficLogData item = trafficLogs[i];
                    if (starId != 0)
                    {
                        if ((item.fromPlanet/100 != starId) && (item.toPlanet/100 != starId))
                        {
                            continue;
                        }
                    }
                    if(planetId != 0)
                    {
                        if ((item.fromPlanet != planetId) && (item.toPlanet != planetId))
                        {
                            continue;
                        }
                    }
                    if (itemId != 0)
                    {
                        if (item.itemId != itemId)
                        {
                            continue;
                        }
                    }
                    if (filterStationGid != 0)
                    {
                        if ((item.toStationGid != filterStationGid)
                            && (item.fromStationGid != filterStationGid))
                        {
                            continue;
                        }
                    }

                    yield return item;

                }
            }
        }




        public static void TakeLog(StationComponent sc, int index)
        {
            if (index < 0)
            {
                return;
            }
            TrafficLogData data = new TrafficLogData();
            data.fromPlanet = sc.workShipDatas[index].planetA;
            data.toPlanet = sc.workShipDatas[index].planetB;
            data.fromStationGid = sc.gid;
            data.fromIndex = sc.workShipOrders[index].thisIndex;
            data.toIndex = sc.workShipOrders[index].otherIndex;
            data.toStationGid = sc.workShipDatas[index].otherGId;
            data.itemId = sc.workShipDatas[index].itemId;
            data.realtimeSinceStartup = Time.realtimeSinceStartup;
            data.isFromDemand = (sc.workShipOrders[index].thisOrdered != 0);
            //DateTime dt = DateTime.Now;
            //data.timeString = dt.Hour.ToString("D2") + ":" + dt.Minute.ToString("D2");
            AddLog(data);
        }

        public static class Patch
        {
            [HarmonyPrefix, HarmonyPatch(typeof(GameMain), "Begin")]
            public static void GameMain_Begin_Prefix()
            {
                //reset
                ResetLog();
            }

            [HarmonyPrefix, HarmonyPatch(typeof(StationComponent), "IdleShipGetToWork"), HarmonyAfter("dsp.nebula-multiplayer")]
            public static void StationComponent_IdleShipGetToWork_Prefix(StationComponent __instance)
            {
                int idx = __instance.workShipCount;
                if (idx <= 0)
                {
                    return;
                }
                TakeLog(__instance, idx - 1);

            }
        }
    }
}
