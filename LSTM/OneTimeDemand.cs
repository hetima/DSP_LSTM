

namespace LSTMMod
{
    public class OneTimeDemand
    {
        public static bool hasOneTimeDemand = false;
        public static bool inOneTimeDemand = false;
        public static int oneTimeItemId;
        public static int oneTimeCount;
        public static int oneTimeGid;
        public static int oneTimeIndex;
        public static int oneTimeSupplyGid;
        public static int oneTimeSupplyIndex;
        public static SupplyDemandPair[] remotePairs;
        public static int remotePairCount;
        public static int remotePairProcess;
        public static StationComponent oneTimeSupplyStation;

        public static void ResetOneTimeDemandState()
        {
            hasOneTimeDemand = false;
            inOneTimeDemand = false;
            oneTimeSupplyStation = null;
        }

        public static bool AddOneTimeDemand(StationComponent sc, int index)
        {
            if (inOneTimeDemand)
            {
                return true;
            }

            hasOneTimeDemand = false;

            if (!sc.isStellar)
            {
                return false;
            }
            StationStore ss = sc.storage[index];
            if (ss.remoteLogic == ELogisticStorage.Demand)
            {
                //return false;
            }

            int itemId = ss.itemId;
            int logisticShipCarries = GameMain.history.logisticShipCarries;
            int count = ss.remoteDemandCount;
            if (count <= 0)
            {
                return false;
            }
            if (count > logisticShipCarries)
            {
                count = logisticShipCarries;
            }

            int demandStarId = sc.planetId / 100;
            int nearestGid = -1;
            int nearestIndex = -1;
            float nearestValue = -1f;
            GalacticTransport galacticTransport = UIRoot.instance.uiGame.gameData.galacticTransport;
            for (int i = 0; i < galacticTransport.stationCursor; i++)
            {
                if (galacticTransport.stationPool[i] == null || sc.gid == i)
                {
                    continue;
                }
                int supplyIndex = galacticTransport.stationPool[i].HasRemoteSupply(itemId, count);
                if (supplyIndex >= 0 && galacticTransport.stationPool[i].idleShipCount > 0 && galacticTransport.stationPool[i].energy > 6000000L)
                {
                    int supplyStarId = galacticTransport.stationPool[i].planetId / 100;
                    float distance = LSTMStarDistance.StarDistance(demandStarId, supplyStarId);
                    if (distance < 0)
                    {
                        continue;
                    }
                    if (distance > 0.1 && galacticTransport.stationPool[i].warperCount <= 2)
                    {
                        continue;
                    }
                    if (nearestValue == -1f || distance < nearestValue)
                    {
                        nearestGid = i;
                        nearestIndex = supplyIndex;
                        nearestValue = distance;
                    }
                }
            }

            if (nearestGid > 0)
            {
                oneTimeItemId = itemId;
                oneTimeCount = count;
                oneTimeGid = sc.gid;
                oneTimeIndex = index;
                oneTimeSupplyGid = nearestGid;
                oneTimeSupplyIndex = nearestIndex;
                hasOneTimeDemand = true;
            }

            return hasOneTimeDemand;
            //ss.itemId;
        }

        public static bool PreOneTimeDemand(StationComponent sc)
        {
            if (sc.storage[oneTimeSupplyIndex].itemId != oneTimeItemId || sc.gid != oneTimeSupplyGid)
            {
                return false;
            }
            StationStore[] obj = sc.storage;
            lock (obj)
            {
                oneTimeSupplyStation = sc;
                remotePairs = sc.remotePairs;
                remotePairCount = sc.remotePairCount;
                remotePairProcess = sc.remotePairProcess;

                sc.remotePairProcess = 0;
                sc.remotePairs = new SupplyDemandPair[4];
                sc.remotePairCount = 0;
                sc.AddRemotePair(oneTimeSupplyGid, oneTimeSupplyIndex, oneTimeGid, oneTimeIndex);
            }
            return true;
        }

        public static void PostOneTimeDemand()
        {
            if (oneTimeSupplyStation != null)
            {
                oneTimeSupplyStation.remotePairs = remotePairs;
                oneTimeSupplyStation.remotePairCount = remotePairCount;
                oneTimeSupplyStation.remotePairProcess = remotePairProcess;
            }
            ResetOneTimeDemandState();
            //UIRoot.instance.uiGame.gameData.galacticTransport.RefreshTraffic(0);
        }
    }
}
