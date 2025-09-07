using UnityEngine;

namespace Books.Menu.MenuPopup.Contents
{
    public interface IPopupContent
    {
        public PopupType PopupType { get; }
        public void Configure(IPopupContentData data, UniversalPopup root);
        public void ClearContent();
    }

    public interface IPopupContentData { }

    public abstract class PopupContent<TData> : MonoBehaviour, IPopupContent 
        where TData : IPopupContentData
    {
        public abstract PopupType PopupType { get; }
        
        protected TData ContentData { get; private set; }
        protected UniversalPopup Root { get; private set; }

        public void Configure(IPopupContentData data, UniversalPopup root)
        {
            ClearContent();
            
            if (data is TData tData)
            {
                ContentData = tData;
                Root = root;
                OnConfigure();
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