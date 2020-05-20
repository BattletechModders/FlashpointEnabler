using System.Collections.Generic;

namespace RepeatableFlashpoints {
    public class Settings {
        public bool randomPlanet = false;
        public bool debugAllRepeat = false;
        public bool caseInsenstiveBlacklist = false;
        public List<string> excludedFactions = new List<string>();

        public void fixExcludedFactions()
        {
            if (caseInsenstiveBlacklist)
            {
                List<string> blackList = new List<string>();
                foreach (string excluded in excludedFactions)
                {
                    blackList.Add(excluded.ToLower());
                }
                excludedFactions = blackList;
            }
        }
    }
}
