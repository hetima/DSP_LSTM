
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
            double result = sc.tripRangeShips;
            StationComponent demandCmp = gStationPool[supplyDemandPair.demandId];
            StationComponent supplyCmp = gStationPool[supplyDemandPair.supplyId];
            double demandRange = demandCmp.tripRangeShips;
            double supplyRange = supplyCmp.tripRangeShips;
            int itemId = supplyCmp.storage[supplyDemandPair.supplyIndex].itemId;

            if (LSTM.enableTLRemoteCluster.Value)
            {
                //空間歪曲器は除外
                if (itemId != 1210)
                {
                    if (!TLCluster.IsSameRemoteCluster(supplyCmp, demandCmp))
                    {
                        return 0;
                    }
                }
            }

            //Remote Distance/Capacity Balance
            if (LSTM.enableTLDCBalance.Value)
            {
                float max;

                max = demandCmp.storage[supplyDemandPair.demandIndex].max;
                if (max >= 2000)
                {
                    float multi = LSTM.TLDCDemandMultiplier.Value;
                    int num = demandCmp.storage[supplyDemandPair.demandIndex].totalSupplyCount;
                    float rate = num / max;
                    //demand距離を減らす利点はあるのか?
                    //if (rate >= 0.7 && div > 1 && div < 100.01f)
                    //{
                    //    demandRange /= div;
                    //}
                    if (rate < 0.299 && multi > 1.01f && multi < 100.01f)
                    {
                        demandRange *= multi;
                    }
                }

                max = supplyCmp.storage[supplyDemandPair.supplyIndex].max;
                if(max >= 2000)
                {
                    float multi = LSTM.TLDCSupplyMultiplier.Value;
                    float div = LSTM.TLDCSupplyDenominator.Value;

                    int num = supplyCmp.storage[supplyDemandPair.supplyIndex].totalSupplyCount;
                    float rate = num / max;
                    if (rate >= 0.7 && multi > 1.01f && multi < 100.01f)
                    {
                        supplyRange *= multi;
                    }
                    else if (rate < 0.301 && div > 1.01f && div < 100.01f)
                    {
                        supplyRange /= div;
                    }
                }

                if (sc.gid == supplyDemandPair.demandId)
                {
                    result = demandRange;
                }
                else
                {
                    result = supplyRange;
                }
            }

            //Remote Demand Delay
            if (LSTM.enableTLRemoteDemandDelay.Value) {
                //remote local 両方 demand
                if (demandCmp.storage[supplyDemandPair.demandIndex].remoteLogic == demandCmp.storage[supplyDemandPair.demandIndex].localLogic)
                {
                    float total = demandCmp.storage[supplyDemandPair.demandIndex].totalSupplyCount;
                    //float actual = demandCmp.storage[supplyDemandPair.demandIndex].count;
                    float max = demandCmp.storage[supplyDemandPair.demandIndex].max;
                    if (max >= 5000 && total / max >= 0.98 )
                    {
                        return 0;
                    }
                }
            }

            if (LSTM.enableTLConsiderOppositeRange.Value)
            {
                //空間歪曲器は除外、 demand 1000未満も除外
                if (itemId != 1210 && demandCmp.storage[supplyDemandPair.demandIndex].max >= 1000)
                {
                    result = demandRange >= supplyRange ? supplyRange : demandRange;
                }
            }

            return result;
        }

        // tripRangeDrones 20° == 0.94, 180° == -1 小さい方が距離が遠い
        // 1 + 1E-06(1.000001)より大きな数を返せば対象外になり輸送を開始しない
        public static double TripRangeDronesInTickLocal(StationComponent sc, StationComponent[] stationPool)
        {
            SupplyDemandPair supplyDemandPair = sc.localPairs[sc.localPairProcess];
            double result = sc.tripRangeDrones;
            StationComponent demandCmp = stationPool[supplyDemandPair.demandId];
            StationComponent supplyCmp = stationPool[supplyDemandPair.supplyId];
            int itemId = supplyCmp.storage[supplyDemandPair.supplyIndex].itemId;
            
            if (LSTM.enableTLLocalCluster.Value)
            {
                //空間歪曲器は除外
                if (itemId != 1210)
                {
                    if (!TLCluster.IsSameLocalCluster(supplyCmp, demandCmp))
                    {
                        return 2.0;
                    }
                }
            }

            if (LSTM.enableTLConsiderOppositeRange.Value) 
            {
                //空間歪曲器は除外
                if (itemId != 1210)
                {
                    result = demandCmp.tripRangeDrones <= supplyCmp.tripRangeDrones ? supplyCmp.tripRangeDrones : demandCmp.tripRangeDrones;
                }
            }

            return result;
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
                    LSTM.Logger.LogInfo("StationComponent_InternalTickRemote_Transpiler (tripRangeShips) seems wrong");
                }

                return ins.AsEnumerable();
            }

            [HarmonyTranspiler, HarmonyPatch(typeof(StationComponent), "InternalTickLocal")]
            public static IEnumerable<CodeInstruction> StationComponent_InternalTickLocal_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> ins = instructions.ToList();
                MethodInfo m_TripRange = typeof(TrafficLogic).GetMethod(nameof(TripRangeDronesInTickLocal));
                FieldInfo f_TripRange = AccessTools.Field(typeof(StationComponent), nameof(StationComponent.tripRangeDrones));
                //StationComponent の planetId は GalacticTransport によって書き込まれるので
                //星間輸送しているものしか設定されない
                //惑星のstationPoolが必要なので引数から貰う

                // if (num18 >= this.tripRangeDrones - 1E-06)
                int patchCount = 0;
                for (int i = 0; i < ins.Count; i++)
                {
                    if (i > 0 && ins[i].opcode == OpCodes.Ldfld && ins[i].operand is FieldInfo o && o == f_TripRange)
                    {
                        if (ins[i-1].opcode == OpCodes.Ldarg_0)
                        {
                            ins[i].opcode = OpCodes.Call;
                            ins[i].operand = m_TripRange;
                            patchCount++;
                            CodeInstruction c = new CodeInstruction(OpCodes.Ldarg_S, 7);
                            yield return c;
                            yield return ins[i];
                            continue;
                        }
                    }
                    yield return ins[i];
                }
                if (patchCount != 3)
                {
                    LSTM.Logger.LogInfo("StationComponent_InternalTickLocal_Transpiler (tripRangeDrones) seems wrong");
                }
            }
            
        }
    }
}
