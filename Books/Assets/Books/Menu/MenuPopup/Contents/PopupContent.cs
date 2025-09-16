using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Books.Menu.MenuPopup.Contents
{
    public interface IPopupContent
    {
        public PopupType PopupType { get; }
        public IObservable<Unit> Configure(IPopupContentData data, UniversalPopup root, List<MenuPopup.Data> configs);
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

        public IObservable<Unit> Configure(IPopupContentData data, UniversalPopup root, List<MenuPopup.Data> configs)
        {
            ClearContent();
            
            var closeParentRequest = new Subject<Unit>();

            if (data is TData tData)
            {
                ContentData = tData;
                Root = root;
                Configs = configs;
                OnConfigure(closeParentRequest);
            }
            else
            {
                Debug.LogErrorFormat($"Несовместимый тип данных: ожидается {typeof(TData).Name}", nameof(data));
            }

            return closeParentRequest;
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

        protected virtual void OnConfigure(Subject<Unit> closeParentRequest) { }
        protected virtual void OnClearContent() { }
    }
}