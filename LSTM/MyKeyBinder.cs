using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;


namespace LSTMMod
{

    public class MyKeyBinder : MonoBehaviour
    {
        public ConfigEntry<KeyboardShortcut> config;

        [SerializeField]
        public Text functionText;

        [SerializeField]
        public Text keyText;

        [SerializeField]
        public InputField setTheKeyInput;

        [SerializeField]
        public Toggle setTheKeyToggle;

        [SerializeField]
        public RectTransform rectTrans;

        [SerializeField]
        public UIButton inputUIButton;

        [SerializeField]
        public Text conflictText;

        [SerializeField]
        public Text waitingText;

        [SerializeField]
        public UIButton setDefaultUIButton;

        [SerializeField]
        public UIButton setNoneKeyUIButton;

        private bool nextNotOn;

        public static RectTransform CreateKeyBinder(ConfigEntry<KeyboardShortcut> config, string label = "")
        {
            void ResetAnchor(RectTransform rect_)
            {
                rect_.anchorMax = Vector2.zero;
                rect_.anchorMin = Vector2.zero;
            }

            UIOptionWindow optionWindow = UIRoot.instance.optionWindow;
            UIKeyEntry uikeyEntry = GameObject.Instantiate<UIKeyEntry>(optionWindow.entryPrefab);
            uikeyEntry.gameObject.SetActive(true);

            GameObject go = uikeyEntry.gameObject;
            go.name = "my-keybinder";
            MyKeyBinder kb = go.AddComponent<MyKeyBinder>();
            kb.config = config;

            kb.functionText = uikeyEntry.functionText;
            kb.keyText = uikeyEntry.keyText;
            kb.setTheKeyInput = uikeyEntry.setTheKeyInput;
            kb.setTheKeyToggle = uikeyEntry.setTheKeyToggle;
            kb.rectTrans = uikeyEntry.rectTrans;
            kb.inputUIButton = uikeyEntry.inputUIButton;
            kb.conflictText = uikeyEntry.conflictText;
            kb.waitingText = uikeyEntry.waitingText;
            kb.setDefaultUIButton = uikeyEntry.setDefaultUIButton;
            kb.setNoneKeyUIButton = uikeyEntry.setNoneKeyUIButton;


            kb.functionText.text = label;
            kb.functionText.fontSize = 17;

            (kb.keyText.transform as RectTransform).anchoredPosition = new Vector2(20f, -22f);
            //kb.keyText.alignment = TextAnchor.MiddleRight;
            kb.keyText.fontSize = 17;
            (kb.inputUIButton.transform.parent.transform as RectTransform).anchoredPosition = new Vector2(0f + 20f, -52f);
            (kb.setDefaultUIButton.transform as RectTransform).anchoredPosition = new Vector2(140f + 20f, -52f);
            (kb.setNoneKeyUIButton.transform as RectTransform).anchoredPosition = new Vector2(240f + 20f, -52f);
            RectTransform rect = go.transform as RectTransform;
            ResetAnchor(rect);
            //rect.sizeDelta = new Vector2(240f, 64f);
            GameObject.Destroy(uikeyEntry);
            kb.setNoneKeyUIButton.gameObject.SetActive(false);

            kb.SettingChanged();
            config.SettingChanged += (sender, args) => {
                kb.SettingChanged();
            };
            kb.inputUIButton.onClick += kb.OnInputUIButtonClick;
            kb.setDefaultUIButton.onClick += kb.OnSetDefaultKeyClick;
            //kb.setNoneKeyUIButton.onClick += kb.OnSetNoneKeyClick;
            return rect;
        }

        private void Update()
        {
            if (!setTheKeyToggle.isOn && inputUIButton.highlighted)
            {
                setTheKeyToggle.isOn = true;
            }
            if (setTheKeyToggle.isOn)
            {
                if (!inputUIButton._isPointerEnter && Input.GetKeyDown(KeyCode.Mouse0))
                {
                    inputUIButton.highlighted = false;
                    setTheKeyToggle.isOn = false;
                    Reset();
                }
                else if (!this.inputUIButton.highlighted)
                {
                    setTheKeyToggle.isOn = false;
                    Reset();
                }
                else
                {
                    waitingText.gameObject.SetActive(true);                    
                    if (TrySetValue())
                    {
                        setTheKeyToggle.isOn = false;
                        inputUIButton.highlighted = false;
                        Reset();
                    }
                }
            }
        }

        
        public bool TrySetValue()
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                VFInput.UseEscape();
                return true;
            }
            if (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1))
            {
                return true;
            }
            bool anyKey = GetIunptKeys();
            if (!anyKey && _lastKey != KeyCode.None)
            {
                string k = GetPressedKey();
                if (string.IsNullOrEmpty(k))
                {
                    return false;
                }
                _lastKey = KeyCode.None;

                config.Value = KeyboardShortcut.Deserialize(k);
                //keyText.text = k;
                return true;
            }

            return false;
        }

        private KeyCode _lastKey;
        private static KeyCode[] modKeys = { KeyCode.RightShift, KeyCode.LeftShift,
                 KeyCode.RightControl, KeyCode.LeftControl,
                 KeyCode.RightAlt, KeyCode.LeftAlt,
                 KeyCode.LeftCommand,  KeyCode.LeftApple, KeyCode.LeftWindows,
                 KeyCode.RightCommand,  KeyCode.RightApple, KeyCode.RightWindows };

        // _lastKey と現在押されているモデファイキーを組み合わせてショートカットを作成
        public string GetPressedKey()
        {
            string key = _lastKey.ToString();
            if (string.IsNullOrEmpty(key))
            {
                return null;
            }
            string mod = "";
            foreach (var modKey in modKeys)
            {
                if (Input.GetKey(modKey))
                {
                    mod += "+" + modKey.ToString();
                }
            }

            if (!string.IsNullOrEmpty(mod))
            {
                key += mod;
            }
            return key;
        }

        //通常キーが押されているかチェック _lastKey に保存
        public bool GetIunptKeys()
        {
            bool anyKey = false;

            List<KeyCode> keys = new List<KeyCode>();
            foreach (KeyCode item in Enum.GetValues(typeof(KeyCode)))
            {
                if (item != KeyCode.None && !modKeys.Contains(item) && Input.GetKey(item))
                {
                    _lastKey = item;
                    anyKey = true;
                }
            }
            return anyKey;

        }

        public void Reset()
        {
            conflictText.gameObject.SetActive(false);
            waitingText.gameObject.SetActive(false);
            setDefaultUIButton.button.Select(); // InputFieldのフォーカス外す
            _lastKey = KeyCode.None;
        }

        public void OnInputUIButtonClick(int data)
        {
            inputUIButton.highlighted = true;
            
            if (nextNotOn) //よくわからんけどたぶん不要
            {
                nextNotOn = false;
                inputUIButton.highlighted = false;
                setTheKeyToggle.isOn = false;
                waitingText.gameObject.SetActive(false);
            }
        }

        public void OnSetDefaultKeyClick(int data)
        {
            config.Value = (KeyboardShortcut)config.DefaultValue;
            keyText.text = config.Value.Serialize();
        }

        public void OnSetNoneKeyClick(int data)
        {
            config.Value = (KeyboardShortcut)config.DefaultValue;
            keyText.text = config.Value.Serialize();
        }

        public void SettingChanged()
        {
            keyText.text = config.Value.Serialize();
        }
    }
}
