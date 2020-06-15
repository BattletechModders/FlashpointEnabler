using BattleTech;
using BattleTech.Data;
using BattleTech.Framework;
using Harmony;
using HBS.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public static class SimGameState_ApplyEventAction_Patch
    {
        private static void Prefix(SimGameState __instance, ref SimGameResultAction action)
        {
            try
            {
                if (action.Type == SimGameResultAction.ActionType.Flashpoint_AddContract || action.Type == SimGameResultAction.ActionType.Flashpoint_StartContract)
                {
                    FlashpointEnabler.Logger.LogLine("FP name = " + action.additionalValues[2]);
                    SimGameState simulation = UnityGameInstance.BattleTechGame.Simulation;

                    // ContractName
                    if (action.additionalValues[2].StartsWith("{RANDOM}"))
                    {
                        string possibleEntries = action.additionalValues[2].Replace("{RANDOM}", "");
                        List<string> entires = possibleEntries.Split(',').ToList();
                        Random random = new Random();
                        string entry = (string)entires[(random.Next(entires.Count))];
                        action.additionalValues[2] = entry.Trim();
                    }
                    
                    //Employer
                    if (action.additionalValues[3].Equals("{RANDOM}"))
                    {
                        action.additionalValues[3] = generateRandomFactionString();
                        FlashpointEnabler.Logger.LogLine("Selected Faction = " + action.additionalValues[3]);
                    }
                    else if (action.additionalValues[3].Equals("{PLANETOWNER}"))
                    {
                        action.additionalValues[3] = simulation.ActiveFlashpoint.CurSystem.OwnerValue.Name;
                        FlashpointEnabler.Logger.LogLine("Set System owner Faction = " + action.additionalValues[3]);
                    }
                    else if (action.additionalValues[3].Equals("{ACTIVE}"))
                    {
                        action.additionalValues[3] = generateActiveFactionString(simulation);
                    }

                    //Target
                    if (action.additionalValues[4].Equals("{RANDOM}"))
                    {
                        action.additionalValues[4] = generateRandomFactionString();
                        FlashpointEnabler.Logger.LogLine("Selected Faction = " + action.additionalValues[4]);
                    }
                    else if (action.additionalValues[4].Equals("{PLANETOWNER}"))
                    {
                        action.additionalValues[4] = simulation.ActiveFlashpoint.CurSystem.OwnerValue.Name;
                        FlashpointEnabler.Logger.LogLine("Set System owner Faction = " + action.additionalValues[4]);
                    }
                    else if (action.additionalValues[4].Equals("{ACTIVE}"))
                    {
                        action.additionalValues[4] = generateActiveFactionString(simulation);
                    }

                    //3-way
                    if (action.additionalValues.Length >= 12 && !string.IsNullOrEmpty(action.additionalValues[11]))
                    {
                        if (action.additionalValues[11].Equals("{RANDOM}"))
                        {
                            action.additionalValues[11] = generateRandomFactionString();
                            FlashpointEnabler.Logger.LogLine("Selected Faction = " + action.additionalValues[11]);
                        }
                        else if (action.additionalValues[11].Equals("{PLANETOWNER}"))
                        {
                            action.additionalValues[11] = simulation.ActiveFlashpoint.CurSystem.OwnerValue.Name;
                            FlashpointEnabler.Logger.LogLine("Set System owner Faction = " + action.additionalValues[11]);
                        }
                        else if (action.additionalValues[11].Equals("{ACTIVE}"))
                        {
                            action.additionalValues[11] = generateActiveFactionString(simulation);
                        }
                    }
                    if (string.IsNullOrEmpty(action.value))
                    {
                        ContractOverride contractOverride = simulation.DataManager.ContractOverrides.Get(action.additionalValues[2]).Copy();
                        int contractType = contractOverride.ContractTypeValue.ID;
                        List<MapAndEncounters> releasedMapsAndEncountersByContractTypeAndOwnership = MetadataDatabase.Instance.GetReleasedMapsAndEncountersByContractTypeAndOwnership(contractType, false);
                        foreach (MapAndEncounters map in releasedMapsAndEncountersByContractTypeAndOwnership)
                        {
                            if (map.Map.MapName.Equals("mapGeneral_terraceLakes_vLow"))
                            {
                                releasedMapsAndEncountersByContractTypeAndOwnership.Remove(map);
                            }
                        }
                        releasedMapsAndEncountersByContractTypeAndOwnership.Shuffle();
                        MapAndEncounters mapAndEncounters = releasedMapsAndEncountersByContractTypeAndOwnership[0];
                        action.value = mapAndEncounters.Map.MapName;
                    }
                }
            }
            catch (Exception e)
            {
                FlashpointEnabler.Logger.LogError(e);
            }
        }

        private static string generateRandomFactionString()
        {
            List<FactionValue> values = FactionEnumeration.FactionList;
            Random random = new Random();
            FactionValue randomFaction;
            do
            {
                randomFaction = (FactionValue)values[(random.Next(values.Count))];
            }
            while (Helper.IsExcluded(randomFaction.Name));
            return randomFaction.Name;
        }

        private static string generateActiveFactionString(SimGameState simulation)
        {
            List<string> values = simulation.ActiveFlashpoint.CurSystem.Def.ContractEmployerIDList;
            Random random = new Random();
            string randomFaction;
            do
            {
                randomFaction = (string)values[(random.Next(values.Count))];
            }
            while (Helper.IsExcluded(randomFaction) || randomFaction == simulation.ActiveFlashpoint.CurSystem.OwnerValue.Name);
            return randomFaction;
        }
    }

}
