using UnityEngine;

namespace MaximovInk.MKPlatformer
{
     public struct MKHitInfo
    {
        public Vector2 Normal;
        public Vector2 Point;
        public Collider2D Collider;
        public bool IsColliding;
        public float Angle;
        public float WorldAngle;
        public bool IsValidAngle;
        public float Distance;
        public float ValueOfSuccessRaycasts;

        public int RaySuccessCount;
        public int RayCount;

        public bool BottomEdge => Edge1;
        public bool TopEdge => Edge2;
        public bool LeftEdge => Edge1;
        public bool RightEdge => Edge2;

        //LEFT/RIGHT TOP/BOTTOM edges of controller

        public bool Edge1; //Left/Bottom edge
        public bool Edge2; //Right/Top edge

        public override string ToString()
        {
            return Collider == null ? "none\n" +
                                      "=\n" +
                                      "=\n" +
                                      "=\n" +
                                      "=" 
                : $"{Collider.gameObject.name}\n" +
                  $"Angle({Angle})\n" +
                  $"Distance({Distance})\n" +
                  $"Fill({ValueOfSuccessRaycasts})\n" +
                  $"Edge1 {Edge1} Edge2 {Edge2}";
        }

        public bool CheckTolerance(float tolerance)
        {
            return ValueOfSuccessRaycasts >= 1f - tolerance;
        }
    }
}
