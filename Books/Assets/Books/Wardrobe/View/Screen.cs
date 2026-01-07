using System.Linq;
using Books.Wardrobe.PathStrategies;
using Books.Wardrobe.ViewModel;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Wardrobe.View
{
    public interface IScreen
    {
        public void BindModel(ScreenModel model);
        
        public void ShowImmediate();
        public void HideImmediate();
    }
    
    public class Screen : MonoBehaviour, IScreen
    {
        [SerializeField] private TextMeshProUGUI _characterNameTMP;
        [SerializeField] private RawImage _mainBack;
        [SerializeField] private RawImage _additionalBack;
        [SerializeField] private ScreenVisual _visualComponent;
        [SerializeField] private CategoryHead[] _categoryHeads;
        [SerializeField] private Layer _layerPrefab;
        [SerializeField] private Button _nextItemSelector;
        [SerializeField] private Button _previousItemSelector;
        [SerializeField] private TextMeshProUGUI _itemNameIMP;

        private Layer[] _layers;
        private CategoryHead _activeCategoryHead;
        private AssetsCategory _categoryModel;
        private ScreenModel _model;

        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public void BindModel(ScreenModel model)
        {
            if (model == null)
            {
                Debug.LogError("Model is null!!!");
                return;
            }
            
            _model = model;
            
            _characterNameTMP.text = model.CharacterName;
            _mainBack.texture = model.DefaultBackLocationModel.LocationImage;
            _additionalBack.texture = model.AdditionalBackLocationModel.LocationImage;


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
            
            _nextItemSelector.onClick.AddListener(NextItem);
            _previousItemSelector.onClick.AddListener(PreviousItem);
        }

        public void SetActiveCategory(CategoryType categoryType)
        {
            if (categoryType == CategoryType.None)
                return;

            if (_activeCategoryHead != null)
                _activeCategoryHead.SetSelect(false);
            
            CategoryHead categoryHead = _categoryHeads.First(c => c.CategoryType == categoryType);
            _activeCategoryHead = categoryHead;
            _activeCategoryHead.SetSelect(true);
            
            _categoryModel = _model.GetCategory(categoryType);
            _itemNameIMP.text = _categoryModel.CurrentItemModel.Value.Name;
        }

        private void VisualizeItem(ClothingAssetModel itemModel)
        {
            if (itemModel == null || _activeCategoryHead == null) 
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
            if (suitLayer >= 0 && suitLayer < _layers.Length)
            {
                var (itemSprite, colorSprite) = itemModel.GetItem(0);
                _layers[suitLayer].ShowItem(itemSprite, itemModel.GlowingSprite);
            }
            
            if (targetCategoryType == _activeCategoryHead.CategoryType)
                _itemNameIMP.text = itemModel.Name;
        }

        private void NextItem()
        {
            if (_categoryModel == null)
            {
                Debug.LogError("Active category model is null");
                return;
            }
                
            _categoryModel.NextItem();
        }
        
        private void PreviousItem()
        {
            if (_categoryModel == null)
            {
                Debug.LogError("Active category model is null");
                return;
            }
                
            _categoryModel.PreviousItem();
        }

        public void UnBindModel()
        {
            _disposable.Dispose();
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