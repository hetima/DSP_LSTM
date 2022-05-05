using System;
//using System.Text;
using System.Collections.Generic;
using UnityEngine;


namespace LSTMMod
{
    public class StationSignRenderer
    {
        public PlanetFactory factory;
        public bool showStationInfo;
        private int itemId;

        private Material entitySignMat;
        public SignData[] entitySignPool;
        public ComputeBuffer entitySignBuffer { get; private set; }
        private int entitySignBufferLen;
        private int bufferLen;
        private static int defaultBufferLenSize = 300;
        private static int maxBufferLenSize = 4000;

        public void Init()
        {
            entitySignMat = new Material(Configs.builtin.entitySignMat);
            SetBufferSize(defaultBufferLenSize);
        }

        private void SetBufferSize(int newSize)
        {
            if (entitySignPool != null)
            {
                entitySignPool = null;
            }
            entitySignPool = new SignData[newSize];
            FreeEntitySignBuffer();
            entitySignBuffer = new ComputeBuffer(newSize, 56, ComputeBufferType.Default);
            entitySignBufferLen = newSize;
            bufferLen = newSize;
        }

        private void CheckBufferSize()
        {
            if (needMoreBuffer)
            {
                if (bufferLen < maxBufferLenSize)
                {
                    SetBufferSize(bufferLen + defaultBufferLenSize);
                }
                needMoreBuffer = false;
            }
        }

        public void Free()
        {
            entitySignPool = null;
            FreeEntitySignBuffer();
        }

        public void FreeEntitySignBuffer()
        {
            if (entitySignBuffer != null)
            {
                entitySignBufferLen = 0;
                entitySignBuffer.Release();
                entitySignBuffer = null;
            }
        }

        private float lastZoom = 0f;
        private int lastFrame = 0;
        private bool needMoreBuffer = false;

        public void Refresh(PlanetFactory currentFactory)
        {
            if (Time.frameCount < lastFrame + 2)
            {
                return;
            }
            float zoom = 1f;
            if (UIGame.viewMode == EViewMode.Globe)
            {
                PlanetPoser planetPoser = GameCamera.instance.planetPoser;
                float zoomOut = planetPoser.dist / (planetPoser.planetRadius * 1.15f);
                if (zoomOut > 1f)
                {
                    zoom = zoomOut;
                }
            }

            if (!needMoreBuffer && lastZoom == zoom && currentFactory == factory && Time.frameCount < lastFrame + 60 && showStationInfo == LSTM.showStationInfo.Value)
            {
                return;
            }

            CheckBufferSize();
            lastFrame = Time.frameCount;
            showStationInfo = LSTM.showStationInfo.Value;
            itemId = 0;//.itemId;

            //Cleanup();

            lastZoom = zoom;
            factory = currentFactory;
            if (factory == null)
            {
                return;
            }
            int entityCursor = 1;
            //int itemsPerStep = 5;

            if (showStationInfo || itemId > 0)
            {
                int targetItemId = itemId;

                //station
                for (int i = 1; i < factory.transport.stationCursor; i++)
                {
                    StationComponent cmp = factory.transport.stationPool[i];
                    if (cmp == null || cmp.id != i || cmp.isVeinCollector)
                    {
                        continue;
                    }

                    Vector3 pos = factory.entityPool[cmp.entityId].pos;
                    float distFactor = 1f + (zoom - 1f) / 4f;
                    int storageLength = cmp.storage.Length;
                    bool hasEmpty = false;
                    int emptyCount = 0;

                    if (cmp.storage.Length == 1) //VeinCollector 最初のifで弾いているので不要だけど
                    {
                        distFactor = 0f;
                    }
                    else
                    {
                        for (int j = 0; j < cmp.storage.Length; j++)
                        {
                            if (showStationInfo && cmp.storage[j].itemId <= 0)
                            {
                                storageLength--;
                                hasEmpty = true;
                            }
                        }
                        if (hasEmpty)
                        {
                            storageLength++;
                        }
                    }

                    int emptySlotCursor = -1;
                    for (int j = 0; j < cmp.storage.Length; j++)
                    {
                        if (showStationInfo && cmp.storage[j].itemId <= 0)
                        {
                            emptyCount++;
                            if (emptySlotCursor > 0 || cmp.isCollector)
                            {
                                continue;
                            }
                            emptySlotCursor = entityCursor;
                        }
                        else if (!showStationInfo && cmp.storage[j].itemId != itemId)
                        {
                            continue;
                        }

                        int slotNum = j;
                        if (emptySlotCursor > 0)
                        {
                            slotNum -= (emptyCount - 1);
                        }
                        Quaternion shipDiskRot = Quaternion.Euler(0f, 360f / (float)storageLength * slotNum, 0f); //角度
                        Vector3 shipDiskPos = shipDiskRot * new Vector3(0f, 0f, cmp.isStellar ? 11.5f * distFactor : 5f * distFactor); //中心からの距離
                        shipDiskPos = factory.entityPool[cmp.entityId].pos + cmp.shipDockRot * shipDiskPos;
                        float height = cmp.isStellar ? 26f + (10 * zoom) : 16.5f + (10 * zoom);
                        float size = cmp.isStellar ? 10f * zoom : 6f * zoom;
                        //int step = j / itemsPerStep;
                        //height += ((size / 1.5f) * (float)step);
                        entitySignPool[entityCursor].Reset(shipDiskPos, height, size);
                        
                        entitySignPool[entityCursor].iconId0 = (uint)cmp.storage[j].itemId;
                        entitySignPool[entityCursor].iconType = 1U;
                        //0だと描画されない
                        entitySignPool[entityCursor].count0 = (float)(cmp.storage[j].count >= 1 ? cmp.storage[j].count : 0.4f);

                        entityCursor++;
                        if (entityCursor >= bufferLen)
                        {
                            needMoreBuffer = true;
                            break;
                        }
                    }
                    //空きスロット数
                    if (showStationInfo && emptySlotCursor > 0 && !cmp.isCollector)
                    {
                        if (emptyCount > 9)
                        {
                            emptyCount = 9;
                        }
                        entitySignPool[emptySlotCursor].iconId0 = (uint)emptyCount + 600U;
                        entitySignPool[emptySlotCursor].iconType = 4U;
                        entitySignPool[emptySlotCursor].count0 = 0f;
                    }
                    if (entityCursor >= bufferLen)
                    {
                        needMoreBuffer = true;
                        break;
                    }

                }
            }


            if (entityCursor >= bufferLen)
            {
                needMoreBuffer = true;
            }
            else
            {
                //remain empty slot
                for (; entityCursor < bufferLen; entityCursor++)
                {
                    if (entitySignPool[entityCursor].iconId0 == 0)
                    {
                        break;
                    }
                    entitySignPool[entityCursor].SetEmpty();
                }
            }

            entitySignBuffer.SetData(entitySignPool);

        }

        public void Draw(PlanetFactory currentFactory)
        {
            if (!EntitySignRenderer.showIcon || !LSTM.showStationInfo.Value)
            {
                return;
            }
            if (UIGame.viewMode != EViewMode.Globe && LSTM.showStationInfoOnlyInPlanetView.Value)
            {
                return;
            }
            Refresh(currentFactory);
            if (entitySignBufferLen > 1)
            {
                Shader.SetGlobalFloat("_Global_ShowEntitySign", EntitySignRenderer.showSign ? 1f : 0f);
                Shader.SetGlobalFloat("_Global_ShowEntityIcon", 1f);
                Shader.SetGlobalInt("_EntitySignMask", EntitySignRenderer.buildingWarnMask);
                entitySignMat.SetBuffer("_SignBuffer", entitySignBuffer);
                entitySignMat.SetPass(0);
                Graphics.DrawProcedural(MeshTopology.Quads, 8 * entitySignBufferLen, 1);
            }
        }

        private void Cleanup()
        {
            
        }
    }
}
