using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Newtonsoft.Json;
using showLevelStats;
using HMUI;
using System.Security.Cryptography.Xml;
using static AlphabetScrollInfo;
using showLevelStats.UI;
using UnityEngine.Profiling;

namespace showLevelStats.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelDetailView))]
    [HarmonyPatch("SetContent", MethodType.Normal)]
    internal class LevelListTableCellSetDataFromLevel
    {
        public static CurvedTextMeshPro textMesh;
        public static LevelInformation levelInfo = null;

        private static void Postfix(IBeatmapLevel level, StandardLevelDetailView __instance, BeatmapDifficulty defaultDifficulty, BeatmapCharacteristicSO defaultBeatmapCharacteristic, PlayerData playerData, TextMeshProUGUI ____actionButtonText)
        {
            Plugin.Log.Info(__instance.ToString());
            UI.StatsView.instance.Create(__instance.transform.parent.GetComponent<StandardLevelDetailViewController>());
            LevelProfileView.Create(__instance.gameObject);

            //カスタム曲でない場合return
            if (level.levelID.IndexOf("custom_level") == -1) return;

            string url = "https://api.beatsaver.com/maps/hash/" + level.levelID.Substring(13);

            StatsView.instance.setText("");
            GetSongStats(url);
        }

        private static async void GetSongStats(string url)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url); // 非同期にWebリクエストを送信する

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync(); // 非同期にレスポンスを読み込む

                levelInfo = JsonConvert.DeserializeObject<LevelInformation>(result);

                string str = levelInfo.id + "(↑<color=#00ff00>" + levelInfo.stats.upvotes +
                        "</color>:↓<color=#ff0000>" + levelInfo.stats.downvotes + "</color>)\n" +
                        levelInfo.updatedAt+"\n\n\n";
                StatsView.instance.setText(str);
                LevelProfileView.instance.setDetails(levelInfo);

                GetMapperInfo(levelInfo.uploader.playlistUrl);
            }
        }
        private static async void GetMapperInfo(string url)
        {
            if (url.IndexOf("/playlist") < 0) return;

            url = url.Substring(0, url.IndexOf("/playlist"));

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url); // 非同期にWebリクエストを送信する

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync(); // 非同期にレスポンスを読み込む

                MapperInformation.User data = JsonConvert.DeserializeObject<MapperInformation.User>(result);
                LevelProfileView.instance.setMapperDetails(data);
                LevelProfileView.instance.setDetails(levelInfo);

            }
        }
        public enum BeatmapDifficulty
        {
            // Token: 0x04000006 RID: 6
            Easy,
            // Token: 0x04000007 RID: 7
            Normal,
            // Token: 0x04000008 RID: 8
            Hard,
            // Token: 0x04000009 RID: 9
            Expert,
            // Token: 0x0400000A RID: 10
            ExpertPlus
        }
    }
}
