
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class AstravionBuildValidation : IPreprocessBuildWithReport
{
    public int callbackOrder => -1000;

    public void OnPreprocessBuild(BuildReport report)
    {
        const string scenePath = "Assets/Scenes/Astravion.unity";

        SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

        if (scene == null)
        {
            throw new BuildFailedException(
                "Scene ASTRAVION tidak ditemukan: " + scenePath
            );
        }

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };

        Debug.Log("ASTRAVION: Scene berhasil dimasukkan ke Build Settings.");
    }
}
