using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Books.Wardrobe.AssetsMeta;
using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.View;
using Books.Wardrobe.ViewModel;
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
            public ReactiveCommand<(string path, string key, ReactiveProperty<Func<UniTask<Texture2D>>> task)> GetTexture;

            public bool IsLightTheme;
        }

        private Ctx _ctx;
        private IScreen _screen;
        
        private readonly EnumDisplayNameResolver _resolver;
        private readonly LocationPathParser _locationPathParser;
        private readonly ClothesPathParser _clothesPathParser;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            _resolver = new EnumDisplayNameResolver();
            _locationPathParser = new LocationPathParser(_resolver);
            _clothesPathParser = new ClothesPathParser(_resolver);
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
                
                // Локации
                
                
                LocationMetadata lightBackMetadata = new LocationMetadata(_ctx.TestData.LocationName, EnvironmentType.Land, LightMode.Light);
                string lightBackPath = RootPath(storyPath) + _locationPathParser.BuildRootFolderPath(lightBackMetadata) + _ctx.TestData.LocationName + ".png";
                Texture2D lightBackTexture = await LoadRequest<Texture2D>(loadTask, lightBackPath);
                
                LocationAssetModel lightBackModel = new LocationAssetModel(lightBackMetadata, lightBackTexture, null);
                
                LocationMetadata darkBackMetadata = new LocationMetadata(_ctx.TestData.LocationName, EnvironmentType.Land, LightMode.Light);
                string darkBackPath = RootPath(storyPath) + _locationPathParser.BuildRootFolderPath(darkBackMetadata) + _ctx.TestData.LocationName + ".png";
                Texture2D darkBackTexture = await LoadRequest<Texture2D>(loadTask, darkBackPath);
                
                LocationAssetModel darkBackModel = new LocationAssetModel(darkBackMetadata, darkBackTexture, null);
                // Одежда
                
                Dictionary<string, ClothesAssetModel> assetModels = new ();
                
                foreach (var path in _ctx.TestData.Clothes)
                {
                    ClothesMetadata meta = _clothesPathParser.ParsePath(path);
                    string relativeRootFolderPath = path.Substring(0, path.LastIndexOf('/') + 1);
                    string fileName = Path.GetFileName(path);
                    string colorsFolderName = "Кружочки";

                    ClothesAssetModel assetModel;
                    
                    if (assetModels.ContainsKey(meta.ItemName))
                    {
                        assetModel = assetModels[meta.ItemName];
                    }
                    else
                    {
                        string glowingSpritePath = RootPath(storyPath) + relativeRootFolderPath + "Свечение.png";
                        Sprite glowingSprite = await LoadRequest<Sprite>(loadTask, glowingSpritePath);
                        
                        assetModel = new ClothesAssetModel(meta, glowingSprite);
                        assetModels.Add(key: meta.ItemName, value: assetModel);
                    }
                    
                    string itemSpritePath = RootPath(storyPath) + path;
                    var itemSprite = await LoadRequest<Sprite>(loadTask, itemSpritePath);
                    
                    string colorSpritePath = RootPath(storyPath) + relativeRootFolderPath + colorsFolderName + "/" + fileName;
                    var colorSprite = await LoadRequest<Sprite>(loadTask, colorSpritePath);
                    
                    assetModel.AddItem(itemSprite, colorSprite);
                }

                ScreenModel screenModel = new ScreenModel(
                    EnvironmentType.Land,
                    lightBackModel,
                    assetModels.Select(o => o.Value).ToArray(),
                    "Максим",
                    darkBackModel);
                
                _screen.BindModel(screenModel);
            }
            
            else // если мы открываем из истории
            {
                string fullLocationPath = RootPath(storyPath) + locationPath;
                if (!CheckValidPath(fullLocationPath))
                {
                    Debug.LogErrorFormat("Некорректный путь текущей локации. Невозможно открыть гардероб!");
                    return;
                }

                LocationMetadata backTextureMetadata = _locationPathParser.ParsePath(locationPath);
                var backTexture = await LoadRequest<Texture2D>(loadTask, fullLocationPath);
            }
            
            loadTask.Dispose();
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