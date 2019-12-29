using System;
using Harmony;
using System.Reflection;

namespace RepeatableFlashpoints {
    public class FlashpointEnabler {
        internal static string ModDirectory;
        internal static Logger Logger;

        public static void Init(string directory, string settingsJSON) {

            // Add a hook to dispose of logging on shutdown
            Logger = new Logger(directory, "flashpoint_enabler", false);
            System.AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Logger.Close();

            try
            {
                var harmony = HarmonyInstance.Create("de.morphyum.FlashpointEnabler");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                ModDirectory = directory;
            }
            catch (Exception e)
            {
                //FileLog.Log(e.ToString());
                Logger.LogError(e);
            }
        }
    }
}
