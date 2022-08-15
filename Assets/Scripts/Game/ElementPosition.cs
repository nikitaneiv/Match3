using UnityEngine;

namespace Game
{
    public class ElementPosition
    {
        public readonly Vector2 LocalPosition;
        public readonly Vector2 GridPosition;

        public ElementPosition(Vector2 localPosition, Vector2 gridPosition)
        {
            LocalPosition = localPosition;
            GridPosition = gridPosition;
        }
    }
}