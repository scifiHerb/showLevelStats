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

namespace showLevelStats.HarmonyPatches
{
    [HarmonyPatch(typeof(StandardLevelDetailView))]
    [HarmonyPatch("SetContent", MethodType.Normal)]
    internal class LevelListTableCellSetDataFromLevel
    {
        public static CurvedTextMeshPro textMesh;

        private static void Postfix(IBeatmapLevel level, BeatmapDifficulty defaultDifficulty, BeatmapCharacteristicSO defaultBeatmapCharacteristic, PlayerData playerData, TextMeshProUGUI ____levelParamsPanel)
        {
            Debug.Log(level.songName + ":" + level.levelID);

            //
            if (textMesh == null) textMesh = new GameObject("Text").AddComponent<CurvedTextMeshPro>();
            textMesh.transform.SetParent(____levelParamsPanel.transform);
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.transform.position = new Vector3(2.3F, 0.5F, 3.7F);
            textMesh.transform.eulerAngles = new Vector3(0, 26, 0);
            textMesh.color = Color.white;
            textMesh.fontSize = 0.1f;
            textMesh.text = "";
            
            //---------------

            //カスタム曲出ない場合return
            if (level.levelID.IndexOf("custom_level") == -1) return;

            string url = "https://api.beatsaver.com/maps/hash/" + level.levelID.Substring(13);


            GetSongStats(url, textMesh);
        }

        private static async void GetSongStats(string url, CurvedTextMeshPro text)
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url); // 非同期にWebリクエストを送信する

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync(); // 非同期にレスポンスを読み込む

                LevelInfomation data = JsonConvert.DeserializeObject<LevelInfomation>(result);
                text.text = "\n\n\n" + data.id + "(↑<color=#00ff00>" + data.stats.upvotes +
                    "</color>:↓<color=#ff0000>" + data.stats.downvotes + "</color>)\n"+
                    data.uploaded;
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
