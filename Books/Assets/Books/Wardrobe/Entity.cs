using System;
using System.Linq;
using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using UniRx;
using UnityEngine;

namespace Books.Wardrobe
{
    public class Entity : BaseDisposable
    {
        public struct TestData
        {
            public string LocationName;

            public TestData(string locationName)
            {
                LocationName = locationName;
            }
        }
        
        public struct Ctx
        {
            public TestData TestData;
            public Data Data;
            
            public ReactiveCommand<(string path, string name, ReactiveProperty<Func<UniTask<UnityEngine.Object>>> task)> GetBundle;
            public ReactiveCommand<(string path, ReactiveProperty<Func<UniTask<string[]>>> namesTask)> GetAllAssetNames;
            
            public ReactiveCommand<(string path, string key, ReactiveProperty<Func<UniTask<Texture2D>>> task)> GetTexture;
            public IObservable<(Texture2D texture, string key)> OnGetTexture;

            public bool IsLightTheme;
        }

        private Ctx _ctx;
        private IScreen _screen;
        
        private readonly EnumDisplayNameResolver _resolver;
        private readonly LocationPathStrategy _locationPathStrategy;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            Init();

            _resolver = new EnumDisplayNameResolver();
            _locationPathStrategy = new LocationPathStrategy(_resolver);
        }

        private async void Init()
        {
            var task = new ReactiveProperty<Func<UniTask<UnityEngine.Object>>>();
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName, task));
            
            var bundle = await task.Value.Invoke();
            task.Dispose();

            _screen = UnityEngine.Object.Instantiate(bundle as GameObject)
                .GetComponent<IScreen>();
            _screen.HideImmediate();
        }

        public async void Open(string storyPath, string storyLocationImagePath = "")
        {
            var namesTask = new ReactiveProperty<Func<UniTask<string[]>>>();
            _ctx.GetAllAssetNames.Execute(("main", namesTask));

            string[] allAssetNames = await namesTask.Value.Invoke();
            namesTask.Dispose();

            //TODO: Добавить проверку корректности и наличия путей, как в кэше
            if (storyLocationImagePath == "") // если мы открываем из главного меню
            {
                
            }
            else // если мы открываем из истории
            {
                
            }
            
            _screen.ShowImmediate();
        }

        private string RootPath(string storyPath) => $"Main/{storyPath}/Визуал/";

        private string RelativePath(string fullPath, string storyPath)
        {
            string rootPart = RootPath(storyPath);
            
            if (!fullPath.StartsWith(rootPart))
            {
                string relativePath = fullPath.Substring(rootPart.Length);
                return relativePath.TrimStart('/');
            }
            
            Debug.LogErrorFormat($"Is not possible to take a relative path from {fullPath}");
            return null;
        }

        private bool CheckValidPath(string fullPath)
        {
            return false;
        }
    }
}