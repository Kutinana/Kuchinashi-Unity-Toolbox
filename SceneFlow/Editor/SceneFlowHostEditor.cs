using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kuchinashi.SceneFlow.Editor
{
    [CustomEditor(typeof(SceneFlowHost))]
    public sealed class SceneFlowHostEditor : UnityEditor.Editor
    {
        #region Constants

        private const string c_InspectorJumpSceneNameProperty = "m_InspectorJumpSceneName";

        #endregion

        #region Private Fields

        private SerializedProperty m_InspectorJumpSceneNameProperty;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            m_InspectorJumpSceneNameProperty = serializedObject.FindProperty(c_InspectorJumpSceneNameProperty);
        }

        #endregion

        #region Public Methods

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, c_InspectorJumpSceneNameProperty);
            DrawInspectorJumpControls();

            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region Private Methods

        private void DrawInspectorJumpControls()
        {
            var sceneNames = GetJumpableSceneNames();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Inspector Scene Jump", EditorStyles.boldLabel);

            if (sceneNames.Count == 0)
            {
                m_InspectorJumpSceneNameProperty.stringValue = string.Empty;
                EditorGUILayout.HelpBox("No jumpable scenes found in Build Settings.", MessageType.Warning);
                return;
            }

            var currentIndex = sceneNames.IndexOf(m_InspectorJumpSceneNameProperty.stringValue);
            if (currentIndex < 0)
            {
                currentIndex = 0;
                m_InspectorJumpSceneNameProperty.stringValue = sceneNames[currentIndex];
            }

            var selectedIndex = EditorGUILayout.Popup("Scene", currentIndex, sceneNames.ToArray());
            m_InspectorJumpSceneNameProperty.stringValue = sceneNames[selectedIndex];

            using (new EditorGUI.DisabledScope(!Application.isPlaying || string.IsNullOrEmpty(m_InspectorJumpSceneNameProperty.stringValue)))
            {
                if (GUILayout.Button("Jump To Scene"))
                {
                    serializedObject.ApplyModifiedProperties();

                    var host = (SceneFlowHost)target;
                    host.TryJumpToInspectorScene();
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to jump through SceneFlow.", MessageType.Info);
            }
        }

        private List<string> GetJumpableSceneNames()
        {
            var host = (SceneFlowHost)target;
            var shellSceneName = host.gameObject.scene.name;
            var sceneNames = new List<string>();

            var scenes = EditorBuildSettings.scenes;
            for (var i = 0; i < scenes.Length; i++)
            {
                var scene = scenes[i];
                if (!scene.enabled)
                {
                    continue;
                }

                var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                if (string.IsNullOrEmpty(sceneName) || sceneName == shellSceneName || sceneNames.Contains(sceneName))
                {
                    continue;
                }

                sceneNames.Add(sceneName);
            }

            return sceneNames;
        }

        #endregion
    }
}
