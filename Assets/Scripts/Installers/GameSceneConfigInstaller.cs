using Game;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameSceneConfigInstaller", menuName = "Installers/GameSceneConfigInstaller")]
public class GameSceneConfigInstaller : ScriptableObjectInstaller<GameSceneConfigInstaller>
{
    [SerializeField] private ElementsConfig elementsConfig;
    public override void InstallBindings()
    {
        Container.BindInstance(elementsConfig);
    }
}