using BattleTech;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;

namespace RepeatableFlashpoints {

    [HarmonyPatch(typeof(SimGameState), "CompleteFlashpoint")]
    public static class SimGameState_CompleteFlashpoint_Patch {
        private static void Postfix(SimGameState __instance, Flashpoint fp, FlashpointEndType howItEnded) {
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
                if (Helper.Settings.randomPlanet || Helper.Settings.debugAllRepeat) {
                    foreach (FlashpointDef fp in ___flashpointPool) {
                        if (Helper.Settings.randomPlanet) {
                            foreach (string tag in fp.LocationRequirements.RequirementTags) {
                                if (tag.Contains("planet_name")) {
                                    fp.LocationRequirements.RequirementTags.Remove(tag);
                                }
                            }
                        }
                        if (Helper.Settings.debugAllRepeat) {
                            fp.Repeatable = true;
                        }
                    }
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "ApplyEventAction")]
    public static class SimGameState_ApplyEventAction_Patch {
        private static void Prefix(SimGameState __instance, ref SimGameResultAction action) {
            try {
                if (action.Type == SimGameResultAction.ActionType.Flashpoint_AddContract || action.Type == SimGameResultAction.ActionType.Flashpoint_StartContract) {
                    SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;
                    if (action.additionalValues[3].Equals("{RANDOM}")) {
                        Array values = Enum.GetValues(typeof(Faction));
                        Random random = new Random();
                        Faction randomFaction = (Faction)values.GetValue(random.Next(values.Length));
                        action.additionalValues[3] = randomFaction.ToString();
                    } else if (action.additionalValues[3].Equals("{PLANETOWNER}")) {
                        action.additionalValues[3] = simulation.ActiveFlashpoint.CurSystem.Owner.ToString();
                    }
                    if (action.additionalValues[4].Equals("{RANDOM}")) {
                        Array values = Enum.GetValues(typeof(Faction));
                        Random random = new Random();
                        Faction randomFaction = (Faction)values.GetValue(random.Next(values.Length));
                        action.additionalValues[4] = randomFaction.ToString();
                    }
                    else if (action.additionalValues[4].Equals("{PLANETOWNER}")) {
                        action.additionalValues[4] = simulation.ActiveFlashpoint.CurSystem.Owner.ToString();
                    }
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }

}
