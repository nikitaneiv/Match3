using Config.Board;
using Game;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "GameSceneConfigInstaller", menuName = "Installers/GameSceneConfigInstaller")]
public class GameSceneConfigInstaller : ScriptableObjectInstaller<GameSceneConfigInstaller>
{
    [SerializeField] private ElementsConfig elementsConfig;
    [SerializeField] private BoardConfig boardConfig;
    public override void InstallBindings()
    {
        Container.BindInstances(elementsConfig,boardConfig);
    }
}