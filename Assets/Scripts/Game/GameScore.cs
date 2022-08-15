using System;
using Signals;
using TMPro;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameScore : MonoBehaviour
    {
        private TextMeshProUGUI _text;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }


        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _signalBus.Subscribe<ScoreChangedSignal>(OnScoreChanged);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<ScoreChangedSignal>(OnScoreChanged);
        }

        private void OnScoreChanged(ScoreChangedSignal signal)
        {
            _text.text = signal.Value.ToString();
        }
    }
}