//using System.Text;
//using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace LSTMMod
{
    public class ConstructionPoint
    {

        public static class Patch
        {
            public static ItemProto ModLDBItemSelect(ItemProtoSet itemProtoSet, int id)
            {
                ItemProto result = itemProtoSet.Select(id);
                if (LSTM.setConstructionPointToGround.Value && result?.prefabDesc != null && result.prefabDesc.isStation)
                {
                    return null;
                }
                return result;
            }

            [HarmonyTranspiler, HarmonyPatch(typeof(ConstructionSystem), "_obj_hpos", [typeof(int)])]
            public static IEnumerable<CodeInstruction> MechaDroneLogic__obj_hpos_Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                List<CodeInstruction> ins = instructions.ToList();
                MethodInfo m_ModLDBItemSelect = typeof(ConstructionPoint.Patch).GetMethod(nameof(ModLDBItemSelect));
                //MethodInfo m_LDBItemSelect = typeof(ItemProtoSet).GetMethod(nameof(ItemProtoSet.Select));

                for (int i = 0; i < ins.Count; i++)
                {
                    //ItemProto itemProto = LDB.items.Select(id);
                    //MethodInfo同士の比較ではなんかうまくいかなかったのでToString()で
                    if (ins[i].opcode == OpCodes.Callvirt && ins[i].operand is MethodInfo o1 && o1.ToString() == "ItemProto Select(Int32)")
                    {
                        ins[i].opcode = OpCodes.Call;
                        ins[i].operand = m_ModLDBItemSelect;
                        break;
                    }
                }

                return ins.AsEnumerable();
            }
        }
    }
}
