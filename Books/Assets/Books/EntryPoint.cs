using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Books
{
    internal sealed class EntryPoint : MonoBehaviour
    {
        [SerializeField] private Data _data;

        private ReactiveCommand _clearCash;
        private Entity _entity;

        private void OnEnable()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopHelper.Initialize(ref playerLoop);

            _clearCash = new ReactiveCommand();

            _entity = new Entity(new Entity.Ctx
            {
                ClearCash = _clearCash,
                Data = _data,
            });
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 150, 100), "Clear cash"))
            {
                _clearCash.Execute();
                Debug.Log("Clear cash done!");
            }
        }
#endif

        private void OnDisable()
        {
            _entity?.Dispose();
            _clearCash?.Dispose();
        }
    }
}

