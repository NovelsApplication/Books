using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Shared.Requests
{
    public class AudioRequest : BaseDisposable
    {
        public struct Ctx
        {
            public ReactiveCommand<(AudioClip audio, string audioPath)> OnGetAudio;
            public IObservable<string> GetAudio;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public AudioRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetAudio.Subscribe(path => GetAudio(path)).AddTo(this);
        }

        private async void GetAudio(string localPath)
        {
            using var request = _ctx.GetRequest.Invoke(localPath);
            var dh = (DownloadHandlerAudioClip)request.downloadHandler;
            dh.compressed = false;
            dh.streamAudio = false;
            request.downloadHandler = dh;
            await request.SendWebRequest();

            dh.audioClip.LoadAudioData();
            while (dh.audioClip.loadState != AudioDataLoadState.Loaded) await UniTask.Yield();

            _ctx.OnGetAudio.Execute((dh.audioClip, localPath));
        }
    }
}
