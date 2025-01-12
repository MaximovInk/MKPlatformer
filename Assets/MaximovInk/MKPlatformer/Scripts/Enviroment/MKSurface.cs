using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKSurface : MonoBehaviour
    {
        public MKSurfaceParameters Parameters;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            var c = collision.collider.GetComponent<MKControllerCollision>();

            if(c != null)
            {
                var hm = c.Controller.FindAbility<MKCHorizontalMovement>();

                if(hm != null)
                {
                    hm.SetSurface(Parameters);
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            var c = collision.collider.GetComponent<MKControllerCollision>();
            if (c != null)
            {
                var hm = c.Controller.FindAbility<MKCHorizontalMovement>();

                if (hm != null)
                    hm.ResetSurface(Parameters);
            }
        }
    }
}
