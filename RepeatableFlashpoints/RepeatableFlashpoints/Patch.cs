using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
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
                FlashpointEnabler.Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "GenerateFlashpoint")]
    public static class SimGameState_GenerateFlashpoint_Patch {
        private static void Prefix(SimGameState __instance) {
            try {
                if (Helper.Settings.randomPlanet || Helper.Settings.debugAllRepeat) {
                    foreach (FlashpointDef fp in __instance.FlashpointPool) {
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
                FlashpointEnabler.Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "ApplyEventAction")]
    public static class SimGameState_ApplyEventAction_Patch {
        private static void Prefix(SimGameState __instance, ref SimGameResultAction action) {
            try {
                if (action.Type == SimGameResultAction.ActionType.Flashpoint_AddContract || action.Type == SimGameResultAction.ActionType.Flashpoint_StartContract) {
                    FlashpointEnabler.Logger.LogLine("FP name = " + action.additionalValues[2]);
                    SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;
                    if (action.additionalValues[3].Equals("{RANDOM}")) {
                        List<FactionValue> values = FactionEnumeration.FactionList;
                        Random random = new Random();
                        FactionValue randomFaction;
                        do {
                            randomFaction = (FactionValue)values[(random.Next(values.Count))];
                        }
                        while (Helper.IsExcluded(randomFaction.Name));
                        action.additionalValues[3] = randomFaction.Name;
                    }
                    else if (action.additionalValues[3].Equals("{PLANETOWNER}")) {
                        action.additionalValues[3] = simulation.ActiveFlashpoint.CurSystem.OwnerValue.Name;
                    }
                    if (action.additionalValues[4].Equals("{RANDOM}")) {
                        List<FactionValue> values = FactionEnumeration.FactionList;
                        Random random = new Random();
                        FactionValue randomFaction;
                        do
                        {
                            randomFaction = (FactionValue)values[(random.Next(values.Count))];
                        }
                        while (Helper.IsExcluded(randomFaction.Name));
                        action.additionalValues[4] = randomFaction.Name;
                    }
                    else if (action.additionalValues[4].Equals("{PLANETOWNER}")) {
                        action.additionalValues[4] = simulation.ActiveFlashpoint.CurSystem.OwnerValue.Name;
                    }
                    if (string.IsNullOrEmpty(action.value)) {
                        ContractOverride contractOverride = simulation.DataManager.ContractOverrides.Get(action.additionalValues[2]).Copy();
                        int contractType = contractOverride.ContractTypeValue.ID;
                       List<MapAndEncounters> releasedMapsAndEncountersByContractTypeAndOwnership = MetadataDatabase.Instance.GetReleasedMapsAndEncountersByContractTypeAndOwnership(contractType, false);
                        releasedMapsAndEncountersByContractTypeAndOwnership.Shuffle();
                        MapAndEncounters mapAndEncounters = releasedMapsAndEncountersByContractTypeAndOwnership[0];
                        action.value = mapAndEncounters.Map.MapName;
                    }
                }
            }
            catch (Exception e) {
                FlashpointEnabler.Logger.LogError(e);
            }
        }
    }

}
