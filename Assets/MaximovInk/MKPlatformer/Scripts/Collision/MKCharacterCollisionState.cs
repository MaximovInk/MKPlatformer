using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [System.Serializable]
    public class MKCharacterCollisionState
    {
        public bool HasCollisions => Below.IsColliding || Left.IsColliding || Right.IsColliding || Above.IsColliding;

        public MKHitInfo Left { get; set; }

        public MKHitInfo Right { get; set; }

        public MKHitInfo Above { get; set; }

        public MKHitInfo Below { get; set; }

        public void Reset()
        {
            Left = default;
            Right = default;
            Above = default;
            Below = default;
        }

        public override string ToString()
        {
            return $"\nLeft:\n{Left}\nRight:\n{Right}\nTop:\n{Above}\nBottom:\n{Below}";
        }

        public bool SlopeAngleIsOk;
        public float SlopeAngle;

        public bool IsJumping;
        public bool IsFalling;
        public bool JustGotGrounded;
    }
}
