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
            public IObservable<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)> GetAudio;

            public Func<string, UnityWebRequest> GetRequest;
        }

        private readonly Ctx _ctx;

        public AudioRequest(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetAudio.Subscribe(data =>
            {
                data.task.Value = async () => await GetAudio(data.path);
            }).AddTo(this);
        }

        private async UniTask<AudioClip> GetAudio(string path)
        {
            using var request = _ctx.GetRequest.Invoke(path);
            var dh = (DownloadHandlerAudioClip)request.downloadHandler;
            dh.compressed = false;
            dh.streamAudio = false;
            request.downloadHandler = dh;
            await request.SendWebRequest();

            dh.audioClip.LoadAudioData();
            while (dh.audioClip.loadState != AudioDataLoadState.Loaded) await UniTask.Yield();

            return dh.audioClip;
        }
    }
}
