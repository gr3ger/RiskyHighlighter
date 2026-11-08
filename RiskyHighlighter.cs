using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace RiskyHighlighter
{
    public class RiskyHighlighter : IMod
    {
        public static ModLog log;
        public static ConfigEntry<string> StolenColor;
        public static ConfigEntry<string> ContrabandColor;
        
        public void Init(ModManifest manifest)
        {
            log = new ModLog(manifest);
            ModConfig config = ModHelper.GetConfig(manifest.ID);
            StolenColor = config.Define("StolenColor", "#00FF0033", "General", () => "StolenColor");
            ContrabandColor = config.Define("ContrabandColor", "#FF00FF33", "General", () => "ContrabandColor");
        }

        public void OnEnable()
        {
        }

        public void OnDisable()
        {
        }

        public static Color GetStolenColor()
        {
            Color color;

            if (!ColorUtility.TryParseHtmlString(StolenColor.Value, out color))
            {
                ColorUtility.TryParseHtmlString(StolenColor.Default, out color);
            }
            return color;
        }
        
        public static Color GetContrabandColor()
        {
            Color color;
            if (!ColorUtility.TryParseHtmlString(ContrabandColor.Value, out color))
            {
                ColorUtility.TryParseHtmlString(ContrabandColor.Default, out color);
            }
            return color;
        }
    }
    
    [HarmonyPatch(typeof (GameItemElement), "Validate")]
    internal static class PatchGameItemElementValidate
    {
        [HarmonyPostfix]
        internal static void Postfix(GameItemElement __instance)
        {
            if(StolenHelper.IsStolenItem(__instance)){
                AddHighlight(__instance, RiskyHighlighter.GetStolenColor());
                return;
            }
            
            if(ContrabandHelper.IsContraband(__instance)){
                AddHighlight(__instance, RiskyHighlighter.GetContrabandColor());
                return;
            }
             
            RemoveHighlight(__instance);
        }

        private static void RemoveHighlight(GameItemElement instance)
        {
            var highlight = instance.background.rectTransform.parent.Find("RiskyHighlight");
            if (highlight != null) {
                Object.Destroy(highlight.gameObject);
            }
        }

        private static void AddHighlight(GameItemElement instance, Color color)
        {
            RectTransform orgItemRect = instance.background.rectTransform;
            if (orgItemRect.parent.Find("RiskyHighlight") != null)
                return;
            
            GameObject borderObject = new GameObject("RiskyHighlight", typeof(RectTransform), typeof(Image));
            borderObject.transform.SetParent(orgItemRect.parent, false);
            borderObject.transform.SetSiblingIndex(orgItemRect.GetSiblingIndex());
            Image borderImage = borderObject.GetComponent<Image>();
            borderImage.color = color;
            
            RectTransform borderRect = borderObject.GetComponent<RectTransform>();
            borderRect.anchorMin = orgItemRect.anchorMin;
            borderRect.anchorMax = orgItemRect.anchorMax;
            borderRect.pivot = orgItemRect.pivot;
            borderRect.anchoredPosition = orgItemRect.anchoredPosition;
            borderRect.sizeDelta = orgItemRect.sizeDelta;
        }
    }
}
