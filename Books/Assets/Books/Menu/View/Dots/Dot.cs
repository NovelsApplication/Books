using UnityEngine;

namespace Books.Menu.View.Dots 
{
    public class Dot : MonoBehaviour
    {
        [SerializeField] private GameObject _selectedRoot;

        public void SetSelected(bool state) => _selectedRoot.SetActive(state);
    }
}
