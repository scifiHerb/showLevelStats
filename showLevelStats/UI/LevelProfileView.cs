using BeatSaberMarkupLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Profiling;
using UnityEngine;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using System.Runtime.Remoting.Contexts;
using BeatSaberMarkupLanguage.Components;
using TMPro;
using System.ComponentModel.Design.Serialization;
using HMUI;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static BeatmapLevelSO.GetBeatmapLevelDataResult;
using static IPA.Logging.Logger;
using showLevelStats;
using System.Windows.Forms;
using BeatSaberMarkupLanguage.Components.Settings;
using showLevelStats.HarmonyPatches;
using JetBrains.Annotations;

namespace showLevelStats.UI
{
    internal class LevelProfileView
    {
        static public LevelProfileView instance = new LevelProfileView();
        static private bool translateFlag = true;
        private string mapperDesc = "";
        private string songDesc = "";

        public static void Create(GameObject obj)
        {
            if (instance.root != null) return;

            BSMLParser.instance.Parse(
                Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), $"showLevelStats.UI.LevelProfileView.bsml"),
                obj, instance
            );

            instance.root.name = "levelProfile";
            instance.root.anchoredPosition = new Vector2(100, 100);
        }

        public void clearDetails()
        {
            if (artwork == null) return;
            songText.text = "";
            mapperText.text = "";
            description.text = "";
            mapperDescription.text = "";

        }
        public void setDetails(LevelInformation level)
        {
            if (artwork == null) return;
            //set 
            if (level.versions[0] != null) artwork.SetImage(level.versions[0].coverURL);
            songText.text = $"<color=#00ffff><size=200%>{level.metadata.songName}</size></color>\n";
            if (level.metadata.songSubName != "") songText.text += $"<color=#00bfff>{level.metadata.songSubName}</color>\n";
            songText.text += $"{level.metadata.songAuthorName} [<color=#00FF00>{level.metadata.levelAuthorName}</color>]\n" +
                $"BSR[<color=#00ff00>{level.id}</color>] - <color=#00ff00>{level.uploaded}</color>\n" +
                $"Votes[↑<color=#00ff00>{level.stats.upvotes}</color>:↓<color=#ff0000>{level.stats.downvotes}</color>]";

            description.text = level.description;
            songDesc = level.description;
        }

        public void setMapperDetails(MapperInformation.User mapper)
        {
            if (artwork == null) return;

            mapperText.text = $"<color=#00ffff><size=200%>{mapper.name}</size></color>\n" +
                $"totalVotes[<color=#00FF00>{mapper.stats.totalUpvotes}</color>,<color=#FF0000>{mapper.stats.totalDownvotes}</color>]\n" +
                $"totalMaps[<color=#00FF00>{mapper.stats.totalMaps}</color>]\n" +
                $"[<color=#00FF00>{mapper.stats.firstUpload}</color> - <color=#00FF00>{mapper.stats.lastUpload}</color>]";


            mapperDescription.text = mapper.description;
            mapperDesc = mapper.description;
            mapperIcon.SetImage(mapper.avatar);
        }

        public void showDetail()
        {
            //debug ui position settings
            parserParams?.EmitEvent("show-detail");
        }

        [UIAction("close-window")]
        protected async Task CloseAll()
        {
            parserParams?.EmitEvent("hide");
            await SiraUtil.Extras.Utilities.PauseChamp;
        }

        [UIAction("onClickSongArtwork")]
        protected async Task onClickSongArtwork()
        {
            parserParams?.EmitEvent("show-artwork");

            imageView.sprite = artwork.sprite;
        }

        [UIAction("onClickMapperArtwork")]
        protected async Task onClickMapperArtwork()
        {
            parserParams?.EmitEvent("show-artwork");

            imageView.sprite = mapperIcon.sprite;
        }

        [UIAction("close-submodal")]
        protected async Task onClickCloseImageView()
        {
            parserParams?.EmitEvent("hide");
            parserParams?.EmitEvent("show-detail");
            ToggleSetting a;
        }

        [UIAction("original")]
        protected async Task onOriginal()
        {
            description.text = LevelListTableCellSetDataFromLevel.levelInfo.description;
            mapperDescription.text = LevelListTableCellSetDataFromLevel.mapperInfo.description;
        }

        [UIAction("translate")]
        public void onTranslate()
        {
            //mapper description
            try
            {
                var w = new WebClient();
                var result = w.DownloadString($"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={Settings.Instance.translateLanguage}&dt=t&q={WebUtility.UrlEncode(mapperDesc)}");

                JArray jsonArray = JArray.Parse(result);

                mapperDescription.text = "";
                foreach (var i in jsonArray)
                {

                    for (int j = 0; j < i.Count() - 1; j++)
                    {
                        if (i[j] != null) mapperDescription.text += i[j][0].ToString();
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Info("エラー: " + ex.Message);

            }
            try
            {
                var w = new WebClient();
                var result = w.DownloadString($"https://translate.googleapis.com/translate_a/single?client=gtx&sl=auto&tl={Settings.Instance.translateLanguage}&dt=t&q={WebUtility.UrlEncode(songDesc)}");

                JArray jsonArray = JArray.Parse(result);

                description.text = "";
                foreach (var i in jsonArray)
                {

                    for (int j = 0; j < i.Count() - 1; j++)
                    {
                        if (i[j] != null) description.text += i[j][0].ToString();
                    }
                    break;
                }
            }
            catch (Exception ex)
            {

                Plugin.Log.Info("エラー: " + ex.Message);
            }  
        }

        [UIParams]
        protected BSMLParserParams parserParams = null;
        [UIComponent("main-modal")]
        protected RectTransform root = null;

        [UIComponent("artworkContainer")]
        protected RectTransform artworkContainer = null;


        [UIComponent("artwork")]
        protected ClickableImage artwork = null;
        [UIComponent("songText")]
        protected TextMeshProUGUI songText = null;
        [UIComponent("description")]
        protected TextMeshProUGUI description = null;

        [UIComponent("mapperText")]
        protected TextMeshProUGUI mapperText = null;
        [UIComponent("mapperIcon")]
        protected ClickableImage mapperIcon = null;
        [UIComponent("mapperDescription")]
        protected TextMeshProUGUI mapperDescription = null;

        [UIComponent("imageView")]
        protected ImageView imageView = null;
    }
}
