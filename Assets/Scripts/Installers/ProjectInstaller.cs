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
        
    }

   
} 