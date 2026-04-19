#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEditor.PackageManager;

namespace Kuchinashi.Editor
{
    /// <summary>
    /// 根据 Packages/manifest.json 是否包含 Newtonsoft 包，同步脚本宏
    /// <see cref="KUCHINASHI_TOOLBOX_NEWTONSOFT_JSON"/>，从而在 Common 下三个脚本中启用/跳过 Newtonsoft 相关编译单元。
    /// </summary>
    public static class ToolboxNewtonsoftDefineSync
    {
        public const string NewtonsoftDefine = "KUCHINASHI_TOOLBOX_NEWTONSOFT_JSON";
        public const string NewtonsoftPackageId = "com.unity.nuget.newtonsoft-json";
        public const string CursorGitUrl = "https://github.com/boxqkrtm/com.unity.ide.cursor.git";
        public static readonly string[] CursorManifestKeys = { "com.boxqkrtm.ide.cursor", "com.unity.ide.cursor" };

        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            SyncNewtonsoftDefineWithManifest();
        }

        public static bool IsNewtonsoftJsonListedInManifest()
        {
            try
            {
                var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                if (!File.Exists(manifestPath)) return false;
                var text = File.ReadAllText(manifestPath);
                return text.IndexOf(NewtonsoftPackageId, StringComparison.Ordinal) >= 0;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsCursorIdePackageListedInManifest()
        {
            try
            {
                var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
                if (!File.Exists(manifestPath)) return false;
                var text = File.ReadAllText(manifestPath);
                if (text.IndexOf(CursorGitUrl, StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
                foreach (var key in CursorManifestKeys)
                {
                    if (text.IndexOf("\"" + key + "\"", StringComparison.Ordinal) >= 0)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static void SyncNewtonsoftDefineWithManifest()
        {
            var wantDefine = IsNewtonsoftJsonListedInManifest();
            foreach (BuildTargetGroup group in Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (group == BuildTargetGroup.Unknown) continue;
                try
                {
                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                    var set = new HashSet<string>(
                        defines.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                        StringComparer.Ordinal);
                    if (wantDefine) set.Add(NewtonsoftDefine);
                    else set.Remove(NewtonsoftDefine);
                    var merged = string.Join(";", set.OrderBy(s => s, StringComparer.Ordinal));
                    if (merged != defines)
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, merged);
                }
                catch
                {
                    // 部分 BuildTargetGroup 在当前 Unity 版本下不可用（例如未安装对应模块），忽略即可。
                }
            }
        }
    }

    internal sealed class ToolboxManifestWatcher : AssetPostprocessor
    {
        const string ToolboxAssetPathToken = "Kuchinashi-Unity-Toolbox";

        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (path == null) continue;
                var n = path.Replace('\\', '/');
                if (n.EndsWith("Packages/manifest.json", StringComparison.OrdinalIgnoreCase))
                    ToolboxNewtonsoftDefineSync.SyncNewtonsoftDefineWithManifest();
                if (n.IndexOf(ToolboxAssetPathToken, StringComparison.OrdinalIgnoreCase) >= 0)
                    ToolboxDependencyAutoPrompt.ScheduleTryAfterImport();
            }
        }
    }

    /// <summary>
    /// 首次检测到依赖缺失时自动打开依赖安装窗口（每个 Unity 项目最多一次）。
    /// </summary>
    static class ToolboxDependencyAutoPrompt
    {
        const string EditorPrefKeyPrefix = "Kuchinashi.Toolbox.AutoDepsPromptDone.";

        [InitializeOnLoadMethod]
        static void OnProjectLoadedInEditor()
        {
            EditorApplication.delayCall += TryAutoShowOnce;
        }

        internal static void ScheduleTryAfterImport()
        {
            EditorApplication.delayCall += TryAutoShowOnce;
        }

        static string ProjectPromptEditorPrefKey()
        {
            return EditorPrefKeyPrefix + PlayerSettings.productGUID.ToString();
        }

        internal static void TryAutoShowOnce()
        {
            if (Application.isBatchMode) return;

            var prefKey = ProjectPromptEditorPrefKey();
            if (EditorPrefs.GetBool(prefKey, false)) return;

            var needNewtonsoft = !ToolboxNewtonsoftDefineSync.IsNewtonsoftJsonListedInManifest();
            var needCursor = !ToolboxNewtonsoftDefineSync.IsCursorIdePackageListedInManifest();
            if (!needNewtonsoft && !needCursor)
            {
                EditorPrefs.SetBool(prefKey, true);
                return;
            }

            EditorPrefs.SetBool(prefKey, true);
            EditorExtension.OpenToolboxPackagesWindow();
        }
    }

    public class EditorExtension : EditorWindow
    {
        const string WindowTitle = "Kuchinashi Toolbox Dependencies Installer";
        static AddRequest _addRequest;
        static Action _addRequestContinuation;
        static PendingInstallKind _pendingInstallKind;

        [MenuItem("Kuchinashi/Open Persistent Data Folder", priority = 0)]
        public static void OpenPersistentDataFolder()
        {
#if UNITY_EDITOR_WIN
            EditorUtility.RevealInFinder(Path.Combine(Application.persistentDataPath, Application.productName));
#elif UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(Application.persistentDataPath);
#endif
        }

        [MenuItem("Kuchinashi/Delete Persistent Data Folder", priority = 1)]
        public static void DeletePersistentDataFolder()
        {
            Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem("Kuchinashi/Reset PlayerPrefs", priority = 2)]
        public static void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Kuchinashi/Dependencies…", priority = 3)]
        public static void OpenToolboxPackagesWindow()
        {
            var w = GetWindow<EditorExtension>(false, WindowTitle, true);
            w.minSize = new Vector2(420, 160);
        }

        void OnEnable()
        {
            ToolboxNewtonsoftDefineSync.SyncNewtonsoftDefineWithManifest();
        }

        void OnGUI()
        {
            EditorGUILayout.Space(8);
            DrawPackageRow(
                "Newtonsoft.Json",
                ToolboxNewtonsoftDefineSync.IsNewtonsoftJsonListedInManifest(),
                PendingInstallKind.NewtonsoftJson,
                InstallNewtonsoftJson);

            EditorGUILayout.Space(6);
            DrawPackageRow(
                "Cursor Unity IDE",
                ToolboxNewtonsoftDefineSync.IsCursorIdePackageListedInManifest(),
                PendingInstallKind.CursorIde,
                InstallCursorIde);
        }

        enum PendingInstallKind
        {
            None = 0,
            NewtonsoftJson = 1,
            CursorIde = 2,
        }

        static void DrawPackageRow(string label, bool installed, PendingInstallKind rowKind, Action onInstall)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, installed ? "Installed" : "Not Installed", EditorStyles.boldLabel);
                var installingThis = _addRequest != null && _pendingInstallKind == rowKind;
                var installingAny = _addRequest != null;
                var buttonLabel = installingThis ? "Installing..." : "Install";
                using (new EditorGUI.DisabledScope(installed || installingAny))
                {
                    if (GUILayout.Button(buttonLabel, GUILayout.Width(100)))
                    {
                        if (!installed && !installingAny)
                            onInstall();
                    }
                }
            }
        }

        static void InstallNewtonsoftJson()
        {
            StartAddRequest(Client.Add(ToolboxNewtonsoftDefineSync.NewtonsoftPackageId), () =>
            {
                ToolboxNewtonsoftDefineSync.SyncNewtonsoftDefineWithManifest();
                AssetDatabase.Refresh();
            }, PendingInstallKind.NewtonsoftJson);
        }

        static void InstallCursorIde()
        {
            StartAddRequest(Client.Add(ToolboxNewtonsoftDefineSync.CursorGitUrl), () => { AssetDatabase.Refresh(); },
                PendingInstallKind.CursorIde);
        }

        static void StartAddRequest(AddRequest request, Action onSuccess, PendingInstallKind kind)
        {
            _addRequest = request;
            _addRequestContinuation = onSuccess;
            _pendingInstallKind = kind;
            EditorApplication.update += PollAddRequestStatic;
        }

        static void PollAddRequestStatic()
        {
            if (_addRequest == null) return;
            if (!_addRequest.IsCompleted)
            {
                foreach (var w in Resources.FindObjectsOfTypeAll<EditorExtension>())
                    w.Repaint();
                return;
            }

            EditorApplication.update -= PollAddRequestStatic;
            var req = _addRequest;
            var cont = _addRequestContinuation;
            _addRequest = null;
            _addRequestContinuation = null;
            _pendingInstallKind = PendingInstallKind.None;
            if (req.Status == StatusCode.Success)
            {
                cont?.Invoke();
                if (req.Result != null)
                    Debug.Log("[Kuchinashi] Package installed successfully: " + req.Result.name);
            }
            else if (!string.IsNullOrEmpty(req.Error?.message))
                Debug.LogError("[Kuchinashi] Package installation failed: " + req.Error.message);
            foreach (var w in Resources.FindObjectsOfTypeAll<EditorExtension>())
                w.Repaint();
        }
    }
}

#endif
