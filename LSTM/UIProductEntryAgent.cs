//using System;
//using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace LSTMMod
{
    public class UIProductEntryAgent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        public UIProductEntry productEntry;
        [SerializeField]
        public Button showBalanceButton;

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

        public static void PreCreate()
        {
            //before UIStatisticsWindow _OnCreate()

            //showBalanceButton
            UIStatisticsWindow statisticsWindow = UIRoot.instance.uiGame.statWindow;
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
            //for OnPointerEnter OnPointerExit
            Image img = productEntry.gameObject.AddComponent<Image>();
            img.color = Color.clear;
            img.alphaHitTestMinimumThreshold = 0f;

        }
        public static void PostCreate()
        {

        }
    }
}
