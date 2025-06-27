using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Books.Story.View
{
    public class Bubble : MonoBehaviour
    {
        [SerializeField] private TMP_Text _headerTextArea;
        [SerializeField] private TMP_Text _bodyTextArea;
        [SerializeField] private Button _chooseButton;
        [SerializeField] private Button _mainButton;

        private readonly Stack<Button> _disabledButtons = new();
        private readonly Stack<Button> _buttons = new();

        private void ClearAll()
        {
            while (_buttons.TryPop(out var unit)) 
            {
                unit.gameObject.SetActive(false);
                _disabledButtons.Push(unit);
            } 
        }

        public async UniTask ShowBubble(Action<int> onClick, string mainCharacter, string header, string body, params (string header, int index)[] buttons)
        {
            gameObject.SetActive(false);

            ClearAll();
            _chooseButton.gameObject.SetActive(false);

            _headerTextArea.text = header;
            _headerTextArea.alignment = TextAlignmentOptions.Right;
            if (header.ToLower() == mainCharacter.ToLower())
            {
                _headerTextArea.alignment = TextAlignmentOptions.Left;
            }
            else if (header.ToLower() == "...") 
            { 
                _headerTextArea.alignment = TextAlignmentOptions.Center;
            }

            _bodyTextArea.text = body;

            var anyButtons = buttons.Length > 0;
            _mainButton.onClick.RemoveAllListeners();
            if (!anyButtons) _mainButton.onClick.AddListener(() => onClick.Invoke(-1));

            _mainButton.gameObject.SetActive(!anyButtons);

            foreach (var (buttonHeader, index) in buttons)
            {
                var button = _disabledButtons.TryPop(out var unit) ? unit : Instantiate(_chooseButton);
                button.transform.SetParent(_chooseButton.transform.parent, false);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => onClick.Invoke(index));
                button.GetComponentInChildren<TMP_Text>(true).text = buttonHeader;
                button.gameObject.SetActive(true);

                _buttons.Push(button);
            }

            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

            await Show();
        }

        private async UniTask Show()
        {
            const int delay = 25;
            const int duration = 250;

            var timer = 0;
            transform.localScale = Vector3.zero;
            while (timer < duration) 
            {
                transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, (float)timer / duration);
                await UniTask.Delay(delay);
                timer += delay;

                LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
            }
            transform.localScale = Vector3.one;
        }
    }
}
