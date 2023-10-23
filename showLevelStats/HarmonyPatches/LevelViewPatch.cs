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
using showLevelStats;

namespace showLevelStats.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelDetailView))]
    [HarmonyPatch("SetContentForBeatmapDataAsync", MethodType.Normal)]
    internal class difficultySelected
    {
        public static IDifficultyBeatmap diff = null;
        private static void Postfix(IDifficultyBeatmap ____selectedDifficultyBeatmap)
        {
            diff = ____selectedDifficultyBeatmap;
            LevelProfileView.instance.clearDetails();
            LevelListTableCellSetDataFromLevel.GetSongStats();
        }
    }

    [HarmonyPatch(typeof(StandardLevelDetailView))]
    [HarmonyPatch("SetContent", MethodType.Normal)]
    internal class LevelListTableCellSetDataFromLevel
    {
        public static CurvedTextMeshPro textMesh;
        public static LevelInformation levelInfo = null;
        public static MapperInformation.User mapperInfo = null;
        public static IBeatmapLevel iLevel = null;

        private static void Postfix(IBeatmapLevel level, IDifficultyBeatmap ____selectedDifficultyBeatmap, StandardLevelDetailView __instance, BeatmapDifficulty defaultDifficulty, BeatmapCharacteristicSO defaultBeatmapCharacteristic, PlayerData playerData, TextMeshProUGUI ____actionButtonText)
        {
            iLevel = level;
            UI.StatsView.instance.Create(__instance.transform.parent.GetComponent<StandardLevelDetailViewController>());
            LevelProfileView.Create(__instance.gameObject);

            StatsView.instance.setText("");
            LevelProfileView.instance.clearDetails();
            GetSongStats();
        }

        public static async void GetSongStats()
        {
            //カスタム曲でない場合return
            if (iLevel.levelID.IndexOf("custom_level") == -1) return;

            //修正
            string url = "https://api.beatsaver.com/maps/hash/" + iLevel.levelID.Substring(13, 40);

            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url); // 非同期にWebリクエストを送信する

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync(); // 非同期にレスポンスを読み込む

                levelInfo = JsonConvert.DeserializeObject<LevelInformation>(result);
                if (iLevel.levelID.Substring(13, 40).ToUpper() != levelInfo.versions[0].hash.ToUpper()) { Plugin.Log.Info(iLevel.levelID + ":" + levelInfo.versions[0].hash); return; }

                //debug
                float offset = difficultySelected.diff.noteJumpStartBeatOffset;
                float jumpDistance = CalculateJumpDistance(difficultySelected.diff.level.beatsPerMinute, difficultySelected.diff.noteJumpMovementSpeed, difficultySelected.diff.noteJumpStartBeatOffset);
                float reactionTime = jumpDistance * 500 / difficultySelected.diff.noteJumpMovementSpeed;
                string textConfig = Settings.Instance.textConfig;

                textConfig = textConfig.Replace("{BSR}", levelInfo.id.ToString());
                textConfig = textConfig.Replace("{upvotes}", levelInfo.stats.upvotes.ToString());
                textConfig = textConfig.Replace("{downvotes}", levelInfo.stats.downvotes.ToString());
                textConfig = textConfig.Replace("{uploadDate}", levelInfo.uploaded.ToString());
                textConfig = textConfig.Replace("{JD}", jumpDistance.ToString("F2"));
                textConfig = textConfig.Replace("{RT}", reactionTime.ToString("F0"));
                textConfig = textConfig.Replace("{NJS}", difficultySelected.diff.noteJumpMovementSpeed.ToString());
                textConfig = textConfig.Replace("{ranked}", ((levelInfo.ranked) ? "rank" : ""));
                textConfig = textConfig.Replace("{totalPlays}", levelInfo.stats.plays.ToString());
                textConfig = textConfig.Replace("{downloads}", levelInfo.stats.downloads.ToString());
                textConfig = textConfig.Replace("{hash}", levelInfo.uploader.hash.ToString());
                textConfig = textConfig.Replace("{name}", levelInfo.name.ToString());
                textConfig = textConfig.Replace("{description}", levelInfo.description.ToString());

                StatsView.instance.setText(textConfig);

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

                mapperInfo = JsonConvert.DeserializeObject<MapperInformation.User>(result);

                LevelProfileView.instance.setMapperDetails(mapperInfo);
                if(Settings.Instance.autoTranslate)LevelProfileView.instance.onTranslate();
            }
        }
        public static float CalculateJumpDistance(float bpm, float njs, float offset)
        {
            float jumpdistance = 0f; // In case
            float halfjump = 4f;
            float num = 60f / bpm;

            // Need to repeat this here even tho it's in BeatmapInfo because sometimes we call this function directly
            if (njs <= 0.01) // Is it ok to == a 0f?
                njs = 10f;

            while (njs * num * halfjump > 17.999)
                halfjump /= 2;

            halfjump += offset;
            if (halfjump < 0.25f)
                halfjump = 0.25f;

            jumpdistance = njs * num * halfjump * 2;

            return jumpdistance;
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
