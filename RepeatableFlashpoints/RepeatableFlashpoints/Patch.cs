using BattleTech;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;

namespace RepeatableFlashpoints {

    [HarmonyPatch(typeof(SimGameState), "CompleteFlashpoint")]
    public static class SimGameState_CompleteFlashpoint_Patch {
        private static void Postfix(SimGameState __instance, Flashpoint fp, FlashpointEndType howItEnded)
        {
            try {
                if (howItEnded == FlashpointEndType.Completed && fp.Def.Repeatable) {
                    __instance.completedFlashpoints.Remove(fp.Def.Description.Id);
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }


    [HarmonyPatch(typeof(SimGameState), "GenerateFlashpoint")]
    public static class SimGameState_GenerateFlashpoint_Patch {
        private static void Prefix(SimGameState __instance, ref bool useInitial, ref List<FlashpointDef> ___flashpointPool) {
            try {
                useInitial = false;
                if (Helper.Settings.randomPlanet) {
                    foreach (FlashpointDef fp in ___flashpointPool) {
                        foreach (string tag in fp.LocationRequirements.RequirementTags) {
                            if (tag.Contains("planet_name")) {
                                fp.LocationRequirements.RequirementTags.Remove(tag);
                            }
                        }
                    }
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }
    
}
