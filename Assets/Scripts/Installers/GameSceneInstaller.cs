using Game;
using UnityEngine;
using Zenject;

public class GameSceneInstaller : MonoInstaller
{
    [SerializeField] private Element elementPrefab;
    [SerializeField] private GameObject  ui;
    public override void InstallBindings()
    {
        Container.BindInstance(ui);
        Container.BindFactory<ElementPosition ,ElementConfigItem,Element,Element.Factory>().FromComponentInNewPrefab(elementPrefab);
    }
}