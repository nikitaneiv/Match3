using Game;
using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<SaveSystem>().AsSingle().NonLazy();
        Container.BindInterfacesTo<GameManager>().AsSingle().NonLazy();
    }
}