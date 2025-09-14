using Cysharp.Threading.Tasks;
using Shared.Disposable;
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

            AudioClip audioClip = null;
            var audioClipDone = false;
            var audioClipPath = $"{_ctx.StoryPath}/Music/{body.Replace(" ", "_")}.mp3";

            var onGetMusicDisp = _ctx.OnGetMusic.Where(data => data.fileName == audioClipPath).Subscribe(data =>
            {
                audioClip = data.clip;
                audioClipDone = true;
            });
            _ctx.GetMusic.Execute(audioClipPath);
            while (!audioClipDone) await UniTask.Yield();
            onGetMusicDisp.Dispose();

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
