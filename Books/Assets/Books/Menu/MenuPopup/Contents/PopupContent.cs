using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Books.Menu.MenuPopup.Contents
{
    public interface IPopupContent
    {
        public PopupType PopupType { get; }
        public void Configure(IPopupContentData data, UniversalPopup root, List<MenuPopup.Data> configs);
        public void ClearContent();
    }

    public interface IPopupContentData { }

    public abstract class PopupContent<TData> : MonoBehaviour, IPopupContent 
        where TData : IPopupContentData
    {
        public abstract PopupType PopupType { get; }
        
        protected TData ContentData { get; private set; }
        protected UniversalPopup Root { get; private set; }
        protected List<MenuPopup.Data> Configs { get; private set; }

        public void Configure(IPopupContentData data, UniversalPopup root, List<MenuPopup.Data> configs)
        {
            ClearContent();
            
            if (data is TData tData)
            {
                ContentData = tData;
                Root = root;
                Configs = configs;
                OnConfigure();
            }
            else
            {
                Debug.LogErrorFormat($"Несовместимый тип данных: ожидается {typeof(TData).Name}", nameof(data));
            }
        }

        public void ClearContent()
        {
            if (ContentData != null)
            {
                ContentData = default;
                Root = null;
                OnClearContent();
            }
        }

        protected virtual void OnConfigure() { }
        protected virtual void OnClearContent() { }
    }
}