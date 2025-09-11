using UnityEngine;

namespace Books.Menu.View.Dots 
{
    public class Dot : MonoBehaviour
    {
        [SerializeField] private GameObject _selectedRoot;
        [SerializeField] private GameObject _bubble;

        public void SetSelected(bool state)
        {
            _selectedRoot.SetActive(state);
            _bubble.SetActive(!state);
        }
    }
}
