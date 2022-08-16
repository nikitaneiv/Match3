using UnityEngine;

namespace Config.Board
{
    [CreateAssetMenu(fileName = "BoardConfig", menuName = "Configs/BoardConfig", order = 0)]
    public class BoardConfig : ScriptableObject
    {
        [SerializeField] private int sizeX = 5;
        [SerializeField] private int sizeY = 7;
        [SerializeField] private float elementOffset = 1f;

        public int SizeX => sizeX;

        public int SizeY => sizeY;

        public float ElementOffset => elementOffset;
    }
}