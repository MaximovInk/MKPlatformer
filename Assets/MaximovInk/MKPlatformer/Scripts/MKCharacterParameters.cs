using System;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    [Serializable]
    public class MKCharacterParameters
    {
        public Vector2 MaxVelocity = new Vector2(100, 100);

        public float MaxSlopeAngle = 30f;

        public float MinWallAngle = 89;
        public float MaxWallAngle = 95;


    }
}
