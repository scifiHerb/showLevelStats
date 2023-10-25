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
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace showLevelStats.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelDetailView))]
    [HarmonyPatch("SetContentForBeatmapDataAsync", MethodType.Normal)]
    internal class difficultySelected
    {
        public static IDifficultyBeatmap diff = null;
        public static CustomDifficultyBeatmap diffData = null;

        private static void Postfix(IDifficultyBeatmap ____selectedDifficultyBeatmap, IBeatmapLevel ____level)
        {
            foreach(var l in ____selectedDifficultyBeatmap.level.previewDifficultyBeatmapSets)
            {
                var temp = ____selectedDifficultyBeatmap.level.beatmapLevelData.GetDifficultyBeatmap(l.beatmapCharacteristic, ____selectedDifficultyBeatmap.difficulty);
                diffData = ((CustomDifficultyBeatmap)temp);
            }

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

                //debug
                float offset = difficultySelected.diff.noteJumpStartBeatOffset;
                float jumpDistance = CalculateJumpDistance(difficultySelected.diff.level.beatsPerMinute, difficultySelected.diff.noteJumpMovementSpeed, difficultySelected.diff.noteJumpStartBeatOffset);
                float reactionTime = jumpDistance * 500 / difficultySelected.diff.noteJumpMovementSpeed;
                string textConfig = Settings.Instance.textConfig;

                textConfig = textConfig.Replace("{BSR}", levelInfo.id.ToString());
                textConfig = textConfig.Replace("{upvotes}", levelInfo.stats.upvotes.ToString());
                textConfig = textConfig.Replace("{downvotes}", levelInfo.stats.downvotes.ToString());
                textConfig = textConfig.Replace("{uploadDate}", levelInfo.uploaded.ToString().Substring(0,10));
                textConfig = textConfig.Replace("{JD}", jumpDistance.ToString("F2"));
                textConfig = textConfig.Replace("{RT}", reactionTime.ToString("F0"));
                textConfig = textConfig.Replace("{NJS}", difficultySelected.diff.noteJumpMovementSpeed.ToString());
                textConfig = textConfig.Replace("{ranked}", ((levelInfo.ranked) ? "rank" : ""));
                textConfig = textConfig.Replace("{totalPlays}", levelInfo.stats.plays.ToString());
                textConfig = textConfig.Replace("{downloads}", levelInfo.stats.downloads.ToString());
                textConfig = textConfig.Replace("{hash}", levelInfo.uploader.hash.ToString());
                textConfig = textConfig.Replace("{name}", levelInfo.name.ToString());
                textConfig = textConfig.Replace("{description}", levelInfo.description.ToString());
                textConfig = textConfig.Replace("{warningCrouchWalls}", getWarningCrouchWallsText());
                textConfig = textConfig.Replace("{firstNoteCutDirection}", getFirstNoteDirectionText());
                StatsView.instance.setText(textConfig);

                LevelProfileView.instance.setDetails(levelInfo);
                GetMapperInfo(levelInfo.uploader.playlistUrl);


            }
        }
        private static string getFirstNoteDirectionText()
        {
            Plugin.Log.Info("test");
            //test
            NoteCutDirection? firstLeftDirection = null;
            NoteCutDirection? firstRightDirection = null;
            foreach (var a in difficultySelected.diffData.beatmapSaveData.colorNotes)
            {
                if (firstLeftDirection == null && a.color == BeatmapSaveDataVersion3.BeatmapSaveData.NoteColorType.ColorA)
                {
                    Plugin.Log.Info(a.cutDirection.ToString());
                    firstLeftDirection = a.cutDirection;
                }
                if (firstRightDirection == null && a.color == BeatmapSaveDataVersion3.BeatmapSaveData.NoteColorType.ColorB)
                {
                    firstRightDirection = a.cutDirection;
                }
                if(firstLeftDirection!=null&&firstRightDirection!=null)return $"<color=#FF0000>{convertCutDirectionToArrow(firstLeftDirection)}</color><color=#0000FF>" +
                        $"{convertCutDirectionToArrow(firstRightDirection)}</color>";
            }

            return firstLeftDirection.ToString()+firstRightDirection.ToString();
        }
        private static string convertCutDirectionToArrow(NoteCutDirection? dir)
        {
            switch(dir)
            {
                case NoteCutDirection.Any:
                    return "・";
                case NoteCutDirection.Left:
                    return "←";
                case NoteCutDirection.Up:
                    return "↑";
                case NoteCutDirection.Right:
                    return "→";
                case NoteCutDirection.Down:
                    return "↓";
                case NoteCutDirection.DownLeft:
                    return "↙";
                case NoteCutDirection.UpLeft:
                    return "↖";
                case NoteCutDirection.DownRight:
                    return "↘";
                case NoteCutDirection.UpRight:
                    return "➚";
            }
            return "";
        }
        private static string getWarningCrouchWallsText()
        {
            float firstWallBeat = -1;
            int chrouchWallsCount = 0;
            firstWallBeat = CheckForCrouchWalls(difficultySelected.diffData.beatmapSaveData.obstacles, ref chrouchWallsCount);
            if (firstWallBeat == -1) return "";

            var firstWallTime = firstWallBeat * (60.0F / difficultySelected.diffData.beatsPerMinute);
            var firstWallText = $"{((int)Math.Floor(firstWallTime / 60)).ToString("D2")}：{((int)Math.Floor(firstWallTime % 60)).ToString("D2")} . {((int)Math.Floor((firstWallTime * 100) % 100)).ToString("D2")}";

            return $" <b><size=3.0><color=#FF0>⚠{firstWallText}({chrouchWallsCount})</color></size></b>";
        }
        public static float CheckForCrouchWalls(List<BeatmapSaveDataVersion3.BeatmapSaveData.ObstacleData> obstacles, ref int count)
        {
            if (obstacles == null || obstacles.Count == 0)
                return -1;

            var wallExistence = new float[2];
            float beat = -1;

            foreach (var o in obstacles)
            {
                // Ignore 1 wide walls on left
                if (o.line == 3 || (o.line == 0 && o.width == 1))
                    continue;

                // Filter out fake walls, they dont drain energy
                if (o.duration < 0 || o.width <= 0)
                    continue;

                // Detect >2 wide walls anywhere, or 2 wide wall in middle
                if (o.width > 2 || (o.width == 2 && o.line == 1))
                {
                    if (o.layer == 2 || o.layer != 0 && (o.height - o.layer >= 2))
                    {
                        count++;
                        if (beat == -1) beat = o.beat;
                        continue;
                    }
                }

                // Is the wall on the left or right half?
                var isLeftHalf = o.line <= 1;

                // Check if the other half has an active wall, which would mean there is one on both halfs
                // I know this technically does not check if one of the halves is half-height, but whatever
                if (wallExistence[isLeftHalf ? 1 : 0] >= o.beat)
                {
                    count++;
                    if (beat == -1) beat = o.beat;
                    continue;
                }

                // Extend wall lengths by 120ms so that staggered crouchwalls that dont overlap are caught
                wallExistence[isLeftHalf ? 0 : 1] = Math.Max(wallExistence[isLeftHalf ? 0 : 1], o.beat + o.duration + 0.12f);
            }
            return beat;
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
