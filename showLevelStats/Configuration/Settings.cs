using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace showLevelStats
{
    public class Settings
    {
        public static Settings Instance = new Settings();
        public string translateLanguage = "ja";
        public List<object> options = new object[] { "ja", "en" }.ToList();
        public bool showBSR = true;
        public bool showDate = true;
        public bool showVotes = true;
        public bool autoTranslate = false;
    }
}
