using System;
using System.Collections.Generic;
using System.IO;
using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.View;
using Cysharp.Threading.Tasks;
using Shared.Disposable;
using UniRx;
using UnityEngine;
using Screen = UnityEngine.Screen;

namespace Books.Wardrobe
{
    public class Entity : BaseDisposable
    {
        public struct TestData
        {
            public string LocationName;
            public string[] Clothes;

            public TestData(string locationName, string[] clothes)
            {
                LocationName = locationName;
                Clothes = clothes;
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
        private readonly ClothesPathStrategy _clothesPathStrategy;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            _resolver = new EnumDisplayNameResolver();
            _locationPathStrategy = new LocationPathStrategy(_resolver);
            _clothesPathStrategy = new ClothesPathStrategy(_resolver);
        }

        public async UniTask Open(string storyPath, string locationPath = "")
        {
            var task = new ReactiveProperty<Func<UniTask<UnityEngine.Object>>>();
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName, task));
            
            var bundle = await task.Value.Invoke();
            task.Dispose();

            var go = UnityEngine.Object.Instantiate(bundle as GameObject);
            _screen = go.GetComponent<IScreen>();
            _screen.HideImmediate();
            
            var loadTask = new ReactiveProperty<Func<UniTask<UnityEngine.Object>>>();

            if (locationPath == "") // если мы открываем из главного меню
            {
                List<ScreenModel.AssetModel> assetModels = new List<ScreenModel.AssetModel>();

                string defaultLightBackgroundPath = RootPath(storyPath) + _locationPathStrategy.BuildPath(new AssetMetadata(
                    itemName: _ctx.TestData.LocationName, itemType: ItemType.Location, environmentType: EnvironmentType.Land,  lightMode: LightMode.Light));
                var defaultLightBackTexture = await LoadRequest<Texture2D>(loadTask, defaultLightBackgroundPath);

                string defaultDarkBackgroundPath = RootPath(storyPath) + _locationPathStrategy.BuildPath(new AssetMetadata(
                    itemName: _ctx.TestData.LocationName, itemType: ItemType.Location, environmentType: EnvironmentType.Land,  lightMode: LightMode.Dark));
                var defaultDarkBackTexture = await LoadRequest<Texture2D>(loadTask, defaultDarkBackgroundPath);

                foreach (var path in _ctx.TestData.Clothes)
                {
                    AssetMetadata assetMetadata = _clothesPathStrategy.ParsePath(path);
                    
                    string fullPath = RootPath(storyPath) + path;
                    var sprite = await LoadRequest<Sprite>(loadTask, fullPath);
                    
                    if (sprite != null && assetMetadata.ItemType != ItemType.None)
                        assetModels.Add(new ScreenModel.AssetModel(sprite, assetMetadata));
                }

                ScreenModel screenModel = new ScreenModel(
                    environmentType: EnvironmentType.Land,
                    mainBackTexture: defaultLightBackTexture,
                    assets: assetModels.ToArray(),
                    additionalBackTexture: defaultDarkBackTexture,
                    targetCharacterType: Character.Main,
                    characterName: "Максим");
            }
            else // если мы открываем из истории
            {
                string fullLocationPath = RootPath(storyPath) + locationPath;
                if (!CheckValidPath(fullLocationPath))
                {
                    Debug.LogErrorFormat("Некорректный путь текущей локации. Невозможно открыть гардероб!");
                    return;
                }

                AssetMetadata backTextureMetadata = _locationPathStrategy.ParsePath(locationPath);
                var backTexture = await LoadRequest<Texture2D>(loadTask, fullLocationPath);
            }
            
            loadTask.Dispose();
            
            _screen.ShowImmediate();
        }

        private async UniTask<T> LoadRequest<T>(ReactiveProperty<Func<UniTask<UnityEngine.Object>>> task, string path) where T: UnityEngine.Object
        {
            if (!CheckValidPath(path))
            {
                return null;
            }
            
            _ctx.GetBundle.Execute(("main", path, task));
            var obj = await task.Value.Invoke();
                
            T concreteObj = obj as T;
            if (concreteObj == null)
            {
                Debug.Log("Загружаемый объект равен NULL");
                return null;
            }
            else
            {
                Debug.Log("Загружен - " + concreteObj.name);
                return concreteObj;
            }
        }

        private string RelativePath(string fullPath, string storyPath)
        {
            string rootPart = RootPath(storyPath);
            
            if (fullPath.StartsWith(rootPart))
            {
                string relativePath = fullPath.Substring(rootPart.Length);
                return relativePath.TrimStart('/');
            }
            
            Debug.LogErrorFormat($"Is not possible to take a relative path from {fullPath}");
            return null;
        }

        private bool CheckValidPath(string fullPath)
        {
            if (!String.IsNullOrEmpty(fullPath) && File.Exists(fullPath))
                return true;

            return false;
        }

        private string RootPath(string storyPath) => $"Assets/Remote/Main/{storyPath}/Визуал/";
    }
}