using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;
using System.IO;
using Newtonsoft.Json;
using showLevelStats;
using BeatSaberMarkupLanguage.Settings;

namespace showLevelStats
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        private HarmonyLib.Harmony _harmony;
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        string filePath = "\\UserData\\showLevelStats.json";

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, IPA.Config.Config conf)
        {
            Instance = this;
            Log = logger;
            Log.Info("showLevelStats initialized.");
            Settings.Instance = conf.Generated<Settings>();
            BSMLSettings.instance.AddSettingsMenu("showLevelStats", "showLevelStats.Configuration.settings.bsml", SettingsHandler.instance);

            //settings
            if (File.Exists(Directory.GetCurrentDirectory() + filePath))
            {


                string settingsJson = File.ReadAllText(Directory.GetCurrentDirectory() + filePath);

                // JSONデータを変数にデシリアライズ
                 Settings.Instance = JsonConvert.DeserializeObject<Settings>(settingsJson);
            }
            else
            {
                File.Create(Directory.GetCurrentDirectory() + filePath);
                Settings.Instance = new Settings();
            }
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        /*
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Log.Debug("Config loaded");
        }
        */
        #endregion

        [OnStart]
        public void OnApplicationStart()
        {
            _harmony = new Harmony("com.scifiHerb.BeatSaber.showLevelStats");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

            Log.Debug("OnApplicationStart");
            new GameObject("showLevelStatsController").AddComponent<showLevelStatsController>();

        }

        [OnExit]
        public void OnApplicationQuit()
        {
            Log.Debug("OnApplicationQuit");
            File.WriteAllText(Directory.GetCurrentDirectory() + filePath, JsonConvert.SerializeObject(Settings.Instance));
        }
    }
}
