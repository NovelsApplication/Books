using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Menu.View.SnapControllers
{
    public class ClickSnapController : SnapController
    {
        protected override int StartTargetElementIndex => 3;

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

        private void OnElementClick(int targetElementIndex)
        {
            StartCoroutine(SmoothSnapToElement(targetElementIndex));
        }
    }
}