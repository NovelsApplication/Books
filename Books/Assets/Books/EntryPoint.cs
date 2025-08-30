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

//#if UNITY_EDITOR
        private bool _showCheats;
        private void OnGUI()
        {
            if (GUI.Button(new Rect(10, 10, 15, 15), _showCheats ? ">" : "<")) 
            {
                _showCheats = !_showCheats;
            }

            if (!_showCheats) return;

            if (GUI.Button(new Rect(10, 25, 150, 100), "Clear cash"))
            {
                _clearCash.Execute();
                Debug.Log("Clear cash done!");
            }
        }
//#endif

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

        private void OnDisable()
        {
            _entity?.Dispose();
            _clearCash?.Dispose();
        }
    }
}

