#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEditor.PackageManager;
using UnityEngine.Networking;

namespace Kuchinashi.Editor
{
    public static class ToolboxQFrameworkBootstrap
    {
        public const string UnityPackageUrl =
            "https://file.liangxiegame.com/Frameworkv1_0_245_Release4f90b1a4_f500_4fea_a1c1_6c3846dff4de.unitypackage";

        public static string CachedUnityPackagePath()
        {
            var dir = Path.Combine(Application.temporaryCachePath, "Kuchinashi");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir,
                "Frameworkv1_0_245_Release4f90b1a4_f500_4fea_a1c1_6c3846dff4de.unitypackage");
        }

        public static bool IsInstalled()
        {
            var dir = Path.Combine(Application.dataPath, "QFramework");
            if (Directory.Exists(dir))
            {
                try
                {
                    return Directory.GetFiles(dir, "*", SearchOption.AllDirectories).Length > 0;
                }
                catch
                {
                    return true;
                }
            }

            return File.Exists(Path.Combine(Application.dataPath, "QFramework.cs"));
        }
    }

    /// <summary>
    /// 根据 Packages/manifest.json 是否包含 Newtonsoft 包，同步脚本宏
    /// <see cref="KUCHINASHI_TOOLBOX_NEWTONSOFT_JSON"/>，从而在 Common 下三个脚本中启用/跳过 Newtonsoft 相关编译单元。
    /// </summary>
    public static class ToolboxNewtonsoftDefineSync
    {
        public const string NewtonsoftDefine = "KUCHINASHI_TOOLBOX_NEWTONSOFT_JSON";
        public const string NewtonsoftPackageId = "com.unity.nuget.newtonsoft-json";
        /// <summary>用于检测 manifest 是否已引用 Cursor 包（无 #revision，兼容旧 manifest）。</summary>
        public const string CursorGitUrl = "https://github.com/boxqkrtm/com.unity.ide.cursor.git";
        /// <summary>与 packages-lock 中 hash 一致；安装时使用可减轻 Git 解析默认分支的等待。</summary>
        public const string CursorGitUrlInstall = CursorGitUrl + "#fd8ff1fb700caf062eb5cce364c1d0a85f99ebe6";
        /// <summary>写入 manifest 时与项目当前 lock 对齐的 Newtonsoft 版本。</summary>
        public const string NewtonsoftManifestVersion = "3.2.2";
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
        static bool _manifestBatchBusy;
        static UnityWebRequest _qFrameworkWebRequest;

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
            w.minSize = new Vector2(420, 220);
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

            EditorGUILayout.Space(6);
            DrawPackageRow(
                "QFramework",
                ToolboxQFrameworkBootstrap.IsInstalled(),
                PendingInstallKind.QFramework,
                InstallQFrameworkFromUpstream);

            var needNewtonsoft = !ToolboxNewtonsoftDefineSync.IsNewtonsoftJsonListedInManifest();
            var needCursor = !ToolboxNewtonsoftDefineSync.IsCursorIdePackageListedInManifest();
            var needQFramework = !ToolboxQFrameworkBootstrap.IsInstalled();
            if (needNewtonsoft || needCursor || needQFramework)
            {
                EditorGUILayout.Space(10);
                using (new EditorGUI.DisabledScope(_addRequest != null || _manifestBatchBusy ||
                                                  _qFrameworkWebRequest != null))
                {
                    if (GUILayout.Button("Install all missing packages", GUILayout.Height(26)))
                        TryInstallAllMissingViaManifest();
                }
            }
        }

        enum PendingInstallKind
        {
            None = 0,
            NewtonsoftJson = 1,
            CursorIde = 2,
            QFramework = 3,
        }

        static void DrawPackageRow(string label, bool installed, PendingInstallKind rowKind, Action onInstall)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, installed ? "Installed" : "Not Installed", EditorStyles.boldLabel);
                var installingThis = (_addRequest != null && _pendingInstallKind == rowKind) ||
                    (_qFrameworkWebRequest != null && rowKind == PendingInstallKind.QFramework);
                var installingAny = _addRequest != null || _manifestBatchBusy || _qFrameworkWebRequest != null;
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
            StartAddRequest(Client.Add(ToolboxNewtonsoftDefineSync.CursorGitUrlInstall), () => { AssetDatabase.Refresh(); },
                PendingInstallKind.CursorIde);
        }

        static void InstallQFrameworkFromUpstream()
        {
            if (ToolboxQFrameworkBootstrap.IsInstalled() || _qFrameworkWebRequest != null ||
                _addRequest != null || _manifestBatchBusy)
                return;
            var cachePath = ToolboxQFrameworkBootstrap.CachedUnityPackagePath();
            try
            {
                if (File.Exists(cachePath))
                    File.Delete(cachePath);
            }
            catch
            {
                // ignore
            }

            _qFrameworkWebRequest = UnityWebRequest.Get(ToolboxQFrameworkBootstrap.UnityPackageUrl);
            _qFrameworkWebRequest.downloadHandler = new DownloadHandlerFile(cachePath);
            _pendingInstallKind = PendingInstallKind.QFramework;
            _qFrameworkWebRequest.SendWebRequest();
            EditorApplication.update += PollQFrameworkWebRequest;
        }

        static void PollQFrameworkWebRequest()
        {
            if (_qFrameworkWebRequest == null)
                return;
            if (!_qFrameworkWebRequest.isDone)
            {
                foreach (var w in Resources.FindObjectsOfTypeAll<EditorExtension>())
                    w.Repaint();
                return;
            }

            EditorApplication.update -= PollQFrameworkWebRequest;
            var req = _qFrameworkWebRequest;
            _qFrameworkWebRequest = null;
            _pendingInstallKind = PendingInstallKind.None;

            var cachePath = ToolboxQFrameworkBootstrap.CachedUnityPackagePath();
            var ok = false;
            try
            {
                ok = req.result == UnityWebRequest.Result.Success;
            }
            finally
            {
                req.Dispose();
            }

            long length = 0;
            try
            {
                if (File.Exists(cachePath))
                    length = new FileInfo(cachePath).Length;
            }
            catch
            {
                // ignore
            }

            if (!ok || length < 64 * 1024)
            {
                Debug.LogError("[Kuchinashi] QFramework .unitypackage download failed or file too small.");
                try
                {
                    if (File.Exists(cachePath))
                        File.Delete(cachePath);
                }
                catch
                {
                    // ignore
                }

                foreach (var w in Resources.FindObjectsOfTypeAll<EditorExtension>())
                    w.Repaint();
                return;
            }

            try
            {
                AssetDatabase.ImportPackage(cachePath, true);
                AssetDatabase.Refresh();
                Debug.Log("[Kuchinashi] QFramework .unitypackage downloaded; use the Import Package window to finish (" + cachePath + ").");
            }
            catch (Exception e)
            {
                Debug.LogError("[Kuchinashi] QFramework ImportPackage failed: " + e.Message);
                try
                {
                    if (File.Exists(cachePath))
                        File.Delete(cachePath);
                }
                catch
                {
                    // ignore
                }
            }

            foreach (var w in Resources.FindObjectsOfTypeAll<EditorExtension>())
                w.Repaint();
        }

        static void TryStartQFrameworkBootstrapIfMissing()
        {
            if (ToolboxQFrameworkBootstrap.IsInstalled() || _qFrameworkWebRequest != null)
                return;
            InstallQFrameworkFromUpstream();
        }

        static void TryInstallAllMissingViaManifest()
        {
            var manifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            if (!File.Exists(manifestPath))
            {
                Debug.LogError("[Kuchinashi] manifest.json not found: " + manifestPath);
                return;
            }

            string text;
            try
            {
                text = File.ReadAllText(manifestPath);
            }
            catch (Exception e)
            {
                Debug.LogError("[Kuchinashi] Failed to read manifest: " + e.Message);
                return;
            }

            var changed = false;
            if (!ToolboxNewtonsoftDefineSync.IsNewtonsoftJsonListedInManifest())
                changed |= TryInjectManifestDependency(ref text, ToolboxNewtonsoftDefineSync.NewtonsoftPackageId,
                    ToolboxNewtonsoftDefineSync.NewtonsoftManifestVersion);
            if (!ToolboxNewtonsoftDefineSync.IsCursorIdePackageListedInManifest())
                changed |= TryInjectManifestDependency(ref text, "com.boxqkrtm.ide.cursor",
                    ToolboxNewtonsoftDefineSync.CursorGitUrlInstall);

            if (!changed)
            {
                Debug.Log("[Kuchinashi] No manifest changes needed (dependencies already listed).");
                TryStartQFrameworkBootstrapIfMissing();
                return;
            }

            try
            {
                File.WriteAllText(manifestPath, text);
            }
            catch (Exception e)
            {
                Debug.LogError("[Kuchinashi] Failed to write manifest: " + e.Message);
                return;
            }

            _manifestBatchBusy = true;
            AssetDatabase.Refresh();
            Client.Resolve();
            EditorApplication.delayCall += EndManifestBatchBusy;
        }

        static void EndManifestBatchBusy()
        {
            _manifestBatchBusy = false;
            ToolboxNewtonsoftDefineSync.SyncNewtonsoftDefineWithManifest();
            AssetDatabase.Refresh();
            foreach (var w in Resources.FindObjectsOfTypeAll<EditorExtension>())
                w.Repaint();
            Debug.Log("[Kuchinashi] Manifest updated; package resolve has been requested.");
            TryStartQFrameworkBootstrapIfMissing();
        }

        /// <summary>
        /// 在 "dependencies": { 之后插入一行。仅适用于标准 Unity manifest 结构。
        /// </summary>
        static bool TryInjectManifestDependency(ref string manifest, string packageKey, string value)
        {
            if (manifest.IndexOf("\"" + packageKey + "\"", StringComparison.Ordinal) >= 0)
                return false;

            const string marker = "\"dependencies\"";
            var m = manifest.IndexOf(marker, StringComparison.Ordinal);
            if (m < 0) return false;
            var brace = manifest.IndexOf('{', m);
            if (brace < 0) return false;

            var insert = brace + 1;
            var line = "\n    \"" + packageKey + "\": \"" + value + "\",";
            manifest = manifest.Insert(insert, line);
            return true;
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
