using System;
using System.Collections.Generic;
using System.Text;

using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LSTMMod
{
    public class UIMaterialPicker : MonoBehaviour
    {
        public static int numberOfSlots = 12;
        public Dictionary<int, int[]> specialMaterials = new Dictionary<int, int[]>(){
            {1001, new int[]{ 1001, 1002, 1003, 1004, 1005, 1006, 0, 1101}}, //Iron ore
            {1002, new int[]{ 1001, 1002, 1003, 1004, 1005, 1006, 0, 1104}}, //Copper ore
            {1003, new int[]{ 1001, 1002, 1003, 1004, 1005, 1006, 0, 1105}}, //Silicon ore
            {1004, new int[]{ 1001, 1002, 1003, 1004, 1005, 1006, 0, 1106}}, //Titanium ore
            {1005, new int[]{ 1001, 1002, 1003, 1004, 1005, 1006, 0, 1108, 1110}}, //Stone
            {1006, new int[]{ 1001, 1002, 1003, 1004, 1005, 1006, 0, 1109}}, //Coal
            {1000, new int[]{ 1000, 1007, 1114, 1116 }}, //Water
            {1007, new int[]{ 1000, 1007, 1114, 1116 }}, //Crude oil
            {1114, new int[]{ 1000, 1007, 1114, 1116 }}, //Refined oil
            {1116, new int[]{ 1000, 1007, 1114, 1116 }}, //Sulfuric acid
            {1120, new int[]{ 1120, 1121, 1011 }}, //Hydrogen
            {1121, new int[]{ 1120, 1121, 1011 }}, //Deuterium
            {1011, new int[]{ 1120, 1121, 1011 }}, //Fire ice
            {1208, new int[]{ 1208, 1122 }}, //Critical photon
            {1122, new int[]{ 1208, 1122 }}, //Antimatter

        };
        public Dictionary<int, int[]> additionalMaterials = new Dictionary<int, int[]>()
        {
            {2001, new int[]{ 2001, 2002, 2003 }}, //Conveyor belt MK.I
            {2002, new int[]{ 2001, 2002, 2003 }}, //Conveyor belt MK.II
            {2003, new int[]{ 2001, 2002, 2003 }}, //Conveyor belt MK.III
            {1141, new int[]{ 1141, 1142, 1143 }}, //Proliferator (Accelerant) Mk.I
            {1142, new int[]{ 1141, 1142, 1143 }}, //Proliferator (Accelerant) Mk.II
            {1143, new int[]{ 1141, 1142, 1143 }}, //Proliferator (Accelerant) Mk.III
            {2011, new int[]{ 2011, 2012, 2013 }}, //Sorter MK.I
            {2012, new int[]{ 2011, 2012, 2013 }}, //Sorter MK.II
            {2013, new int[]{ 2011, 2012, 2013 }}, //Sorter MK.III
            {2303, new int[]{ 2303, 2304, 2305 }}, //Assembling machine Mk.I
            {2304, new int[]{ 2303, 2304, 2305 }}, //Assembling machine Mk.II
            {2305, new int[]{ 2303, 2304, 2305 }}, //Assembling machine Mk.III
            {2302, new int[]{ 2302, 2315 }}, //Smelter
            {2315, new int[]{ 2302, 2315 }}, //Plane  Smelter
            {1801, new int[]{ 1801, 1802, 1803 }}, //Hydrogen fuel rod
            {1802, new int[]{ 1801, 1802, 1803 }}, //Deuteron fuel rod
            {1803, new int[]{ 1801, 1802, 1803 }}, //Antimatter fuel rod
            {2101, new int[]{ 2101, 2102, 2106 }}, //Storage MK.I
            {2102, new int[]{ 2101, 2102, 2106 }}, //Storage MK.II
            {2106, new int[]{ 2101, 2102, 2106 }}, //Storage tank
        };

        public int productItemId;
        public UIMaterialViewItem[] items;
        public UIBalanceWindow window;

        public static UIMaterialPicker CreateView(Component parent)
        {
            GameObject go = new GameObject("material-view");
            go.AddComponent<RectTransform>();
            UIMaterialPicker materialView = go.AddComponent<UIMaterialPicker>();
            go.transform.SetParent(parent.transform, false);

            UISlotPicker slotPicker = UIRoot.instance.uiGame.slotPicker;

            RectTransform bg = slotPicker.bgTrans;
            if (bg != null)
            {
                GameObject bgGo = UnityEngine.Object.Instantiate(bg.gameObject);
                Image img = bgGo.GetComponent<Image>();
                if (img != null)
                {
                    img.color = new Color(0.19f, 0.19f, 0.19f, 0.92f);
                }

                RectTransform rect2 = Util.NormalizeRectB(bgGo);
                bgGo.transform.SetParent(go.transform, false);
                rect2.offsetMax = Vector2.zero;
                rect2.offsetMin = Vector2.zero;
            }

            RectTransform rect = Util.NormalizeRectD(go);
            rect.sizeDelta = new Vector2(30f, 200f);
            rect.anchoredPosition = new Vector2(-20f, -90f);

            materialView.items = new UIMaterialViewItem[numberOfSlots];

            for (int i = 0; i < numberOfSlots; i++)
            {
                GameObject go2 = new GameObject("btn-" + i.ToString());
                go2.AddComponent<RectTransform>();
                rect = Util.NormalizeRectD(go2, 40f, 40f);
                materialView.items[i] = go2.AddComponent<UIMaterialViewItem>();
                go2.transform.SetParent(go.transform, false);

                UIButton btn = UnityEngine.Object.Instantiate<UIButton>(slotPicker.iconButtonProto, go2.transform);
                Image img = UnityEngine.Object.Instantiate<Image>(slotPicker.iconImageProto, go2.transform);
                if (btn != null && img != null)
                {
                    btn.gameObject.SetActive(true);
                    img.gameObject.SetActive(true);
                    (btn.transform as RectTransform).anchoredPosition = Vector2.zero;
                    img.rectTransform.anchoredPosition = Vector2.zero;
                    materialView.items[i].image = img;
                    materialView.items[i].btn = btn;
                    

                    btn.tips.offset = new Vector2(10, -20);
                    btn.tips.corner = 6;
                    btn.tips.delay = 0.8f;
                    btn.tips.tipText = "Click: select item\nRight-click: select and update this list".Translate();
                    btn.tips.tipTitle = "Material Selector".Translate();

                    materialView.items[i].itemId = 0;
                    btn.onClick += materialView.OnMaterialButtonClick;
                    btn.onRightClick += materialView.OnMaterialButtonRightClick;
                    btn.data = i;
                }
                rect.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                go2.SetActive(false);
            }

            go.SetActive(false);
            return materialView;
        }


        private void OnMaterialButtonClick(int idx)
        {
            if (window != null && idx >= 0 && idx < numberOfSlots)
            {
                int itemId = items[idx].itemId;
                window.Filter(itemId, 0, false);
            }
        }
        private void OnMaterialButtonRightClick(int idx)
        {
            if (window != null && idx >= 0 && idx < numberOfSlots)
            {
                int itemId = items[idx].itemId;
                window.Filter(itemId, 0, true);
            }
        }

        public void RefreshWithProduct(int newProductItemId)
        {
            if (!LSTM.showMaterialPicker.Value)
            {
                if (gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                }
                return;
            }
            if (productItemId == newProductItemId)
            {
                return;
            }
            productItemId = newProductItemId;

            if (productItemId == 0)
            {
                ResetSlots();
                return;
            }

            int[] materials = MaterialsForItem(productItemId);
            int idx = 0;
            foreach (int mId in materials)
            {
                if (idx >= numberOfSlots)
                {
                    break;
                }
                items[idx].SetItemId(mId);
                idx++;
            }
            ResetSlots(idx);
        }

        public int[] MaterialsForItem(int itemId)
        {
            if (specialMaterials.ContainsKey(itemId))
            {
                return specialMaterials[itemId];
            }

            int[] reslt = new int[numberOfSlots];
            ItemProto itemProto = LDB.items.Select(productItemId);

            int idx = 0;
            List<RecipeProto> recipes = itemProto?.recipes;
            foreach (RecipeProto recipe in recipes)
            {
                foreach (int mId in recipe.Items)
                {
                    if (idx >= numberOfSlots)
                    {
                        break;
                    }
                    reslt[idx] = mId;
                    idx++;
                }
                if (idx >= numberOfSlots)
                {
                    break;
                }
                reslt[idx] = 0;
                idx++;
            }

            
            if (additionalMaterials.ContainsKey(itemId))
            {
                foreach (int mId2 in additionalMaterials[itemId])
                {
                    if (idx >= numberOfSlots)
                    {
                        break;
                    }
                    reslt[idx] = mId2;
                    idx++;
                }
            }
            else
            {
                if (idx < numberOfSlots && idx > 0)
                {
                    reslt[idx] = itemId;
                }
            }

            return reslt;
        }

        public void ResetSlots(int fromIdx = 0)
        {
            if (fromIdx < numberOfSlots)
            {
                for (int i = fromIdx; i < numberOfSlots; i++)
                {
                    items[i].SetItemId(0);
                }
            }
            RearrangeLayout();
        }

        public void RearrangeLayout()
        {
            float offset = 10f;
            bool prevActive = false;
            foreach (var item in items)
            {
                bool isActive = item.RefreshActive();
                if (isActive)
                {
                    item.SetPosition(offset);
                    offset += 32;
                }
                else if (prevActive)
                {
                    offset += 10;
                }
                prevActive = isActive;

            }
            if (offset < 42f)
            {
                gameObject.SetActive(false);
            }
            else
            {
                RectTransform rect = (RectTransform)gameObject.transform;
                if (!prevActive)
                {
                    offset -= 2;
                }
                rect.sizeDelta = new Vector2(46f, offset);
                gameObject.SetActive(true);
            }
        }

    }

    public class UIMaterialViewItem : MonoBehaviour
    {
        public UIButton btn;
        public Image image;
        public int itemId;

        public void SetItemId(int newId)
        {
            if (itemId == newId || image == null)
            {
                return;
            }
            itemId = newId;

            if (btn != null)
            {
                btn.tips.itemId = itemId;
            }

            if (image != null) {
                if (itemId == 0)
                {
                    image.sprite = null;
                    return;
                }
                Sprite sprite = LDB.items.Select(itemId)?.iconSprite;
                image.sprite = sprite;
            }

        }

        public void SetPosition(float offset)
        {
            RectTransform rect = (RectTransform)gameObject.transform;
            rect.anchoredPosition = new Vector2(4, -offset);
        }

        public bool RefreshActive()
        {
            bool active = itemId != 0;
            gameObject.SetActive(active);
            return active;
        }
    }
}
