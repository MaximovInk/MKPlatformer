using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public static class Utility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float GetJumpForce(float height, float gravityForce)
        {
            return Mathf.Sqrt(-2.0f * gravityForce * height);
        }
    }
}
