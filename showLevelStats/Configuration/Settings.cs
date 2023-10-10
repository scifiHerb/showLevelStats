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
        public bool autoTranslate = false;
        public string textConfig = "{BSR}(↑<color=#00ff00>{upvotes}</color>:↓<color=#ff0000>{downvotes}</color>)\n{uploadDate}(<color=#ffff00>{JD}</color>:<color=#800080>{RT}</color>)";
        public string sample1 = "{BSR}(↑<color=#00ff00>{upvotes}</color>:↓<color=#ff0000>{downvotes}</color>)\n{uploadDate}";
        public string sample2 = "{BSR}(↑<color=#00ff00>{upvotes}</color>:↓<color=#ff0000>{downvotes}</color>)\n{uploadDate}(<color=#ffff00>{JD}</color>:<color=#800080>{RT}</color>)";
        public float offset_x = 82.0F;
        public float offset_y = -40;
        public float offset_z = 0;
    }
}
