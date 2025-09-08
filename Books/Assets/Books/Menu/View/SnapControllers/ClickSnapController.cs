using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.View.SnapControllers
{
    public class ClickSnapController : SnapController
    {
        private readonly List<Button> _buttonComponents = new ();
        private IEnumerator _smoothSnapToElement;

        protected override void OnFollowElement(RectTransform element, int index)
        {
            Button buttonComponent = element.GetComponent<Button>();
            if (buttonComponent == null) 
                return;
            buttonComponent.onClick.AddListener(() => OnElementClick(index));
            _buttonComponents.Add(buttonComponent);
        }

        private void OnElementClick(int targetElementIndex)
        {
            if (_smoothSnapToElement != null)
                StopCoroutine(_smoothSnapToElement);
            
            _smoothSnapToElement = SmoothSnapToElement(targetElementIndex);
            StartCoroutine(_smoothSnapToElement);
        }
    }
}