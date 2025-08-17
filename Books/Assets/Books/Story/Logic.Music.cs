using Cysharp.Threading.Tasks;
using Shared.Disposable;
using UniRx;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic
    {
        [Logic(LogicIdx.Music, LogicIdx.Музыка)]
        private async UniTask<bool> RunMusic(string header, string attributes, string body)
        {
            AudioClip audioClip = null;
            var audioClipDone = false;
            var audioClipPath = $"{_ctx.StoryPath}/Music/{body.Replace(" ", "_")}.mp3";

            _ctx.OnGetMusic.Where(data => data.fileName == audioClipPath).Subscribe(data =>
            {
                audioClip = data.clip;
                audioClipDone = true;
            }).AddTo(this);
            _ctx.GetMusic.Execute(audioClipPath);
            while (!audioClipDone) await UniTask.Yield();

            var musicGO = new GameObject("MusicOwner");
            var music = musicGO.AddComponent<AudioSource>();
            music.clip = audioClip;
            music.Play();

            return true;
        }
    }
}
