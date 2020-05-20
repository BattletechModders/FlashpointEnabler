using BattleTech;
using Newtonsoft.Json;
using System;
using System.IO;

namespace RepeatableFlashpoints {
    public static class Helper {
        private static Settings _settings;
        public static Settings Settings {
            get {
                try {
                    if (_settings == null) {
                        using (StreamReader r = new StreamReader($"{FlashpointEnabler.ModDirectory}/settings.json")) {
                            string json = r.ReadToEnd();
                            _settings = JsonConvert.DeserializeObject<Settings>(json);
                            _settings.fixExcludedFactions();
                        }
                    }
                    return _settings;
                }
                catch (Exception ex) {
                    FlashpointEnabler.Logger.LogError(ex);
                    return null;
                }
            }
        }

        public static bool IsExcluded(string faction) {
            string toCheck = faction;
            if (Settings.caseInsenstiveBlacklist)
            {
                toCheck = faction.ToLower();
            }
            if (Settings.excludedFactions.Contains(toCheck)) {
                return true;
            } else {
                return false;
            }
        }
    }
}