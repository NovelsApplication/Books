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
            return true;

            var audioClipName = $"{_ctx.StoryPath}/Music/{body.Replace(" ", "_")}.mp3";

            Debug.Log($"{audioClipName} start {audioClipName}");

            var audioClip = await Cacher.GetAudioClipAsync(audioClipName);

            var musicGO = new GameObject("MusicOwner");
            var music = musicGO.AddComponent<AudioSource>();
            music.clip = audioClip;
            music.Play();

            Debug.Log($"{audioClipName} play {music.clip.name} {music.clip.length}");

            return true;
        }
    }
}
