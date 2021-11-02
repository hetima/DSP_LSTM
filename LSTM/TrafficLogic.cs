
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;

/*
 GalacticTransport.RefreshTraffic -> StationComponent.RematchRemotePairs -> AddRemotePair
 PlanetTransport.RefreshTraffic -> StationComponent.RematchLocalPairs -> AddLocalPair
 の流れで追加されるときにフィルタする方法もあるが、Condition系クラスが数をチェックしてるので変更すると実績等に影響するかもしれない
 よく分からんので直前にフィルタする
 
 
 */


namespace LSTMMod
{
    public class TrafficLogic
    {

        //StationComponent.tripRangeShips 
        //0を返すとすべてのステーションが対象外になり輸送を開始しない
        //どのペアとの距離を調べているかは、このメソッドが呼ばれた時点の remotePairProcess に入っている
        public static double TripRangeShipsInTickRemote(StationComponent sc)
        {
            SupplyDemandPair supplyDemandPair = sc.remotePairs[sc.remotePairProcess];
            StationComponent[] gStationPool = UIRoot.instance.uiGame.gameData.galacticTransport.stationPool;

            if (LSTM.enableTLRemoteCluster.Value)
            {
                StationComponent demandCmp = gStationPool[supplyDemandPair.demandId];
                StationComponent supplyCmp = gStationPool[supplyDemandPair.supplyId];
                int itemId = supplyCmp.storage[supplyDemandPair.supplyIndex].itemId;
                //空間歪曲器は除外
                if (itemId != 1210)
                {
                    if (!TLCluster.IsSameRemoteCluster(supplyCmp, demandCmp))
                    {
                        return 0;
                    }
                }
            }

            return sc.tripRangeShips;
        }

        public static class Patch
        {

            // StationComponent InternalTickRemote
            // Prefixで完全に置き換えているmod(GalacticScale, StationRangeLimiter 等)とは互換性なし こっちが使えなくなる
            [HarmonyTranspiler, HarmonyPatch(typeof(StationComponent), "InternalTickRemote")]
            public static IEnumerable<CodeInstruction> StationComponent_InternalTickRemote_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> ins = instructions.ToList();
                MethodInfo m_TripRangeShips = typeof(TrafficLogic).GetMethod(nameof(TripRangeShipsInTickRemote));
                FieldInfo f_TripRangeShips = AccessTools.Field(typeof(StationComponent), nameof(StationComponent.tripRangeShips));

                //bool flag3 = num16 < this.tripRangeShips;
                //IL_02b9: ldloc.s trip
                //IL_02bb: ldarg.0      // this
                //IL_02bc: ldfld float64 StationComponent::tripRangeShips //ここを call に
                //IL_02c1: clt
                //IL_02c3: stloc.s flag1

                //bool flag5 = num17 < this.tripRangeShips;
                //IL_07d4: ldloc.s trip_V_39
                //IL_07d6: ldarg.0      // this
                //IL_07d7: ldfld float64 StationComponent::tripRangeShips //ここを call に
                //IL_07dc: clt
                //IL_07de: stloc.s flag1_V_40

                //this.tripRangeShips を置き換える
                int patchCount = 0;
                for (int i = 100; i < ins.Count - 100; i++)
                {
                    if (ins[i].opcode == OpCodes.Ldarg_0)
                    {
                        if (ins[i + 1].opcode == OpCodes.Ldfld && ins[i + 1].operand is FieldInfo o && o == f_TripRangeShips)
                        {
                            ins[i + 1].opcode = OpCodes.Call;
                            ins[i + 1].operand = m_TripRangeShips;
                            patchCount++;
                            i++;
                        }
                    }
                }
                if (patchCount != 2)
                {
                    LSTM.Logger.LogInfo("StationComponent_InternalTickRemote_Transpiler (tripRangeShips) seems fail");
                }

                return ins.AsEnumerable();
            }
        }
    }
}
