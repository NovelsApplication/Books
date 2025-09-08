using System.Collections.Generic;
using UnityEngine;

namespace Books.Menu.View.Dots
{
    public class DotsContainer : MonoBehaviour
    {
        private Dot[] _dots;
        
        private int _selectDotIndex;
        private int _followDotIndex;

        public void Initialize(int dotsCount)
        {
            _dots = new Dot[dotsCount];
        }
        
        public void FollowDot(Dot dot)
        {
            if (dot == null || _dots == null)
                return;

            if (_followDotIndex >= _dots.Length)
            {
                Debug.LogErrorFormat($"attempt to track an extra dot object with name {dot.name}");
                return;
            }
            
            dot.gameObject.SetActive(true);
            dot.SetSelected(false);

            _dots[_followDotIndex] = dot;
            _followDotIndex++;
        }
        
        internal void SetDotSelect(int index)
        {
            if (index > _dots.Length - 1)
                return;
            
            _dots[_selectDotIndex].SetSelected(false);
            
            _dots[index].SetSelected(true);
            _selectDotIndex = index;
        }
    }
}