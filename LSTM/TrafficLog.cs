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
        public static IEnumerable<TrafficLogData> AllTrafficLogData(int filterStationGid = 0, int filterIndex = 0)
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
                    if (filterStationGid > 0)
                    {
                        if ((trafficLogs[i].toStationGid == filterStationGid && trafficLogs[i].toIndex == filterIndex) 
                         || (trafficLogs[i].fromStationGid == filterStationGid && trafficLogs[i].fromIndex == filterIndex))
                        {
                            yield return trafficLogs[i];
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
