using System;
using Harmony;
using System.Reflection;

namespace RepeatableFlashpoints {
    public class RepeatableFlashpoints {
        internal static string ModDirectory;
        public static void Init(string directory, string settingsJSON) {
            try
            {
                var harmony = HarmonyInstance.Create("de.morphyum.RepeatableFlashpoints");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                ModDirectory = directory;
            }
            catch (Exception e)
            {
                FileLog.Log(e.ToString());
                Logger.LogError(e);
            }
        }
    }
}
