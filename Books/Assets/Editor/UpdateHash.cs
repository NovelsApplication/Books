using UnityEditor;
using System.IO;
using UnityEngine;
using System.Linq;

public class UpdateHash
{
    [MenuItem("Assets/UpdateHash")]
    private static void CreateOrUpdateHash() 
    {
        var streamingAssetsPath = Application.streamingAssetsPath;

        var files = Directory.GetFiles(streamingAssetsPath, "*", SearchOption.AllDirectories)
            .Where(p => !p.EndsWith(".meta"))
            .Where(p => !p.EndsWith(".manifest"))
            .ToDictionary(f => f.Replace($"{streamingAssetsPath}\\", ""), f => File.ReadAllBytes(f).GetHashCode());

        Debug.Log($"{string.Join("\n", files.Select(f => $"{f.Value}-{f.Key}"))}");
    }
}
