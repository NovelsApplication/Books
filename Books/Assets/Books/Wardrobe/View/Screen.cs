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
        [SerializeField] private RawImage _lightBack;
        [SerializeField] private RawImage _darkBack;
        
        [SerializeField] private ScreenVisual _visualComponent;

        private ScreenModel _model;
        
        public void BindModel(ScreenModel model)
        {
            if (model == null)
                return;

            _model = model;
            
            _visualComponent.UpdateVisual(model.Visual);
            
            
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