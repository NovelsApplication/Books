using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;

namespace Books.Story
{
    internal partial class Logic
    {
        private AudioSource _music;

        [Logic(LogicIdx.Music, LogicIdx.Музыка)]
        private async UniTask<bool> RunMusic(string header, string attributes, string body)
        {
            if (_music != null) _music.Stop();

            var audioClipPath = $"{_ctx.StoryPath}/Music/{body.Replace(" ", "_")}.mp3";

            var getMusicTask = new ReactiveProperty<Func<UniTask<AudioClip>>>().AddTo(this);
            _ctx.GetMusic.Execute((audioClipPath, getMusicTask));
            var audioClip = await getMusicTask.Value.Invoke();

            if (_music == null) 
            {
                var musicGO = new GameObject("MusicOwner");
                _music = musicGO.AddComponent<AudioSource>();
            }

            _music.clip = audioClip;
            _music.Play();

            return true;
        }
    }
}
