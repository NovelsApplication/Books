using UnityEngine;

namespace Books.Menu.View 
{
    public class MainTag : MonoBehaviour
    {
        [SerializeField] private GameObject _selectedRoot;
        [SerializeField] private GameObject _defaultRoot;

        [SerializeField] private Entity.MainTags _mainTag;

        public void SetSelected(bool state) 
        {
            _selectedRoot.SetActive(state);
            _defaultRoot.SetActive(!state);
        }
    }
}

