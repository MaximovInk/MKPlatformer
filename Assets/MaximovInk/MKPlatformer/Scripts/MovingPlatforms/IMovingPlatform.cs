using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public interface IMovingPlatform
    {
        public Vector2 GetSpeed { get; }

        public Vector3 Position { get; }

        public Vector3 GetDestination { get; }

        public Transform Transform { get; }
    }
}