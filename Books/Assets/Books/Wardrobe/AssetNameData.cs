using UnityEngine;

namespace Books.Wardrobe
{
    [CreateAssetMenu(fileName = "ScriptableObjects/AssetNameData", menuName = "WardrobeAssetNameData")]
    public class AssetNameData : ScriptableObject
    {
        public string landLightDefaultBackgroundName = "Гардероб суша день";
        public string landDarkDefaultBackgroundName = "Гардероб суша ночь";
        public string waterLightDefaultBackgroundName = "Гардероб вода день";
        public string waterDarkDefaultBackgroundName = "Гардероб вода ночь";
        
        
    }
}