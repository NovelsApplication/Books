using Books.Wardrobe.AssetsMeta;
using UnityEngine;
using UnityEngine.Video;

namespace Books.Wardrobe.ViewModel
{
    public class LocationAssetModel
    {
        public LocationMetadata Metadata { get; }
        public Texture2D LocationImage { get; }
        public VideoClip Video { get; }

        public LocationAssetModel(LocationMetadata metadata, Texture2D image, VideoClip video)
        {
            Metadata = metadata;
            LocationImage = image;
            Video = video;
        }
    }
}