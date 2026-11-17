using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AstravionBuildValidation : IPreprocessBuildWithReport
{
    private const string MainScene = "Assets/Scenes/Astravion.unity";
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScene) == null)
            throw new BuildFailedException("Scene ASTRAVION tidak ditemukan: " + MainScene);

        var scenes = EditorBuildSettings.scenes;
        if (scenes == null || !scenes.Any(s => s.enabled && s.path == MainScene))
            throw new BuildFailedException("Scene ASTRAVION belum diaktifkan pada Build Settings: " + MainScene);
    }

    [MenuItem("ASTRAVION/Validate Android Build Scene")]
    public static void ValidateScene()
    {
        bool exists = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScene) != null;
        bool enabled = EditorBuildSettings.scenes != null &&
                       EditorBuildSettings.scenes.Any(s => s.enabled && s.path == MainScene);
        Debug.Log("ASTRAVION Scene exists: " + exists + " | enabled in Build Settings: " + enabled);
    }
}
