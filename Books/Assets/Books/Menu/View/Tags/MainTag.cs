using UnityEngine;

namespace Books.Menu.View.Tags 
{
    public class MainTag : MonoBehaviour
    {
        [field: SerializeField] 
        public Entity.MainTags Tag { get; private set; }
        
        [SerializeField] private GameObject _selectedRoot;
        [SerializeField] private GameObject _defaultRoot;
        
        public void SetSelected(bool state) 
        {
            _selectedRoot.SetActive(state);
            _defaultRoot.SetActive(!state);
        }
    }
}

