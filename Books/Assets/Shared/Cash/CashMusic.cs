using Cysharp.Threading.Tasks;
using Shared.Disposable;
using System;
using System.IO;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Shared.Cash
{
    internal sealed class CashMusic : BaseDisposable
    {
        [Serializable]
        public struct AudioClipData
        {
            public string Name;
            public int Channels;
            public int Frequency;
            public float[] Samples;
        }

        public struct Ctx
        {
            public IObservable<(AudioClip clip, string audioPath)> OnGetMusicRequest;
            public ReactiveCommand<string> GetMusicRequest;

            public ReactiveCommand<(AudioClip clip, string fileName)> OnGetMusic;
            public IObservable<string> GetMusic;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;

            public Func<string, string> ConvertPath;
        }

        private readonly Ctx _ctx;

        public CashMusic(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetMusic.Subscribe(async fileName => await GetMusicAsync(fileName)).AddTo(this);
        }

        private async UniTask GetMusicAsync(string fileName)
        {
            if (_ctx.IsCashed.Invoke(fileName))
            { 
                _ctx.OnGetMusic.Execute((AudioClipFromCache(fileName), fileName)); 
            }
            else
            {
                var audioRequestDone = false;
                AudioClip clip = null;
                var disposable = _ctx.OnGetMusicRequest.Where(data => fileName == data.audioPath).Subscribe(data =>
                {
                    clip = data.clip;
                    audioRequestDone = true;
                });
                _ctx.GetMusicRequest.Execute(fileName);
                while (!audioRequestDone) await UniTask.Yield();
                disposable.Dispose();

                _ctx.OnGetMusic.Execute((AudioClipToCache(clip, fileName), fileName)); 
            }
        }

        private AudioClip AudioClipFromCache(string fileName)
        {
            using (Stream stream = File.Open(_ctx.ConvertPath.Invoke(fileName), FileMode.Open))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var audioData = (AudioClipData)binaryFormatter.Deserialize(stream);

                var audioClip = AudioClip.Create(audioData.Name, audioData.Samples.Length, audioData.Channels, audioData.Frequency, false);
                audioClip.SetData(audioData.Samples, 0);

                return audioClip;
            }
        }

        private AudioClip AudioClipToCache(AudioClip data, string fileName)
        {
            var samples = new float[data.samples];
            data.LoadAudioData();
            data.GetData(samples, 0);

            var rawData = new AudioClipData
            {
                Name = fileName.Split("/").Last(),
                Channels = data.channels,
                Frequency = data.frequency,
                Samples = samples,
            };

            var file = _ctx.ConvertPath.Invoke(fileName);
            if (File.Exists(file))
                File.Delete(file);

            Debug.Log($"Save audio to: {file}");

            using (var stream = File.Open(file, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, rawData);
            }

            return data;
        }
    }
}

