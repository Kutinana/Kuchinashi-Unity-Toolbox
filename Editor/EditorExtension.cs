# if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Kuchinashi.Editor
{
    public class EditorExtension : EditorWindow
    {
        [MenuItem ("Kuchinashi/Open Persistent Data Folder")]
        public static void OpenPersistentDataFolder()
        {

# if UNITY_EDITOR_WIN
            EditorUtility.RevealInFinder(Path.Combine(Application.persistentDataPath, Application.productName));
# elif UNITY_EDITOR_OSX
            EditorUtility.RevealInFinder(Application.persistentDataPath);
# endif

        }

        [MenuItem ("Kuchinashi/Delete Persistent Data Folder")]
        public static void DeletePersistentDataFolder()
        {
            Directory.Delete(Application.persistentDataPath, true);
        }

        [MenuItem ("Kuchinashi/Reset PlayerPrefs")]
        public static void ResetPlayerPrefs()
        {
            PlayerPrefs.DeleteAll();
        }
    }

}

#endif