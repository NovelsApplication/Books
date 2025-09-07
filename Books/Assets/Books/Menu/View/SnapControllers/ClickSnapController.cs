using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.View.SnapControllers
{
    public class ClickSnapController : SnapController
    {
        private List<Button> _buttonComponents = new ();
        private int _followElementsCounter;
        
        public override void FollowElement(RectTransform element)
        {
            base.FollowElement(element);

            int tempFollowElementIndex = _followElementsCounter;
            _followElementsCounter++;

            Button buttonComponent = element.GetComponent<Button>();
            if (buttonComponent == null) 
                return;
            buttonComponent.onClick.AddListener(() => OnElementClick(tempFollowElementIndex));
            
            _buttonComponents.Add(buttonComponent);
        }

        private IEnumerator _smoothSnapToElement;

        private void OnElementClick(int targetElementIndex)
        {
            if (_smoothSnapToElement != null)
                StopCoroutine(_smoothSnapToElement);
            
            _smoothSnapToElement = SmoothSnapToElement(targetElementIndex);
            StartCoroutine(_smoothSnapToElement);
        }
    }
}