using System;
using System.Collections.Generic;
using Books.Menu.MenuPopup.Contents;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Books.Menu.MenuPopup
{
    public class PopupFactory
    {
        private List<Data> _popupData;

        public PopupFactory(List<Data> popupData)
        {
            _popupData = popupData;
        }

        public IPopupContent OpenPopup(PopupType popupType, UniversalPopup popupRoot, IPopupContentData data = null)
        {
            Data popupConfig = _popupData.Find(i => i.PopupType == popupType);

            if (popupConfig.PopupContentPrefab == null)
            {
                throw new Exception($"Не найдет попап с типом [{popupType}] в данных");
            }

            RectTransform contentInstance = Object.Instantiate(popupConfig.PopupContentPrefab, popupRoot.transform, false);
            IPopupContent popupContent = contentInstance.GetComponent<IPopupContent>();
            
            popupRoot.SetPopupContent(contentInstance, popupContent);
            
            if (data != null)
                popupContent.Configure(data, popupRoot);
            
            popupRoot.Show().Forget();

            return popupContent;
        }
    }
}