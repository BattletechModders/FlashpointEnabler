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
                        using (StreamReader r = new StreamReader($"{RepeatableFlashpoints.ModDirectory}/settings.json")) {
                            string json = r.ReadToEnd();
                            _settings = JsonConvert.DeserializeObject<Settings>(json);
                        }
                    }
                    return _settings;
                }
                catch (Exception ex) {
                    Logger.LogError(ex);
                    return null;
                }
            }
        }
    }
}