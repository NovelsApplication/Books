using UnityEditor;
using System.IO;
using UnityEngine;

public class CreateAssetBundles
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        var assetBundleDirectory = "Assets/StreamingAssets/Remote";
        if (!Directory.Exists(assetBundleDirectory)) 
        {
            Debug.LogError("Create remote folder");
            return;
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None, BuildTarget.WebGL);
    }
}