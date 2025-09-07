using System;
using UnityEngine;

namespace Books.Menu.MenuPopup
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private RectTransform _popupContentPrefab;
        [SerializeField] private PopupType _popupType;

        public readonly RectTransform PopupContentPrefab => _popupContentPrefab;
        public readonly PopupType PopupType => _popupType;
    }
    
    public enum PopupType
    {
        None,
        OneButtonWarning,
        ScreenBook,
        TwoButtonWarning,
        Settings,
    }
}