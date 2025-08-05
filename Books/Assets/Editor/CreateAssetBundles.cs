using UnityEditor;
using System.IO;
using UnityEngine;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        var remotePath = "Assets/StreamingAssets/Remote";
        if (!Directory.Exists(remotePath))
        {
            Directory.CreateDirectory(remotePath);
            Debug.Log($"Create remote folder {remotePath}");
        }

        var assetBundleDirectoryWebGL = $"{remotePath}/WebGL";
        if (!Directory.Exists(assetBundleDirectoryWebGL)) 
        {
            Directory.CreateDirectory(assetBundleDirectoryWebGL);
            Debug.Log($"Create remote folder {assetBundleDirectoryWebGL}");
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectoryWebGL, BuildAssetBundleOptions.None, BuildTarget.WebGL);

        var assetBundleDirectoryWin = $"{remotePath}/Win";
        if (!Directory.Exists(assetBundleDirectoryWin))
        {
            Directory.CreateDirectory(assetBundleDirectoryWin);
            Debug.Log($"Create remote folder {assetBundleDirectoryWin}");
        }
        BuildPipeline.BuildAssetBundles(assetBundleDirectoryWin, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}