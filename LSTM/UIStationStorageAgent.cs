//using System;
//using System.Text;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static System.Collections.Specialized.BitVector32;

namespace LSTMMod
{
    public class UIStationStorageAgent : MonoBehaviour
    {
        public static List<UIStationStorageAgent> agents = new List<UIStationStorageAgent>();
        public UIStationStorage uiStorage;
        public Button oneShotBtn = null;


        [SerializeField]
        public UIButton localBtn;
        [SerializeField]
        public UIButton remoteBtn;

        void Start()
        {
            if(localBtn != null) localBtn.onClick += OpenLocalBalance;
            if (remoteBtn != null) remoteBtn.onClick += OpenRemoteBalance;
        }

        public void RefreshValues()
        {
            if (!LSTM.showButtonInStationWindow.Value || uiStorage.station == null || uiStorage.index >= uiStorage.station.storage.Length || uiStorage.station.storage[uiStorage.index].itemId <= 0 || uiStorage.popupBoxRect.gameObject.activeSelf)
            {
                localBtn.gameObject.SetActive(false);
                remoteBtn.gameObject.SetActive(false);
                return;
            }

            if (uiStorage.station.isCollector)
            {
                localBtn.gameObject.SetActive(false);
            }
            else
            {
                localBtn.gameObject.SetActive(true);
            }

            if (uiStorage.station.isStellar || uiStorage.station.isCollector)
            {
                remoteBtn.gameObject.SetActive(true);
            }
            else
            {
                remoteBtn.gameObject.SetActive(false);
            }
        }


        public void OpenLocalBalance(int obj)
        {
            //VFAudio.Create("ui-click-0", null, Vector3.zero, true, 2);
            OpenBalance(true);
        }
        public void OpenRemoteBalance(int obj)
        {
            //VFAudio.Create("ui-click-0", null, Vector3.zero, true, 2);
            OpenBalance(false);
        }

        public void OpenBalance(bool isLocal)
        {
            StationComponent cmp = uiStorage.station;
            if (cmp == null)
            {
                return;
            }
            int index = uiStorage.index;
            UIStationWindow stationWindow = UIRoot.instance.uiGame.stationWindow;
            
            LSTM.OpenBalanceWindow(cmp, index, isLocal, stationWindow.factory);
        }


        private void OneShotBtnClick()
        {
            uiStorage.popupBoxRect.gameObject.SetActive(false);
            OneTimeDemand.AddOneTimeDemand(uiStorage.station, uiStorage.index);

        }

        private void MakeOneShotBtn()
        {
            oneShotBtn = GameObject.Instantiate<Button>(uiStorage.optionButton0, uiStorage.optionButton0.transform.parent);
            RectTransform rect = oneShotBtn.gameObject.transform as RectTransform;
            rect.localPosition = new Vector2(rect.localPosition.x, rect.localPosition.y - 18);
            rect.Find("button-text").GetComponent<Text>().text = "one-time Dmd";
            rect.GetComponent<Image>().color = new Color(0.8f, 0.3f, 0f, 1f);
            oneShotBtn.onClick.RemoveAllListeners();
            oneShotBtn.onClick.AddListener(new UnityAction(this.OneShotBtnClick));
            oneShotBtn.gameObject.SetActive(false);
        }

        private void ShowOneShotBtn()
        {
            if (oneShotBtn == null)
            {
                MakeOneShotBtn();
            }
            if (!oneShotBtn.gameObject.activeSelf)
            {
                RectTransform rectTransform = uiStorage.popupBoxRect;
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y + 30f);
                oneShotBtn.gameObject.SetActive(true);

                RectTransform rect = (uiStorage.optionButton0.gameObject.transform as RectTransform);
                rect.localPosition = new Vector2(rect.localPosition.x, rect.localPosition.y + 20);
                rect = (uiStorage.optionButton1.gameObject.transform as RectTransform);
                rect.localPosition = new Vector2(rect.localPosition.x, rect.localPosition.y + 20);
            }

        }
        private void HideOneShotBtn()
        {
            if (oneShotBtn != null && oneShotBtn.gameObject.activeSelf)
            {
                RectTransform rectTransform = uiStorage.popupBoxRect;
                rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.y - 30f);
                oneShotBtn.gameObject.SetActive(false);

                RectTransform rect = (uiStorage.optionButton0.gameObject.transform as RectTransform);
                rect.localPosition = new Vector2(rect.localPosition.x, rect.localPosition.y - 20);
                rect = (uiStorage.optionButton1.gameObject.transform as RectTransform);
                rect.localPosition = new Vector2(rect.localPosition.x, rect.localPosition.y - 20);
            }

        }
        public void OnRemoteSdButtonClick()
        {
            StationComponent cmp = uiStorage.station;
            if (cmp.storage[uiStorage.index].remoteDemandCount > 0)
            {
                RectTransform rectTransform = uiStorage.popupBoxRect;
                ShowOneShotBtn();
            }
            else
            {
                HideOneShotBtn();
            }
        }
        public void OnLocalSdButtonClick()
        {
            HideOneShotBtn();
        }

        public static UIStationStorageAgent MakeUIStationStorageAgent(UIStationStorage stationStorage)
        {
            GameObject parent = stationStorage.gameObject;
            GameObject go = new GameObject("lstm-open-barance-button");

            UIStationStorageAgent agent = parent.AddComponent<UIStationStorageAgent>();
            go.transform.parent = parent.transform;
            go.transform.localPosition = new Vector3(523, -60, 0);
            go.transform.localScale = new Vector3(1, 1, 1);
            //RectTransform rect = (RectTransform)go.transform;
            //rect.sizeDelta = new Vector2(16, 32);
            go.transform.SetAsFirstSibling();

            Sprite s = Util.LoadSpriteResource("ui/textures/sprites/icons/resume-icon");
            agent.remoteBtn = Util.MakeIconButton(go.transform, s, 0, 0);
            agent.localBtn = Util.MakeIconButton(go.transform, s, 0, 32);

            if (agent.localBtn != null && agent.remoteBtn != null)
            {

                agent.localBtn.gameObject.name = "lstm-open-barance-local";
                agent.remoteBtn.gameObject.name = "lstm-open-barance-remote";
                //btn.uiBtn.gameObject.transform.Find("bg").gameObject.SetActive(false); //or destroy
                //btn.uiBtn.gameObject.transform.Find("sd").gameObject.SetActive(false);
                agent.uiStorage = stationStorage;
            }
            else
            {
                LSTM.Log("UIStationStorageAgent is null");
            }
            agents.Add(agent);
            return agent;
        }

        public static class Patch
        {
            [HarmonyPostfix, HarmonyPatch(typeof(UIStationStorage), "RefreshValues")]
            public static void UIStationStorage_RefreshValues_Postfix(UIStationStorage __instance)
            {
                if (LSTM.showButtonInStationWindow.Value)
                {
                    __instance.GetComponent<UIStationStorageAgent>()?.RefreshValues();
                }
            }

            [HarmonyPostfix, HarmonyPatch(typeof(UIStationStorage), "OnRemoteSdButtonClick")]
            public static void UIStationStorage_OnRemoteSdButtonClick_Postfix(UIStationStorage __instance)
            {
                if (LSTM.showButtonInStationWindow.Value)
                {
                    __instance.GetComponent<UIStationStorageAgent>()?.OnRemoteSdButtonClick();
                }
            }
            [HarmonyPostfix, HarmonyPatch(typeof(UIStationStorage), "OnLocalSdButtonClick")]
            public static void UIStationStorage_OnLocalSdButtonClick_Postfix(UIStationStorage __instance)
            {
                if (LSTM.showButtonInStationWindow.Value)
                {
                    __instance.GetComponent<UIStationStorageAgent>()?.OnLocalSdButtonClick();
                }
            }
        }
    }
}
