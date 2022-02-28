using NebulaAPI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Compatibility
{
    public static class NebulaCompat
    {
        public static bool IsMultiplayerActive = false;
        public static bool IsClient = false;
        public static Action OnReceiveData;

        public static void Init()
        {
            try
            {
                if (NebulaModAPI.NebulaIsInstalled)
                {
                    NebulaModAPI.OnMultiplayerGameStarted += MultiplayerStart;
                    NebulaModAPI.OnMultiplayerGameEnded += MultiplayerEnd;
                    NebulaModAPI.RegisterPackets(Assembly.GetExecutingAssembly());
                    LSTMMod.LSTM.Logger.LogInfo("Nebula compatibility ready");
                }
            }
            catch (Exception e)
            {
                LSTMMod.LSTM.Logger.LogError("Nebula compatibility failed!");
                LSTMMod.LSTM.Logger.LogError(e);
            }
        }

        public static void MultiplayerStart()
        {
            IsMultiplayerActive = NebulaModAPI.IsMultiplayerActive;
            IsClient = NebulaModAPI.IsMultiplayerActive && NebulaModAPI.MultiplayerSession.LocalPlayer.IsClient;
        }

        public static void MultiplayerEnd()
        {
            IsMultiplayerActive = false;
            IsClient = false;
        }

        public static void SendRequest()
        {
            // Request for host ILS storage data when client open UI or hit Global button 
            NebulaModAPI.MultiplayerSession.Network.SendPacket(new LSTMRequest());
        }
    }
    
    internal class LSTMRequest
    {
        public LSTMRequest() { }
    }
    internal class LSTMResponse
    {
        public int[] StationGId { get; set; }
        public int[] StorageLength { get; set; }

        public int[] ItemId { get; set; }
        public int[] Max { get; set; }
        public int[] Count { get; set; }
        public int[] Inc { get; set; }
        public int[] RemoteOrder { get; set; }
        public byte[] Logic { get; set; }

        public LSTMResponse() { }
        public LSTMResponse(in StationComponent[] gStationPool)
        {
            List<int> stationGId = new List<int>();
            List<int> storageLength = new List<int>();
            int arraySize = 0;
            int offset = 0;

            foreach (StationComponent stationComponent in gStationPool)
            {
                if (stationComponent != null)
                {
                    stationGId.Add(stationComponent.gid);
                    storageLength.Add(stationComponent.storage.Length);
                    arraySize += stationComponent.storage.Length;
                }
            }

            StationGId = stationGId.ToArray();
            StorageLength = storageLength.ToArray();
            ItemId = new int[arraySize];
            Max = new int[arraySize];
            Count = new int[arraySize];
            Inc = new int[arraySize];
            RemoteOrder = new int[arraySize];
            Logic = new byte[arraySize];

            for (int i = 0; i < stationGId.Count; i++)
            {
                StationComponent station = gStationPool[stationGId[i]];
                for (int j = 0; j < storageLength[i]; j++)
                {
                    StationStore stationStore = station.storage[j];
                    ItemId[offset + j] = stationStore.itemId;
                    Max[offset + j] = stationStore.max;
                    Count[offset + j] = stationStore.count;
                    Inc[offset + j] = stationStore.inc;
                    RemoteOrder[offset + j] = stationStore.remoteOrder;
                    Logic[offset + j] = (byte)stationStore.remoteLogic;
                }
                offset += storageLength[i];
            }
        }
    }

    [RegisterPacketProcessor]
    internal class LSTMRequestProcessor : BasePacketProcessor<LSTMRequest>
    {
        public override void ProcessPacket(LSTMRequest packet, INebulaConnection conn)
        {
            if (IsHost)
                conn.SendPacket(new LSTMResponse(GameMain.data.galacticTransport.stationPool));
        }
    }

    [RegisterPacketProcessor]
    internal class PauseNotificationProcessor : BasePacketProcessor<LSTMResponse>
    {
        public override void ProcessPacket(LSTMResponse packet, INebulaConnection conn)
        {
            if (IsHost)
                return;

            int offset = 0;
            StationComponent[] gStationPool = GameMain.data.galacticTransport.stationPool;
            for (int i = 0; i < packet.StationGId.Length; i++)
            {
                StationComponent station = gStationPool[packet.StationGId[i]];
                if (station == null)
                {
                    LSTMMod.LSTM.Logger.LogWarning($"Gid {packet.StationGId[i]} does not in client");
                    continue;
                }
                if (station.storage == null)
                    station.storage = new StationStore[packet.StorageLength[i]];

                for (int j = 0; j < packet.StorageLength[i]; j++)
                {
                    station.storage[j].itemId = packet.ItemId[offset + j];
                    station.storage[j].max = packet.Max[offset + j];
                    station.storage[j].count = packet.Count[offset + j] ;
                    station.storage[j].inc = packet.Inc[offset + j];
                    station.storage[j].remoteOrder = packet.RemoteOrder[offset + j];
                    station.storage[j].remoteLogic = (ELogisticStorage)packet.Logic[offset + j];
                }
                offset += packet.StorageLength[i];
            }

            foreach(var station in gStationPool)
            {
                if (station != null && station.storage == null)
                {
                    LSTMMod.LSTM.Logger.LogWarning($"Gid {station.gid} does not in server");
                }
            }
            // Refresh UI
            NebulaCompat.OnReceiveData?.Invoke();
        }
    }
}