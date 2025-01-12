using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public interface IColliderAdapter
    {
        public Vector2 Size { get; set; }
        public Vector2 Offset { get; set; }
        public Vector2 OriginalSize { get; }
        public Vector2 OriginalOffset { get; }

        public void Initialize();
        public void ResetSize();
    }
}
