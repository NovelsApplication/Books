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
            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)> GetMusicRequest;
            public IObservable<(string path, ReactiveProperty<Func<UniTask<AudioClip>>> task)> GetMusic;

            public Func<string, bool> IsCashed;

            public Func<string, byte[]> FromCash;
            public Action<byte[], string> ToCash;

            public Func<string, string> ConvertPath;
        }

        private readonly Ctx _ctx;

        public CashMusic(Ctx ctx)
        {
            _ctx = ctx;

            _ctx.GetMusic.Subscribe(data =>
            {
                data.task.Value = async () => await GetMusicAsync(data.path);
            }).AddTo(this);
        }

        private async UniTask<AudioClip> GetMusicAsync(string path)
        {
            if (_ctx.IsCashed.Invoke(path))
            {
                var result = AudioClipFromCache(path);
                return result;
            }
            else
            {
                var task = new ReactiveProperty<Func<UniTask<AudioClip>>>();
                _ctx.GetMusicRequest.Execute((path, task));
                var clip = await task.Value.Invoke();
                task.Dispose();

                var result = AudioClipToCache(clip, path);
                return result;
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

            using (var stream = File.Open(file, FileMode.Create))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, rawData);
            }

            return data;
        }
    }
}

