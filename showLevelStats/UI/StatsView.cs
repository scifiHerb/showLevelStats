using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

using static IPA.Logging.Logger;
using System.Configuration;
using IPA.Utilities;
using System.IO;

namespace showLevelStats.UI
{
    internal class StatsView
    {
        static public StatsView instance = new StatsView();

        public void Create(StandardLevelDetailViewController detailView)
        {
            if (root != null) return;

            var screen = GameObject.Find("ScreenContainer");
            BSMLParser.instance.Parse(
                Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), $"showLevelStats.UI.StatsView.bsml"),
                detailView.gameObject, instance
            );

            detailView.didActivateEvent += ResultsView_didActivateEvent;
            detailView.didDeactivateEvent += ResultsView_didDeactivateEvent;

            root.name = "showLevelStats";
            root.localPosition = detailView.transform.position + new Vector3(45.5F, -34.5F, 0);
            root.SetParent(screen.transform);

            var bsrTextClickable = bsrText.GetComponent<ClickableText>();
            bsrTextClickable.fontSize = 3;
            bsrTextClickable.paragraphSpacing = -30F;
            bsrTextClickable.text = "";
            bsrTextClickable.alignment = TextAlignmentOptions.Center;

        }
        private void ResultsView_didActivateEvent(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            //Plugin.Log.Info("Activate");
            root.gameObject.SetActive(true);
        }
        private void ResultsView_didDeactivateEvent(bool removedFromHierarchy, bool screenSystemDisabling)
        {
            //Plugin.Log.Info("DEACTIVATE");
            root.gameObject.SetActive(false);
        }


        public void setText(string  text)
        {
            bsrText.text = text;
        }
        [UIAction("onClick")]
        protected async Task onClick()
        {
            Plugin.Log.Info("onclick");
            LevelProfileView.instance.showDetail();
        }

        [UIComponent("root")]
        protected RectTransform root = null;
        [UIComponent("bsrText")]
        protected ClickableText bsrText = null;
    }
}
