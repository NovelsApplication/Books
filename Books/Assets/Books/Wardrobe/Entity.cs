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
            
            var textureTask = new ReactiveProperty<Func<UniTask<Texture2D>>>();

            if (locationPath == "") // если мы открываем из главного меню
            {
                
                // Локации
                
                
                LocationMetadata lightBackMetadata = new LocationMetadata(_ctx.TestData.LocationName, EnvironmentType.Land, LightMode.Light);
                string lightBackPath = RootContentPath(storyPath) + _locationPathParser.BuildRootFolderPath(lightBackMetadata) + _ctx.TestData.LocationName + ".png";
                Texture2D lightBackTexture = await LoadTexture(textureTask, lightBackPath);
                
                LocationAssetModel lightBackModel = new LocationAssetModel(lightBackMetadata, lightBackTexture, null);
                
                LocationMetadata darkBackMetadata = new LocationMetadata(_ctx.TestData.LocationName, EnvironmentType.Land, LightMode.Light);
                string darkBackPath = RootContentPath(storyPath) + _locationPathParser.BuildRootFolderPath(darkBackMetadata) + _ctx.TestData.LocationName + ".png";
                Texture2D darkBackTexture = await LoadTexture(textureTask, darkBackPath);
                
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
                        string glowingTexturePath = RootContentPath(storyPath) + relativeRootFolderPath + "Свечение.png";
                        Texture2D glowingTexture = await LoadTexture(textureTask, glowingTexturePath);
                        Sprite glowingSprite = CreateSprite(glowingTexture);
                        
                        assetModel = new ClothesAssetModel(meta, glowingSprite);
                        assetModels.Add(key: meta.ItemName, value: assetModel);
                    }
                    
                    string itemSpritePath = RootContentPath(storyPath) + path;
                    var itemTexture = await LoadTexture(textureTask, itemSpritePath);
                    Sprite itemSprite = CreateSprite(itemTexture);
                    
                    string colorSpritePath = RootContentPath(storyPath) + relativeRootFolderPath + colorsFolderName + "/" + fileName;
                    var colorTexture = await LoadTexture(textureTask, colorSpritePath);
                    Sprite colorSprite = CreateSprite(colorTexture);
                    
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
                string fullLocationPath = RootContentPath(storyPath) + locationPath;

                LocationMetadata backTextureMetadata = _locationPathParser.ParsePath(locationPath);
                var backTexture = await LoadTexture(textureTask, fullLocationPath);
            }
            
            textureTask.Dispose();
        }

        private async UniTask<Texture2D> LoadTexture(ReactiveProperty<Func<UniTask<Texture2D>>> textureTask, string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                return null;
            }
            
            string key = path;
            _ctx.GetTexture.Execute((path, key, textureTask));
            
            return await textureTask.Value.Invoke();
        }
        
        private Sprite CreateSprite(Texture2D texture)
        {
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            Sprite sprite = Sprite.Create(texture, rect, pivot, 100f);
            
            if (sprite == null)
            {
                Debug.Log($"Не удалось создать спрайт для текстуры : {texture.name}");
                return null;
            }
            else
            {
                Debug.Log($"Спрайт для объекта - {texture.name} создан" );
                return sprite;
            }
        }

        private string RelativePath(string fullPath, string storyPath)
        {
            string rootPart = RootContentPath(storyPath);
            
            if (fullPath.StartsWith(rootPart))
            {
                string relativePath = fullPath.Substring(rootPart.Length);
                return relativePath.TrimStart('/');
            }
            
            Debug.LogErrorFormat($"Is not possible to take a relative path from {fullPath}");
            return null;
        }

        private string RootContentPath(string storyPath) => $"{storyPath}/Визуал/";
    }
}