using Signals;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game
{
    public class RestartButton : MonoBehaviour
    {
            private Button _button;
            private SignalBus _signalBus;

            [Inject]
            public void Construct(SignalBus signalBus)
            {
                _signalBus = signalBus;
            }


            private void Start()
            {
                _button = GetComponent<Button>();
                _button.onClick.AddListener(OnButtonClick);
            }

            private void OnDestroy()
            {
                _button.onClick.RemoveListener(OnButtonClick);
            }

            private void OnButtonClick()
            {
                _signalBus.Fire<RestartGameSignal>();
            }
    }
}