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
            
            GameObject highlightContainer = new GameObject("RiskyHighlight", typeof(RectTransform));
            highlightContainer.transform.SetParent(orgItemRect.parent, false);
            highlightContainer.transform.SetSiblingIndex(orgItemRect.GetSiblingIndex());

            RectTransform rootRect = highlightContainer.GetComponent<RectTransform>();
            rootRect.anchorMin = orgItemRect.anchorMin;
            rootRect.anchorMax = orgItemRect.anchorMax;
            rootRect.pivot = orgItemRect.pivot;
            rootRect.anchoredPosition = orgItemRect.anchoredPosition;
            rootRect.sizeDelta = orgItemRect.sizeDelta;
            
            var gridShape = instance.shape;
            float cellWidth = orgItemRect.rect.width / gridShape.width;
            float cellHeight = orgItemRect.rect.height / gridShape.height;
            
            for (int localY = 0; localY < gridShape.height; ++localY)
            {
                for (int localX = 0; localX < gridShape.width; ++localX)
                {
                    (int x, int y) = gridShape.ForwardTransform(localX, localY);
                    if (gridShape.GetLocal(localX, localY) == 0)
                        continue;
                    
                    GameObject cellObject = new GameObject("Cell", typeof(RectTransform), typeof(Image));
                    cellObject.transform.SetParent(highlightContainer.transform, false);

                    Image cellImage = cellObject.GetComponent<Image>();
                    cellImage.color = color;

                    RectTransform cellRect = cellObject.GetComponent<RectTransform>();
                    cellRect.anchorMin = new Vector2(0, 1);
                    cellRect.anchorMax = new Vector2(0, 1);
                    cellRect.pivot = new Vector2(0, 1);
                    cellRect.sizeDelta = new Vector2(cellWidth, cellHeight);
                    cellRect.anchoredPosition = new Vector2(x * cellWidth, -y * cellHeight);

                }
            }
        }
    }
}
