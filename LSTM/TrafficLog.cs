//using System;
//using System.Text;

using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSTMMod
{
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
        public bool isFromDemand;

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

        public string distance
        {
            get
            {
                float d = LSTMStarDistance.StarDistance(fromPlanet / 100, toPlanet / 100);
                if (d <= 0)
                {
                    return "0ly";
                }
                return d.ToString("F1") + "ly";
            }
        }

        public string time
        {
            get
            {
                float dur = Time.realtimeSinceStartup - realtimeSinceStartup;
                if (dur < 60.0)
                {
                    return dur.ToString("F0") + "s";
                }
                else if (dur < 3600.0)
                {
                    return (dur/60.0).ToString("F0") + "m";
                }
                return (dur / 3600.0).ToString("F1") + "h";
            }
        }

        public string Info()
        {
            return realtimeSinceStartup.ToString("F1") + "]" + fromPlanet + "/" + fromIndex + "->" + toPlanet + "/" + toIndex + ":" + itemId; 
        }
    }

    public class TrafficLog
    {
        public static int trafficLogsSize = 9999;
        public static TrafficLogData[] trafficLogs = null;
        public static int trafficLogsCursor = 0;
        public static void ResetLog()
        {
            trafficLogs = new TrafficLogData[trafficLogsSize];
            trafficLogsCursor = 0;
        }
        public static void AddLog(TrafficLogData logData)
        {
            lock (trafficLogs) {
                if (trafficLogs == null)
                {
                    ResetLog();
                }
                trafficLogs[trafficLogsCursor] = logData;
                trafficLogsCursor++;
                if (trafficLogsCursor >= trafficLogsSize)
                {
                    trafficLogsCursor = 0;
                }
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

        //新しい順
        public static IEnumerable<TrafficLogData> AllTrafficLogData(int planetId=0, int itemId=0, int filterStationGid=0, int filterIndex=0)
        {
            if (trafficLogs == null)
            {
                yield break;
            }
            lock (trafficLogs)
            {
                int i = trafficLogsCursor - 1;

                while (true)
                {
                    if (i < 0)
                    {
                        i = trafficLogsSize - 1;
                    }
                    if (i == trafficLogsCursor)
                    {
                        break;
                    }
                    if (trafficLogs[i] != null)
                    {
                        TrafficLogData item = trafficLogs[i];
                        if (planetId != 0)
                        {
                            if ((item.fromPlanet == planetId) || (item.toPlanet == planetId))
                            {
                                yield return item;
                            }
                        }
                        else if (itemId != 0)
                        {
                            if (item.itemId == itemId)
                            {
                                yield return item;
                            }
                        }
                        else if (filterStationGid != 0)
                        {
                            if ((item.toStationGid == filterStationGid && item.toIndex == filterIndex)
                                || (item.fromStationGid == filterStationGid && item.fromIndex == filterIndex))
                            {
                                yield return item;
                            }
                        }
                        else
                        {
                            yield return trafficLogs[i];
                        }
                    }
                    else
                    {
                        break;
                    }
                    i--;
                }
            }
        }

        public static IEnumerable<TrafficLogData> TrafficLogDataForPlanet(int planetId)
        {
            return AllTrafficLogData(planetId);
        }

        public static IEnumerable<TrafficLogData> TrafficLogDataForItem(int itemId)
        {
            return AllTrafficLogData(0, itemId);
        }

        public static IEnumerable<TrafficLogData> TrafficLogDataForStationSlot(int filterStationGid, int filterIndex)
        {
            return AllTrafficLogData(0, 0, filterStationGid, filterIndex);
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
