using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using Verse;
using RimWorld;
using System.Reflection;
using UnityEngine;

namespace FastUnitSelection {
    [StaticConstructorOnStartup]
    class FastUnitSelection {

        static Selector selector;

        static List<object> selectedObjs;

        static Dictionary<KeyCode, List<object>> teamDict;

        static List<KeyCode> numList;

        static FastUnitSelection() {
            var harmony = HarmonyInstance.Create("alexniver.FastUnitSelection");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
            selectedObjs = new List<object>();
            teamDict = new Dictionary<KeyCode, List<object>>();
            numList = new List<KeyCode>();
            numList.Add(KeyCode.Alpha5);
            numList.Add(KeyCode.Alpha6);
            numList.Add(KeyCode.Alpha7);
            numList.Add(KeyCode.Alpha8);
            numList.Add(KeyCode.Alpha9);
            numList.Add(KeyCode.Alpha0);
        }

        static KeyCode NumKeyPressed() {
            foreach(KeyCode code in numList) {
                if(Input.GetKey(code)) {
                    return code;
                }
            }
            return KeyCode.None;
        }

        [HarmonyPatch(typeof(Selector), "Select", new Type[] { typeof(object), typeof(bool), typeof(bool) })]
        class Patch_Selector_Select {
            // Token: 0x06000070 RID: 112 RVA: 0x00004C08 File Offset: 0x00002E08
            private static void Postfix(Selector __instance, object obj, bool playSound = true, bool forceDesignatorDeselect = true) {
                if(selector == null) {
                    selector = __instance;
                }

                selectedObjs = new List<object>();
                foreach(object tmpObj in __instance.SelectedObjects) {
                    if(tmpObj is Pawn) {
                        selectedObjs.Add(tmpObj);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Selector), "Deselect", new Type[] { typeof(object) })]
        class Patch_Selector_Deselect {
            private static void Postfix(Selector __instance, object obj) {
                if(selector == null) {
                    selector = __instance;
                }
                selectedObjs = new List<object>();
                foreach(object tmpObj in __instance.SelectedObjects) {
                    if(tmpObj is Pawn) {
                        selectedObjs.Add(tmpObj);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Selector), "ClearSelection")]
        class Patch_Selector_ClearSelection {
            private static void Postfix(Selector __instance) {
                if(selector == null) {
                    selector = __instance;
                }

                selectedObjs = new List<object>();
            }
        }



        [HarmonyPatch(typeof(Game), "UpdatePlay")]
        class Patch_Game_UpdatePlay {
            private static void Postfix() {
                KeyCode numPressed = NumKeyPressed();
                if(Input.GetKey(KeyCode.LeftControl) && numPressed != KeyCode.None) {
                    if(selectedObjs.Count > 0) {
                        teamDict[numPressed] = selectedObjs;
                    }
                }
                if(!Input.GetKey(KeyCode.LeftControl) && numPressed != KeyCode.None) {
                    if(teamDict.ContainsKey(numPressed) && teamDict[numPressed].Count > 0) {
                        selector.ClearSelection();
                        List<object> tmpList = new List<object>(teamDict[numPressed]);
                        foreach(object obj in tmpList) { 
                            selector.Select(obj);
                        }
                    }
                }
            }
        }
    }
}
