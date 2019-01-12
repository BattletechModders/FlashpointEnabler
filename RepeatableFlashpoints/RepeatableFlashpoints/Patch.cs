using BattleTech;
using Harmony;
using System;

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
}
