using Books.Wardrobe.ViewModel;
using TMPro;
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

        // [SerializeField] private CategoryHead _behaviorCategory;
        // [SerializeField] private CategoryHead _hairCategory;
        // [SerializeField] private CategoryHead _suitsCategory;
        // [SerializeField] private CategoryHead _accessoriasCategory;

        [SerializeField] private CategoryHead[] _categoryHeads;

        [SerializeField] private Layer _layerPrefab;

        [SerializeField] private Button _nextItemSelector;
        [SerializeField] private Button _previousItemSelector;
        [SerializeField] private TextMeshProUGUI _itemNameIMP;

        private Layer[] _layers;
        private ScreenModel _model;
        
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
            
            //_visualComponent.UpdateVisual(model.Visual);
            
            _layers = new Layer[model.MaxLayerNumber + 1];
            
            for (int i = 0; i <= model.MaxLayerNumber; i++)
            {
                var layerInstance = Object.Instantiate(_layerPrefab, _layerPrefab.transform.parent);
                layerInstance.gameObject.SetActive(true);
                _layers[i] = layerInstance;
            }
            
            _nextItemSelector.onClick.AddListener(() => VisualizeItem(model.NextItem()));
            _previousItemSelector.onClick.AddListener(() => VisualizeItem(model.PreviousItem()));
        }

        private void VisualizeItem(ClothingAssetModel itemModel)
        {
            
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