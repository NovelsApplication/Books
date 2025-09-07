using System;
using System.Collections.Generic;
using UnityEngine;

namespace Books.Menu
{
    [Serializable]
    public struct Data
    {
        [SerializeField] private string _screenName;
        [SerializeField] private List<Menu.MenuPopup.Data> _popupData;

        public readonly string ScreenName => _screenName;
        public readonly List<Menu.MenuPopup.Data> PopupData => _popupData;

    }
}