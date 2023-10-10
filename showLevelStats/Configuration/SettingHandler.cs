using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;

namespace showLevelStats
{
    class SettingsHandler : PersistentSingleton<SettingsHandler>
    {
        [UIValue("autoTranslate")]
        public bool autoTranslate
        {
            get => Settings.Instance.autoTranslate;
            set
            {
                Settings.Instance.autoTranslate = value;
            }
        }

        [UIValue("list-options")]
        private List<object> options = new object[] { "ja", "en" , "cn", "tw", "ko", "ru", "fr", "de" }.ToList();

        [UIValue("list-choice")]
        private string translateLanguage
        {
            get => Settings.Instance.translateLanguage;
            set
            {
                Settings.Instance.translateLanguage = value;
            }
        }
    }
}
