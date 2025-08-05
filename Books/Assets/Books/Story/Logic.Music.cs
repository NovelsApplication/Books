using Cysharp.Threading.Tasks;
using Shared.LocalCache;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Music, LogicIdx.Музыка)]
        private async UniTask<bool> RunMusic(string header, string attributes, string body)
        {
            var audioClipName = $"{_ctx.RootFolderName}/Music/{body.Replace(" ", "_")}.mp3";

            Debug.Log(audioClipName);

            var audioClip = await Cacher.GetAudioClipAsync(audioClipName);

            var musicGO = new GameObject("MusicOwner");
            var music = musicGO.AddComponent<AudioSource>();
            music.clip = audioClip;
            music.Play();

            return true;
        }
    }
}
