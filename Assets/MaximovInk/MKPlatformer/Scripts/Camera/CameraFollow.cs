using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class CameraFollow : MonoBehaviour
    {
        public float LookDistance = 1f;
        public float SmoothTime = 0.3f;
        public Transform Target;

        private Vector3 _refVelocity;

        private bool _isLooking;

        [SerializeField] private Vector3 _offset;

        private Vector3 _delta;

        [Range(0,1)]
        [SerializeField] private float _distanceApply = 1f;

        private void Update()
        {
            if (Target == null)
                return;

            var transformPos = transform.position;

            _isLooking = MKInputManager.Instance.State.GetButton("Look").IsPressed;

            var mousePos = Input.mousePosition / new Vector2(Screen.width, Screen.height);

            var targetPos = Target.transform.position;

            mousePos = new Vector2((mousePos.x - 0.5f) * 2f, (mousePos.y - 0.5f) * 2f);

            if (_isLooking)
                _delta = mousePos * LookDistance;

            var finalTarget = targetPos + _delta + _offset;
            // var distance = Vector2.Distance(finalTarget, transformPos);

            // var lT = _distanceApply / Mathf.Clamp(distance,0.05f,1f);

            // var t = Mathf.Lerp(SmoothTime, 0, lT);

            var distance = Vector2.Distance(finalTarget, transformPos);

            distance = Mathf.Clamp(distance, 0, 3);
            var dT = distance / 2;



            var t = Mathf.Lerp(SmoothTime, 0, dT*_distanceApply);

            var position = transformPos;
            position = Vector3.SmoothDamp(position, finalTarget, ref _refVelocity, t);

            position.z = transformPos.z;

            transform.position = position;
        }

    }
}
