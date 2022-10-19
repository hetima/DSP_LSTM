

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

        public static void ResetOneTimeDemandState()
        {
            hasOneTimeDemand = false;
            inOneTimeDemand = false;
        }

        public static bool AddOneTimeDemand(StationComponent sc, int index)
        {
            if (inOneTimeDemand)
            {
                return false;
            }

            hasOneTimeDemand = false;

            if (!sc.isStellar)
            {
                return false;
            }
            StationStore ss = sc.storage[index];
            if (ss.remoteLogic == ELogisticStorage.Demand)
            {
                return false;
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
            GalacticTransport galacticTransport = UIRoot.instance.uiGame.gameData.galacticTransport;
            for (int i = 0; i < galacticTransport.stationCursor; i++)
            {
                if (galacticTransport.stationPool[i] == null || sc.gid == i)
                {
                    continue;
                }
                oneTimeSupplyIndex = galacticTransport.stationPool[i].HasRemoteSupply(itemId, count);
                if (oneTimeSupplyIndex >= 0 && galacticTransport.stationPool[i].idleShipCount > 0 && galacticTransport.stationPool[i].energy > 6000000L)
                {
                    oneTimeItemId = itemId;
                    oneTimeCount = count;
                    oneTimeGid = sc.gid;
                    oneTimeIndex = index;
                    oneTimeSupplyGid = i;
                    hasOneTimeDemand = true;
                    break;
                }
            }

            return hasOneTimeDemand;
            //ss.itemId;
        }

        public static bool PrepareCallOneTimeDemand(StationComponent sc)
        {
            if (sc.storage[oneTimeSupplyIndex].itemId != oneTimeItemId || sc.gid != oneTimeSupplyGid)
            {
                return false;
            }
            StationStore[] obj = sc.storage;
            lock (obj)
            {
                sc.ClearRemotePairs();
                sc.remotePairProcess = 0;
                sc.AddRemotePair(oneTimeSupplyGid, oneTimeSupplyIndex, oneTimeGid, oneTimeIndex);
            }
            return true;
        }

        public static void ResetOneTimeDemandTraffic()
        {
            ResetOneTimeDemandState();
            UIRoot.instance.uiGame.gameData.galacticTransport.RefreshTraffic(0);
        }
    }
}
