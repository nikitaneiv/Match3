using Game;
using Signals;
using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField] private Element elementPrefab;
    [SerializeField] private GameObject  ui;
    public override void InstallBindings()
    {
        BindSignals();
        Container.BindInstance(ui);
        Container.BindFactory<ElementPosition ,ElementConfigItem,Element,Element.Factory>().FromComponentInNewPrefab(elementPrefab);
        Container.Bind<BoardController>().AsSingle().NonLazy();
        Container.BindInterfacesTo<GameManager>().AsSingle().NonLazy();
    }
    private void BindSignals()
    {
        Container.DeclareSignal<CreateGameSignal>();
        Container.DeclareSignal<OnElementSignal>();
        Container.DeclareSignal<ScoreChangedSignal>();
        Container.DeclareSignal<RestartGameSignal>();
        Container.DeclareSignal<AddScoreSignal>();
        Container.DeclareSignal<OnBoardClosedSignal>();
        Container.DeclareSignal<OnBoardMatchSignal>();
        
    }
}