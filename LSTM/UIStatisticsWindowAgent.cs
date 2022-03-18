//using System;
//using System.Text;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LSTMMod
{

    public class UIStatisticsWindowAgent : MonoBehaviour
    {
        public UIStatisticsWindow window;
        public UIProductEntryAgent[] entries;
        public int entriesLen;
        public UIButton balanceBtn;

        public void ShowBalance()
        {
            for (int i = 0; i < entriesLen; i++)
            {
                entries[i].ShowBalance();
            }
            if (balanceBtn != null)
            {
                balanceBtn.highlighted = true;
            }
        }

        public void HideBalance()
        {
            for (int i = 0; i < entriesLen; i++)
            {
                entries[i].HideBalance();
            }
            if(balanceBtn != null)
            {
                balanceBtn.highlighted = false;
            }
        }

        public void OnBalanceBtnClick(int obj)
        {
            if (balanceBtn.highlighted)
            {
                HideBalance();
            }
            else
            {
                ShowBalance();
            }
        }

        public static void PreCreate()
        {
            //before UIStatisticsWindow _OnCreate()
            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;

            //showBalanceButton
            UIProductEntry productEntry = statisticsWindow.productEntry;
            UIButton btn = Util.MakeSmallTextButton("LSTM", 38f, 20f);
            RectTransform rect = Util.NormalizeRectD(btn.gameObject);
            rect.SetParent(productEntry.transform, false);
            rect.anchoredPosition = new Vector3(6f, -6f);
            rect.localScale = Vector3.one;
            btn.gameObject.SetActive(false);

            UIProductEntryAgent p = productEntry.gameObject.AddComponent<UIProductEntryAgent>();
            p.productEntry = productEntry;
            p.showBalanceButton = btn.button;
            p.graphTrans = productEntry.statGraph.GetComponent<RectTransform>();
            p.graphTransDefaultWidth = p.graphTrans.sizeDelta.x;

            //for OnPointerEnter OnPointerExit
            Image img = productEntry.gameObject.AddComponent<Image>();
            img.color = Color.clear;
            img.alphaHitTestMinimumThreshold = 0f;

            //UIStatBalance
            p.stationBalance = UIStatBalance.CreatePreｆab();
            //p.stationBalance = GameObject.Instantiate<UIStatBalance>(prefab);

            p.stationBalance.name = "station-balance";
            p.stationBalance.transform.SetParent(productEntry.transform, false);
            //p.demandBalance.leftBar.color = p.demandBalance.demandColor;
            (p.stationBalance.gameObject.transform as RectTransform).anchoredPosition = new Vector2(458, 0);
            p.stationBalance.gameObject.SetActive(true);

            //test
            p.graphTrans.sizeDelta = new Vector2(p.graphTrans.sizeDelta.x - 60, p.graphTrans.sizeDelta.y);

        }

        public static void PostCreate()
        {
            //entries
            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
            UIStatisticsWindowAgent agent = statisticsWindow.gameObject.AddComponent<UIStatisticsWindowAgent>();
            agent.window = statisticsWindow;
            agent.entriesLen = AccessTools.FieldRefAccess<UIStatisticsWindow, int>(statisticsWindow, "entriesLen");
            agent.entries = new UIProductEntryAgent[agent.entriesLen];

            UIProductEntry[] ary = AccessTools.FieldRefAccess<UIStatisticsWindow, UIProductEntry[]>(statisticsWindow, "entries");
            for (int i = 0; i < agent.entriesLen; i++)
            {
                agent.entries[i] = ary[i]?.gameObject.GetComponent<UIProductEntryAgent>();
            }


            //balanceBtn
            UIBuildMenu buildMenu = UIRoot.instance.uiGame.buildMenu;
            UIButton src = buildMenu.categoryButtons[6];
            UIButton btn = GameObject.Instantiate<UIButton>(src, statisticsWindow.productPanel.transform);
            btn.button.interactable = true;
            GameObject go = btn.transform.Find("text")?.gameObject;
            if (go != null)
            {
                go.SetActive(false); //GameObject.Destroy(go);
            }
            //onClick が m_PersistentCalls に残ってるので作り直す
            Button oldbutton = btn.button;
            GameObject.DestroyImmediate(oldbutton);
            Button button = btn.gameObject.AddComponent<Button>();
            btn.button = button;
            btn.gameObject.SetActive(true);

            btn.onClick += agent.OnBalanceBtnClick;
            btn.onRightClick += agent.OnBalanceBtnClick;
            btn.tips.delay = 0.8f;
            btn.tips.tipTitle = "ILS Stock Percentage".Translate();
            btn.tips.tipText = "Click:show / hide ILS Stock Percentage\nRight click:".Translate();
            btn.tips.corner = 3;
            btn.tips.offset = new Vector2(6, 38);
            RectTransform rect = btn.transform as RectTransform;
            rect.anchorMax = new Vector2(0f, 1f);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(458f, -51f);
            rect.sizeDelta = new Vector2(36, 36);
            RectTransform iconRect = rect.Find("icon")?.transform as RectTransform;
            if (iconRect != null)
            {
                iconRect.sizeDelta = new Vector2(20, 20);
            }
            
            agent.balanceBtn = btn;
            agent.HideBalance();
        }
    }

    public class UIProductEntryAgent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        public UIProductEntry productEntry;
        [SerializeField]
        public Button showBalanceButton;

        [SerializeField]
        public UIStatBalance stationBalance;

        [SerializeField]
        public RectTransform graphTrans;
        [SerializeField]
        public float graphTransDefaultWidth;

        void Start()
        {
            showBalanceButton?.onClick.AddListener(ShowBalanceButtonClicked);
        }

        public void ShowBalanceButtonClicked()
        {
            int itemId = productEntry.entryData.itemId;
            if (itemId > 0)
            {
                //VFAudio.Create("ui-click-0", null, Vector3.zero, true, 2);
                LSTM.OpenBalanceWindow(itemId);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (LSTM.showButtonInStatisticsWindow.Value)
            {
                showBalanceButton.gameObject.SetActive(true);
            }
        }
        public void OnPointerExit(PointerEventData _eventData)
        {
            showBalanceButton.gameObject.SetActive(false);
        }

        public void ShowBalance()
        {
            stationBalance.gameObject.SetActive(true);
            graphTrans.sizeDelta = new Vector2(graphTransDefaultWidth - 60, graphTrans.sizeDelta.y);

        }
        public void HideBalance()
        {
            stationBalance.gameObject.SetActive(false);
            graphTrans.sizeDelta = new Vector2(graphTransDefaultWidth, graphTrans.sizeDelta.y);

        }

    }

    public class UIStatBalance : MonoBehaviour
    {
        [SerializeField]
        public Text demandText;

        [SerializeField]
        public Text supplyText;

        public void SetDemandValue(float val)
        {
            if (val < 0)
            {
                demandText.text = "--";
            }
            else
            {
                demandText.text = string.Format("{0:F1}<size=18> %</size>", val);
            }
        }

        public void SetSupplyValue(float val)
        {
            if (val < 0)
            {
                supplyText.text = "--";
            }
            else
            {
                supplyText.text = string.Format("{0:F1}<size=18> %</size>", val);
            }
        }

        public static UIStatBalance CreatePreｆab()
        {
            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
            UIProductEntry productEntry = statisticsWindow.productEntry;
            float height = productEntry.rectTransform.sizeDelta.y;

            GameObject go = new GameObject("station-balance-prefab");
            go.AddComponent<RectTransform>();
            RectTransform rect = Util.NormalizeRect(go);
            rect.sizeDelta = new Vector2(100, height);

            UIStatBalance prefab = go.AddComponent<UIStatBalance>();

            prefab.demandText = Util.MakeGameObject<Text>(go.transform, productEntry.consumeText.gameObject,  0, 0, 90, 40, false, true);
            prefab.supplyText = Util.MakeGameObject<Text>(go.transform, productEntry.productText.gameObject,  0, 60, 90, 40, false, true);
            prefab.demandText.alignment = TextAnchor.MiddleLeft;
            prefab.demandText.resizeTextForBestFit = true;
            prefab.demandText.resizeTextMaxSize = 34;
            prefab.supplyText.alignment = TextAnchor.MiddleLeft;
            prefab.supplyText.resizeTextForBestFit = true;
            prefab.supplyText.resizeTextMaxSize = 34;

            Text txt = Util.MakeGameObject<Text>(go.transform, productEntry.consumeLabel.gameObject, 0, 28, 0, 0, false, true); 
            txt.text = "Demand Storage";
            txt = Util.MakeGameObject<Text>(go.transform, productEntry.productLabel.gameObject, 0, 88, 0, 0, false, true);
            txt.text = "Supply Storage";

            prefab.demandText.supportRichText = true;
            prefab.supplyText.supportRichText = true;
            prefab.demandText.text = "99.9<size=18> %</size>";
            prefab.supplyText.text = "99.9<size=18> %</size>";

            return prefab;
        }

    }
}
