using System;
using System.Linq;
using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.ViewModel;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Books.Wardrobe.View
{
    public interface IScreen
    {
        public void BindModel(ScreenModel model, bool isLightTheme);

        public void SetTheme(bool isLightTheme);
        
        public void ShowImmediate();
        public void HideImmediate();
    }
    
    public class Screen : MonoBehaviour, IScreen
    {
        [SerializeField] private TextMeshProUGUI _characterNameTMP;
        [SerializeField] private RawImage _background;
        [SerializeField] private ScreenVisual _visualComponent;
        [SerializeField] private CategoryHead[] _categoryHeads;
        [SerializeField] private Layer _layerPrefab;
        [SerializeField] private Button _nextItemSelector;
        [SerializeField] private Button _previousItemSelector;
        [SerializeField] private TextMeshProUGUI _itemNameIMP;
        [SerializeField] private SuitUpdate_Animation _suitUpdateAnimation;
        [SerializeField] private ColorMenu _colorMenu;
        [SerializeField] private Button _lightingButton;

        private Layer[] _layers;
        private CategoryHead _activeCategoryHead;
        private AssetsCategory _categoryModel;
        private ScreenModel _model;
        private bool _isLightTheme;

        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public void BindModel(ScreenModel model, bool isLightTheme)
        {
            if (model == null)
            {
                Debug.LogError("Model is null!!!");
                return;
            }
            
            _model = model;
            _characterNameTMP.text = model.CharacterName;

            _layerPrefab.gameObject.SetActive(false);
            _layers = new Layer[model.MaxLayerNumber + 1];
            
            for (int i = 0; i <= model.MaxLayerNumber; i++)
            {
                var layerInstance = Object.Instantiate(_layerPrefab, _layerPrefab.transform.parent);
                layerInstance.HideItem();
                layerInstance.gameObject.SetActive(true);
                _layers[i] = layerInstance;
            }

            foreach (var categoryHead in _categoryHeads)
            {
                AssetsCategory category = _model.GetCategory(categoryHead.CategoryType);
                //Реактивное изменение текущей шмотки
                _disposable.Add(category.CurrentItemModel.Subscribe(VisualizeItem));
                
                categoryHead.InitCategory(category);
                categoryHead.SetSelect(false);
                
                Button btn = categoryHead.GetComponent<Button>();
                btn.onClick.AddListener(() => SetActiveCategory(categoryHead.CategoryType));
            }
            
            SetActiveCategory(CategoryType.Suit);
            SetTheme(isLightTheme);
            
            _lightingButton.onClick.AddListener(() => SetTheme(!_isLightTheme));
            
            _nextItemSelector.onClick.AddListener(NextItem);
            _previousItemSelector.onClick.AddListener(PreviousItem);
        }

        public void SetTheme(bool isLightTheme)
        {
            if (isLightTheme && _model.LightBackLocationModel != null)
            {
                _background.texture = _model.LightBackLocationModel.LocationImage;
            }
            else if (_model.DarkBackLocationModel != null)
            {
                _background.texture = _model.DarkBackLocationModel.LocationImage;
            }

            if (_layers != null)
            {
                foreach (var layer in _layers)
                {
                    layer.SetDark(!isLightTheme);
                }
            }

            _isLightTheme = isLightTheme;
        }

        private void SetActiveCategory(CategoryType categoryType)
        {
            if (categoryType == CategoryType.None)
                return;

            if (_activeCategoryHead != null)
                _activeCategoryHead.SetSelect(false);
            
            CategoryHead categoryHead = _categoryHeads.First(c => c.CategoryType == categoryType);
            _activeCategoryHead = categoryHead;
            _activeCategoryHead.SetSelect(true);
            
            _categoryModel = _model.GetCategory(categoryType);
            
            VisualizeItem(_categoryModel.CurrentItemModel.Value);
        }

        private void VisualizeItem(ClothingAssetModel itemModel)
        {
            if (itemModel == null) 
                return;

            CategoryType targetCategoryType = itemModel.Metadata.CategoryType;
            AssetsCategory targetCategory = _model.GetCategory(targetCategoryType);
            
            int[] categoryLayers = targetCategory.GetCategoryLayers();
            
            foreach (int layer in categoryLayers)
            {
                if (layer >= 0 && layer < _layers.Length)
                {
                    _layers[layer].HideItem();
                }
            }
            
            int suitLayer = itemModel.Metadata.SuitLayer;
            if (suitLayer >= 0 && suitLayer < _layers.Length && !itemModel.IsEmptyModel)
            {
                var items = new (Sprite item, Sprite color)[itemModel.ItemsCount];
                for (int i = 0; i < itemModel.ItemsCount; i++)
                {
                    items[i] = itemModel.GetItem(i);
                }
                
                Action<int> showColorVariantAction = index =>
                {
                    var element = items[index];
                    itemModel.SetColorActive(index); // можно тут менять значение реактивщины
                    _layers[suitLayer].ShowItem(element.item, itemModel.GlowingSprite);
                };
                showColorVariantAction.Invoke(itemModel.CurrentColorIndex);

                _colorMenu.InitColors(items.Select(i => i.color).ToArray(), showColorVariantAction, itemModel.CurrentColorIndex);
            }
            else
            {
                _colorMenu.HideImmediate();
            }
            
            if (_activeCategoryHead != null && targetCategoryType == _activeCategoryHead.CategoryType)
                _itemNameIMP.text = itemModel.Name;
        }

        private void NextItem()
        {
            if (_categoryModel == null)
            {
                Debug.LogError("Active category model is null");
                return;
            }

            CanvasGroup clone = _suitUpdateAnimation.CreateClone();
            
            _categoryModel.NextItem();
            OnSelectItem(clone);
        }
        
        private void PreviousItem()
        {
            if (_categoryModel == null)
            {
                Debug.LogError("Active category model is null");
                return;
            }

            CanvasGroup clone = _suitUpdateAnimation.CreateClone();
            
            _categoryModel.PreviousItem();
            OnSelectItem(clone);
        }

        private async void OnSelectItem(CanvasGroup clone)
        {
            _nextItemSelector.onClick.RemoveAllListeners();
            _previousItemSelector.onClick.RemoveAllListeners();
            
            await _suitUpdateAnimation.Play(clone);
            
            _nextItemSelector.onClick.AddListener(NextItem);
            _previousItemSelector.onClick.AddListener(PreviousItem);
        }

        public void UnBindModel()
        {
            _disposable.Dispose();
            _colorMenu.Clear();
            _model = null;
            _activeCategoryHead = null;
            _categoryModel = null;
        }

        public void ShowImmediate()
        {
            gameObject.SetActive(true);
        }

        public void HideImmediate()
        {
            gameObject.SetActive(false);
        }
    }
}