using System;
using Signals;
using UnityEngine;
using Zenject;

namespace Game
{
    public class GameManager: IInitializable, IDisposable 
    {
        private readonly SaveSystem _saveSystem;
        private readonly SignalBus _signalBus;

        private int _score = -1;
        
        private int Score
        {
            get => _score;
            set
            {
                if (value == _score)
                    return;
                
                _score = value;
                _signalBus.Fire(new ScoreChangedSignal(_score));
            }
        }

        public GameManager(SaveSystem saveSystem, SignalBus signalBus)
        {
            _saveSystem = saveSystem;
            _signalBus = signalBus;
        }

        public void Initialize()
        {
            _saveSystem.Initialize();
            SubscribeSignals();
            Score = _saveSystem.Data.Score;
        }
        
        public void Dispose()
        {
            UnSubscribeSignals(); 
        }

        private void SubscribeSignals()
        {
            _signalBus.Subscribe<RestartGameSignal>(OnRestart);
            _signalBus.Subscribe<AddScoreSignal>(OnAddScore);
        }
        
        private void UnSubscribeSignals()
        {
            _signalBus.Unsubscribe<RestartGameSignal>(OnRestart);
            _signalBus.Unsubscribe<AddScoreSignal>(OnAddScore);
        }

        private void OnAddScore(AddScoreSignal signal)
        {
            Score += signal.Value;
        }


        private void OnRestart() 
        {
            Debug.Log("Restart Game Invoke");
        }
    }
}