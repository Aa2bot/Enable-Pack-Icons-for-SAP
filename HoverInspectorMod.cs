using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(HoverInspector.HoverInspectorMod), "Hover Inspector", "3.4.0", "you")]
[assembly: MelonGame(null, "Super Auto Pets")]

namespace HoverInspector
{
    /// <summary>
    /// Always shows pack UI on Scoreboard and ScoreboardMini.
    /// - Leaves other UI untouched.
    /// - Scene load/F6/every frame: activates parents, enables graphics, sets alpha=1 for pack subpaths.
    /// </summary>
    public class HoverInspectorMod : MelonMod
    {
        internal const string EntryNamePrefix = "ScoreboardEntry(Clone)";
        internal static readonly string[] PackSubpaths =
        {
            "Content/Pack/Standard",
            "Content/Pack/Standard/Image"
        };

        internal static readonly string[] EntryRoots =
        {
            "Build/Hangar/ScoreboardMini/Canvas/NotchPadding/Layout/Content/Scroll View/Viewport/Content/Entries",
            "Build/Hangar/Scoreboard/Canvas/NotchPadding/Layout/Content/Scroll View/Viewport/Content/Entries"
        };

        public override void OnInitializeMelon()
        {
            PackRevealer.RevealPackIcons(false);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName) => PackRevealer.RevealPackIcons(false);

        // Mirror EnableMiniLives: keep targets active every frame so the game can't re-hide them.
        public override void OnUpdate()
        {
            PackRevealer.RevealPackIcons(false);
            if (Input.GetKeyDown(KeyCode.F6))
                PackRevealer.RevealPackIcons(true);
        }

        public override void OnLateUpdate() => PackRevealer.RevealPackIcons(false);
    }

    [HarmonyPatch(typeof(UnityWebRequest), "SendWebRequest")]
    internal static class UnityWebRequest_SendWebRequest_Patch
    {
        private static void Prefix(UnityWebRequest __instance)
        {
            if (__instance == null || string.IsNullOrEmpty(__instance.url))
                return;

            MelonLogger.Msg($"[API] {__instance.method} {__instance.url}");
        }
    }

    internal static class PackRevealer
    {
        public static void RevealPackIcons(bool logShown)
        {
            int shown = 0;

            foreach (var rootPath in HoverInspectorMod.EntryRoots)
            {
                var root = GameObject.Find(rootPath);
                if (root == null || !root.activeInHierarchy)
                    continue;

                var entries = root.transform;
                bool skipFirst = true; // always skip presumed local player (first entry)
                for (int i = 0; i < entries.childCount; i++)
                {
                    var child = entries.GetChild(i);
                    if (child == null || !child.name.StartsWith(HoverInspectorMod.EntryNamePrefix))
                        continue;

                    if (skipFirst && i == 0)
                        continue;

                    foreach (var sub in HoverInspectorMod.PackSubpaths)
                    {
                        var target = child.Find(sub);
                        if (target == null) continue;

                        var go = target.gameObject;
                        ActivateParents(go.transform);
                        EnableGraphics(go);

                        shown++;
                        if (logShown)
                            MelonLogger.Msg($"[HoverInspector] Shown: {BuildPath(target)}");
                    }
                }
            }

            if (logShown)
                MelonLogger.Msg($"[HoverInspector] Total pack graphics shown: {shown}");
        }

        private static void EnableGraphics(GameObject root)
        {
            var graphics = root.GetComponentsInChildren<Graphic>(true);
            foreach (var g in graphics)
            {
                if (g == null) continue;
                g.enabled = true;
                var c = g.color;
                if (c.a < 1f) { c.a = 1f; g.color = c; }
                g.canvasRenderer?.SetAlpha(1f);
            }

            var groups = root.GetComponentsInParent<CanvasGroup>(true);
            foreach (var cg in groups)
            {
                if (cg == null) continue;
                if (cg.alpha < 1f) cg.alpha = 1f;
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }

        private static void ActivateParents(Transform t)
        {
            while (t != null)
            {
                if (!t.gameObject.activeSelf)
                    t.gameObject.SetActive(true);
                t = t.parent;
            }
        }

        private static string BuildPath(Transform t)
        {
            var sb = new StringBuilder(128);
            var stack = new System.Collections.Generic.Stack<string>();
            while (t != null)
            {
                stack.Push(t.name);
                t = t.parent;
            }
            while (stack.Count > 0)
            {
                sb.Append('/');
                sb.Append(stack.Pop());
            }
            return sb.ToString();
        }
    }
}
