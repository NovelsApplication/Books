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
            public string[] Clothes;
            public string[] Hairstyles;
            public string[] Accessories;
            public string[] Appearance;

            public TestData(string[] clothes, string[] hairstyles, string[] accessories, string[] appearance)
            {
                Clothes = clothes;
                Hairstyles = hairstyles;
                Accessories = accessories;
                Appearance = appearance;
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
        
        private readonly EnumDisplayNameResolver _enumResolver;
        private readonly LocationPathParser _locationPathParser;
        private readonly SuitPathParser _suitPathParser;
        private readonly HairstylePathParser _hairstylePathParser;
        private readonly AccessoriesPathParser _accessoriesPathParser;
        private readonly AppearancePathParser _appearancePathParser;

        public Entity(Ctx ctx)
        {
            _ctx = ctx;
            
            _enumResolver = new EnumDisplayNameResolver();
            _locationPathParser = new LocationPathParser(_enumResolver);
            _suitPathParser = new SuitPathParser(_enumResolver);
            _hairstylePathParser = new HairstylePathParser(_enumResolver);
            _accessoriesPathParser = new AccessoriesPathParser(_enumResolver);
            _appearancePathParser = new AppearancePathParser(_enumResolver);
        }

        public async UniTask Open(Menu.Entity.StoryManifest storyManifest, string locationPath = "")
        {
            string storyPath = storyManifest.StoryPath;
            
            var task = new ReactiveProperty<Func<UniTask<UnityEngine.Object>>>();
            _ctx.GetBundle.Execute(("main", _ctx.Data.ScreenName, task));
            
            var bundle = await task.Value.Invoke();
            task.Dispose();

            var go = UnityEngine.Object.Instantiate(bundle as GameObject);
            _screen = go.GetComponent<IScreen>();
            _screen.HideImmediate();
            
            var textureTask = new ReactiveProperty<Func<UniTask<Texture2D>>>();
            
            int startSize = _ctx.TestData.Clothes.Length + _ctx.TestData.Accessories.Length + _ctx.TestData.Hairstyles.Length;
            Dictionary<string, ClothingAssetModel> assetModels = new (startSize);

            if (locationPath == "") // если мы открываем из главного меню
            {
                
                // Локации
                
                LocationMetadata lightBackMetadata = new LocationMetadata("Гардероб суша день", EnvironmentType.Land, LightMode.Light);
                string lightBackPath = RootContentPath(storyPath) + _locationPathParser.BuildRootFolderPath(lightBackMetadata) + "Гардероб суша день" + ".png";
                Texture2D lightBackTexture = await LoadTexture(textureTask, lightBackPath);
                
                LocationAssetModel lightBackModel = new LocationAssetModel(lightBackMetadata, lightBackTexture, null);
                
                LocationMetadata darkBackMetadata = new LocationMetadata("Гардероб суша ночь", EnvironmentType.Land, LightMode.Dark);
                string darkBackPath = RootContentPath(storyPath) + _locationPathParser.BuildRootFolderPath(darkBackMetadata) + "Гардероб суша ночь" + ".png";
                Texture2D darkBackTexture = await LoadTexture(textureTask, darkBackPath);
                
                LocationAssetModel darkBackModel = new LocationAssetModel(darkBackMetadata, darkBackTexture, null);
                
                
                string colorsFolderName = "Кружочки";
                string hairColorsFolderName = "Цвета волос";
                
                // Одежда

                ClothingMetadata emptySuitMeta = new ClothingMetadata(
                    ItemType.Suit, CategoryType.Suit,
                    "Без одежды",
                    EnvironmentType.Universal,
                    4, "");

                ClothingAssetModel emptySuitModel = new ClothingAssetModel(emptySuitMeta);
                assetModels.Add(emptySuitModel.Name, emptySuitModel);
                
                foreach (var path in _ctx.TestData.Clothes)
                {
                    ClothingMetadata meta = _suitPathParser.ParsePath(path);
                    string relativeRootFolderPath = path.Substring(0, path.LastIndexOf('/') + 1);
                    string fileName = Path.GetFileName(path);

                    ClothingAssetModel assetModel;
                    
                    if (assetModels.ContainsKey(meta.ItemName))
                    {
                        assetModel = assetModels[meta.ItemName];
                    }
                    else
                    {
                        string glowingTexturePath = RootContentPath(storyPath) + relativeRootFolderPath + "Свечение.png";
                        Texture2D glowingTexture = await LoadTexture(textureTask, glowingTexturePath);
                        Sprite glowingSprite = CreateSprite(glowingTexture);
                        
                        assetModel = new ClothingAssetModel(meta, glowingSprite);
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

                // Причёски

                foreach (var path in _ctx.TestData.Hairstyles)
                {
                    ClothingMetadata meta = _hairstylePathParser.ParsePath(path);
                    string relativeRootFolderPath = path.Substring(0, path.LastIndexOf('/') + 1);
                    string fileName = Path.GetFileName(path);

                    ClothingAssetModel assetModel;
                    
                    if (assetModels.ContainsKey(meta.ItemName))
                    {
                        assetModel = assetModels[meta.ItemName];
                    }
                    else
                    {
                        string glowingTexturePath = RootContentPath(storyPath) + relativeRootFolderPath + "Свечение.png";
                        Texture2D glowingTexture = await LoadTexture(textureTask, glowingTexturePath);
                        Sprite glowingSprite = CreateSprite(glowingTexture);
                        
                        assetModel = new ClothingAssetModel(meta, glowingSprite);
                        assetModels.Add(key: meta.ItemName, value: assetModel);
                    }
                    
                    string itemSpritePath = RootContentPath(storyPath) + path;
                    var itemTexture = await LoadTexture(textureTask, itemSpritePath);
                    Sprite itemSprite = CreateSprite(itemTexture);

                    string colorSpritePath = RootContentPath(storyPath) + "Персонажи" + "/" + meta.TargetCharacterName + "/" + hairColorsFolderName + "/" + fileName;
                    var colorTexture = await LoadTexture(textureTask, colorSpritePath);
                    Sprite colorSprite = CreateSprite(colorTexture);
                    
                    assetModel.AddItem(itemSprite, colorSprite);
                }
                
                //Аксессуары
                
                ClothingMetadata emptyAccessoriesMeta = new ClothingMetadata(
                    ItemType.Accessories, CategoryType.Accessories,
                    "Без аксессуаров",
                    EnvironmentType.Universal,
                    0, "");

                ClothingAssetModel emptyAccessoriesModel = new ClothingAssetModel(emptyAccessoriesMeta);
                assetModels.Add(emptyAccessoriesModel.Name, emptyAccessoriesModel);
                
                foreach (var path in _ctx.TestData.Accessories)
                {
                    ClothingMetadata meta = _accessoriesPathParser.ParsePath(path);
                    string relativeRootFolderPath = path.Substring(0, path.LastIndexOf('/') + 1);
                    string fileName = Path.GetFileName(path);

                    ClothingAssetModel assetModel;
                        
                    if (assetModels.ContainsKey(meta.ItemName))
                    {
                        assetModel = assetModels[meta.ItemName];
                    }
                    else
                    {
                        string glowingTexturePath = RootContentPath(storyPath) + relativeRootFolderPath + "Свечение.png";
                        Texture2D glowingTexture = await LoadTexture(textureTask, glowingTexturePath);
                        Sprite glowingSprite = CreateSprite(glowingTexture);
                            
                        assetModel = new ClothingAssetModel(meta, glowingSprite);
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
                
                //Внешность

                foreach (var path in _ctx.TestData.Appearance)
                {
                    ClothingMetadata meta = _appearancePathParser.ParsePath(path);
                    string relativeRootFolderPath = path.Substring(0, path.LastIndexOf('/') + 1);
                    string fileName = Path.GetFileName(path);

                    ClothingAssetModel assetModel;
                    
                    if (assetModels.ContainsKey(meta.ItemName))
                    {
                        assetModel = assetModels[meta.ItemName];
                    }
                    else
                    {
                        assetModel = new ClothingAssetModel(meta);
                        assetModels.Add(key: meta.ItemName, value: assetModel);
                    }
                    
                    string itemSpritePath = RootContentPath(storyPath) + path;
                    var itemTexture = await LoadTexture(textureTask, itemSpritePath);
                    Sprite itemSprite = CreateSprite(itemTexture);
                    
                    assetModel.AddItem(itemSprite);
                }
                
                //--------------------------------//
                
                ScreenModel screenModel = new ScreenModel(
                    EnvironmentType.Land,
                    lightBackModel,
                    darkBackModel,
                    assetModels.Select(o => o.Value).ToArray(),
                    "Элизабет");
                
                _screen.BindModel(screenModel, _ctx.IsLightTheme);

                screenModel.GetCategory(CategoryType.Appearance).SetElementActive(1);
                screenModel.GetCategory(CategoryType.Suit).SetElementActive(1);
            }
            
            else // открываем из истории
            {
                string fullLocationPath = RootContentPath(storyPath) + locationPath;

                LocationMetadata backTextureMetadata = _locationPathParser.ParsePath(locationPath);
                Texture2D backTexture = await LoadTexture(textureTask, fullLocationPath);
                LocationAssetModel backModel = new LocationAssetModel(backTextureMetadata, backTexture, null);

                bool isLightTheme = backTextureMetadata.LightMode == LightMode.Light;
                var lightBack = isLightTheme ? backModel : null;
                var darkBack = !isLightTheme ? backModel : null;
                
                ScreenModel screenModel = new ScreenModel(
                    EnvironmentType.Land,
                    lightBack,
                    darkBack,
                    assetModels.Select(o => o.Value).ToArray(),
                    "Элизабет");
                
                _screen.BindModel(screenModel, isLightTheme);
            }
            
            textureTask.Dispose();
            
            _screen.ShowImmediate();
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
            if (texture == null)
                return null;
            
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);

            Sprite sprite = Sprite.Create(texture, rect, pivot, 100f);
            
            if (sprite == null)
            {
                //Debug.Log($"Не удалось создать спрайт для текстуры : {texture.name}");
                return null;
            }
            else
            {
                //Debug.Log($"Спрайт для объекта - {texture.name} создан" );
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