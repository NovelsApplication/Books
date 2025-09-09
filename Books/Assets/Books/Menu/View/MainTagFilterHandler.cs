using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Books.Menu.View
{
    public class MainTagFilterHandler : MonoBehaviour
    {
        [SerializeField] private ScreenBook _screenBook;
        [SerializeField] private RectTransform _contentRoot;
        
        public async UniTask HideIfNotTaggedAs(Entity.MainTags tag)
        {
            if (_screenBook.IsTaggedWith(tag))
            {
                _contentRoot.gameObject.SetActive(false);
                await UniTask.Yield();
                gameObject.SetActive(true);
                await UniTask.Delay(25);
                _contentRoot.gameObject.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}