using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class AutoSave
{
    static double nextSave;
    const double saveInterval = 100; // 3 минуты

    static AutoSave()
    {
        nextSave = EditorApplication.timeSinceStartup + saveInterval;
        EditorApplication.update += Update;
    }

    static void Update()
    {
        if (EditorApplication.timeSinceStartup > nextSave)
        {
            if (!EditorApplication.isPlaying)
            {
                Debug.Log("Auto-saving scene...");
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();
            }
            nextSave = EditorApplication.timeSinceStartup + saveInterval;
        }
    }
}