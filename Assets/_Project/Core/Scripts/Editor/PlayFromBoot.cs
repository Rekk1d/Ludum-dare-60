using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class PlayFromBoot
{
    static PlayFromBoot()
    {
        var boot = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Project/Core/Scenes/Boot.unity");
        EditorSceneManager.playModeStartScene = boot;
    }
}