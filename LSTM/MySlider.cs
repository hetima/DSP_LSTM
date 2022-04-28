using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace LSTMMod
{
    public class MySlider : MonoBehaviour
    {
        public Slider slider;
        public RectTransform rectTrans;
        public Text labelText;
        public string labelFormat;
        public ConfigEntry<float> config;
        private bool _sliderEventLock;

        public static RectTransform CreateSlider(ConfigEntry<float> config, float minValue, float maxValue, string format = "{0}", float width = 0f)
        {
            void ResetAnchor(RectTransform rect_)
            {
                rect_.anchorMax = Vector2.zero;
                rect_.anchorMin = Vector2.zero;
                rect_.anchoredPosition3D = new Vector3(0, 0, 0);
            }

            UIOptionWindow optionWindow = UIRoot.instance.optionWindow;
            Slider src = optionWindow.audioVolumeComp;

            GameObject go = GameObject.Instantiate(src.gameObject);
            //sizeDelta = 240, 20
            go.name = "my-slider";
            go.SetActive(true);
            MySlider sl = go.AddComponent<MySlider>();
            sl.config = config;
            RectTransform rect = go.transform as RectTransform;
            sl.rectTrans = rect;
            ResetAnchor(rect);
            if (width > 0)
            {
                rect.sizeDelta = new Vector2(width, rect.sizeDelta.y);
            }

            sl.slider = go.GetComponent<Slider>();
            sl.slider.minValue = minValue;
            sl.slider.maxValue = maxValue;
            sl.slider.onValueChanged.RemoveAllListeners();
            sl.slider.onValueChanged.AddListener(new UnityAction<float>(sl.SliderChanged));
            sl.labelText = sl.slider?.handleRect?.Find("Text")?.GetComponent<Text>();
            if (sl.labelText != null)
            {
                sl.labelText.fontSize = 14;
                (sl.labelText.transform as RectTransform).sizeDelta = new Vector2(22f, 22f);
            }
            sl.labelFormat = format;

            Image bg = sl.slider.transform.Find("Background")?.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            }
            Image fill = sl.slider.fillRect.GetComponent<Image>();
            if (fill != null)
            {
                fill.color = new Color(1f, 1f, 1f, 0.28f);
            }
            sl.SettingChanged();
            sl.UpdateLabel(); //値が同じだったときテキストが更新されないので
            config.SettingChanged += (sender, args) => {
                sl.SettingChanged();
            };

            return sl.rectTrans;
        }
        public void SettingChanged()
        {
            if (!_sliderEventLock)
            {
                _sliderEventLock = true;
                if (config.Value != slider.value)
                {
                    float sliderVal = config.Value;
                    if (sliderVal > slider.maxValue)
                    {
                        sliderVal = slider.maxValue;
                    }
                    else if (sliderVal < slider.minValue)
                    {
                        sliderVal = slider.minValue;
                    }
                    slider.value = sliderVal;
                    UpdateLabel();
                }
                _sliderEventLock = false;
            }
        }
        public void UpdateLabel()
        {
            if (labelText != null)
            {
                labelText.text = config.Value.ToString(labelFormat);
            }
        }

        public void SliderChanged(float val)
        {
            if (!_sliderEventLock)
            {
                _sliderEventLock = true;
                float newVal = Mathf.Round(slider.value);
                config.Value = newVal;
                UpdateLabel();
                _sliderEventLock = false;
            }

        }

    }
}
