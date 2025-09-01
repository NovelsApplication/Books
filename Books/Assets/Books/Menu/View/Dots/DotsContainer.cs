using System.Collections.Generic;
using UnityEngine;

namespace Books.Menu.View.Dots
{
    public class DotsContainer : MonoBehaviour
    {
        private List<Dot> _dots = new ();
        private int _selectDotIndex;

        public void FollowDot(Dot dot)
        {
            if (dot == null)
                return;

            dot.gameObject.SetActive(true);
            dot.SetSelected(false);
            
            _dots.Add(dot);
        }
        
        public void SetDotSelect(int index)
        {
            if (index > _dots.Count - 1)
                return;
            
            _dots[_selectDotIndex].SetSelected(false);
            
            _dots[index].SetSelected(true);
            _selectDotIndex = index;
        }
    }
}