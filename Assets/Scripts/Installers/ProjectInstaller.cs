using Game;
using Signals;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        SignalBusInstaller.Install(Container);
        Container.Bind<SaveSystem>().AsSingle().NonLazy();
        Container.BindInterfacesTo<GameManager>().AsSingle().NonLazy();
        BindSignals();
    }

    private void BindSignals()
    {
        Container.DeclareSignal<CreateGameSignal>();
        Container.DeclareSignal<OnElementSignal>();
        Container.DeclareSignal<ScoreChangedSignal>();
        Container.DeclareSignal<RestartGameSignal>();
        Container.DeclareSignal<AddScoreSignal>();
        
    }
} 