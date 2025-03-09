using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Story 
{
    public class StoryBubble : MonoBehaviour
    {
        public enum Side 
        {
            Left,
            Center,
            Right,
        }

        [SerializeField] private TMP_Text _headerTextArea;
        [SerializeField] private TMP_Text _bodyTextArea;
        [SerializeField] private Button _chooseButton;

        private readonly Stack<GameObject> _buttons = new();

        public void UpdateText(Side side, string header, string body) 
        {
            ClearAll();
            _chooseButton.gameObject.SetActive(false);

            _headerTextArea.text = header;
            switch (side) 
            {
                case Side.Left:
                    _headerTextArea.alignment = TextAlignmentOptions.Left;
                    break;
                case Side.Center:
                    _headerTextArea.alignment = TextAlignmentOptions.Center;
                    break;
                case Side.Right:
                    _headerTextArea.alignment = TextAlignmentOptions.Right;
                    break;
            }
            _bodyTextArea.text = body;

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        }

        public void UpdateButtons(Action<int> onClick, params (string header, int index)[] buttons)
        {
            ClearAll();
            _chooseButton.gameObject.SetActive(false);

            foreach (var (header, index) in buttons) 
            {
                var newButton = Instantiate(_chooseButton);
                newButton.transform.SetParent(_chooseButton.transform.parent, false);
                newButton.onClick.RemoveAllListeners();
                newButton.onClick.AddListener(() => onClick?.Invoke(index));
                var newButtonHeader = newButton.GetComponentInChildren<TMP_Text>(true);
                newButtonHeader.text = header;

                newButton.gameObject.SetActive(true);

                _buttons.Push(newButton.gameObject);

                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            }
        }

        private void ClearAll()
        {
            while (_buttons.TryPop(out var unitGO))
                Destroy(unitGO);
        }
    }
}
