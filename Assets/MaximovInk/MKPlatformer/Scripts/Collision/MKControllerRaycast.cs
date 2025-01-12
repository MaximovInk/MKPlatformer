using System;
using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKControllerRaycast : MonoBehaviour
    {
        public LayerMask ObstaclesMask => _obstaclesMask;

        [SerializeField] private LayerMask _obstaclesMask;

        [Header("Count of rays")]
        public int VerticalRaysCount = 8;
        public int HorizontalRaysCount = 8;

        [Header("Offset rays 'inside' of character")]
        public float RayOffsetH = -0.15f;
        public float RayOffsetV = -0.15f;

        [Header("Offset rays of character corners")]
        public float RayOffsetBorderH = -0.05f;
        public float RayOffsetBorderV = -0.05f;

        [Header("Extra size of rays")]
        public float RayExtraSizeH = 0f;
        public float RayExtraSizeV = 0f;

        private readonly RaycastHit2D[] _resultCache = new RaycastHit2D[8];

        private MKControllerCollision _collision;
        private MKCharacterController _controller;
        private Transform _transform;

        private Vector2 _min;
        private Vector2 _max;
        private Vector2 _direction;

        private List<RaycastHit2D> _tempRaycasts = new();
        private readonly List<Collider2D> _ignoredColliders = new();
        private bool _edge1Detected = false;
        private bool _edge2Detected = false;
        private int _raySuccessCount = 0;

        public void Initialize(MKCharacterController controller)
        {
            _controller = controller;
            _collision = controller.Collision;
            _transform = controller.transform;
        }

        private void CalculateMinMax(MKSide side, float offsetBorders, float offsetInside)
        {
            var halfWidth = _collision.HalfWidth;
            var halfHeight = _collision.HalfHeight;

            switch (side)
            {
                case MKSide.Top:
                    _min = new Vector2(-halfWidth - offsetBorders, halfHeight + offsetInside);
                    _max = new Vector2(+halfWidth + offsetBorders, halfHeight + offsetInside);
                    _direction = _transform.up;
                    break;
                case MKSide.Bottom:
                    _min = new Vector2( - halfWidth - offsetBorders, -halfHeight - offsetInside);
                    _max = new Vector2( + halfWidth + offsetBorders, -halfHeight - offsetInside);
                    _direction = -_transform.up;
                    break;
                case MKSide.Left:
                    _min = new Vector2(-halfWidth - offsetInside,  - halfHeight - offsetBorders);
                    _max = new Vector2(-halfWidth - offsetInside,  + halfHeight + offsetBorders);
                    _direction = -_transform.right;
                    break;
                case MKSide.Right:
                    _min = new Vector2(halfWidth + offsetInside,  - halfHeight - offsetBorders);
                    _max = new Vector2(halfWidth + offsetInside,  + halfHeight + offsetBorders);
                    _direction = _transform.right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side), side, null);
            }
        }

        public void CheckCollisionsAtState(MKSide side, ref List<RaycastHit2D> result, float distance,
            int rayCount, LayerMask obstaclesMask, float offsetBorders = 0, float offsetInside = 0)
        {
            _edge1Detected = false;
            _edge2Detected = false;
            _raySuccessCount = 0;

            CalculateMinMax(side, offsetBorders, offsetInside);

            _min = GetRayOrigin(_min);
            _max = GetRayOrigin(_max);

            result.Clear();

            for (var i = 0; i < rayCount; i++)
            {
                var t = ((float)i / (rayCount - 1));
                var rayOrigin = Vector2.Lerp(_min, _max, t);

                var size = Physics2D.RaycastNonAlloc(rayOrigin, _direction, _resultCache, distance, obstaclesMask);

                if (size <= 0)
                {
                    Debug.DrawRay(rayOrigin, _direction * distance, Color.green);
                    continue;
                }

                var found = false;
                for (int j = 0; j < size; j++)
                {
                    if (_ignoredColliders.Contains(_resultCache[j].collider))
                        continue;

                    if (_collision.IsIgnore(_resultCache[j].collider))
                        continue;

                    if (_collision.IsThis(_resultCache[j].collider))
                        continue;

                    result.Add(_resultCache[j]);
                    found = true;
                }

                if (i == 0)
                    _edge1Detected = found;

                if (i == rayCount-1)
                    _edge2Detected = found;

                if (found)
                    _raySuccessCount++;

                Debug.DrawRay(rayOrigin, _direction * distance, found ? Color.red : Color.green);
            }
        }

        private bool IsHorizontal(MKSide side) => side is MKSide.Left or MKSide.Right;

        public Vector2 GetRayOrigin(Vector2 localOrigin)
        {
            var colliderOffset = _collision.Offset;
            var pos = _transform.position;

            localOrigin += colliderOffset;

            var rot = _transform.rotation;

            localOrigin = (rot * localOrigin) + pos;

            return localOrigin;
        }

        public MKHitInfo CheckRay(Vector2 origin, float raySize)
        {
            var size = Physics2D.RaycastNonAlloc(origin, _direction, _resultCache, raySize, _obstaclesMask);

            if (size <= 0)
            {
                Debug.DrawRay(origin, _direction * raySize, Color.green);
                return default;
            }

            var found = false;

            for (int j = 0; j < size; j++)
            {
                if (_ignoredColliders.Contains(_resultCache[j].collider))
                    continue;

                if (_collision.IsIgnore(_resultCache[j].collider))
                    continue;

                if (_collision.IsThis(_resultCache[j].collider))
                    continue;

                found = true;
                break;
            }

            if (!found)
            {
                return default;
            }

            var hit = _resultCache[0];

            var angle = Vector2.Angle(hit.normal, _transform.up);
            var worldAngle = Vector2.Angle(hit.normal, Vector2.up);
            var distance = Mathf.Abs(hit.distance);

            Debug.DrawRay(origin, _direction * raySize, Color.red);

            return new MKHitInfo()
            {
                Angle = angle,
                Collider = hit.collider,
                Distance = distance,
                IsColliding = true,
                Normal = hit.normal,
                Point = hit.point,
                RayCount = 1,
                RaySuccessCount = 1,
                WorldAngle = worldAngle
            };
        }

        public MKHitInfo CheckRays(MKSide side, float raySize, int rayCount, float minAngle, float maxAngle)
        {
            var isH = IsHorizontal(side);

            var offsetBorders = isH ? RayOffsetBorderH : RayOffsetBorderV;
            var offsetInside = isH ? RayOffsetH : RayOffsetV;

            CheckCollisionsAtState(side, ref _tempRaycasts, raySize,
                rayCount, _obstaclesMask, offsetBorders, offsetInside);

            if (_tempRaycasts.Count <= 0) return default;

            var hit = _tempRaycasts[0];

            var angle  = Vector2.Angle(hit.normal, _transform.up);
            var worldAngle = Vector2.Angle(hit.normal, Vector2.up);

            var fill = _tempRaycasts.Count / (float)rayCount;

            var distance = 0f;

            for (int i = 0; i < _tempRaycasts.Count; i++)
            {
                distance += Mathf.Abs(Mathf.Abs(hit.distance) - Mathf.Abs(offsetInside));
            }

            distance /= _tempRaycasts.Count;

            return new MKHitInfo
            {
                Normal = hit.normal,
                Point = hit.point,
                Collider = hit.collider,
                Angle = angle,
                WorldAngle = worldAngle,
                IsColliding = true,
                Distance = distance,
                IsValidAngle =
                    angle >= minAngle &&
                    angle <= maxAngle,
                ValueOfSuccessRaycasts = fill,
                RaySuccessCount = _raySuccessCount,
                RayCount = rayCount,
                Edge1 = _edge1Detected,
                Edge2 = _edge2Detected,
            };

        }

        public void AddIgnore(Collider2D ignore)
        {
            if (_ignoredColliders.Contains(ignore)) return;

            _ignoredColliders.Add(ignore);
        }

        public void RemoveIgnore(Collider2D ignore)
        {
            if (!_ignoredColliders.Contains(ignore)) return;

            _ignoredColliders.Remove(ignore);
        }

        public MKHitInfo CheckRays(MKSide side, float extraSize = 0f)
        {
            var raySize = 0.30f;
            raySize += IsHorizontal(side) ? RayExtraSizeH : RayExtraSizeV;
            raySize += extraSize;
            return CheckRays(side, raySize, -360, +360);
        }

        public MKHitInfo CheckRays(MKSide side, float minAngle,
            float maxAngle)
        {
            var raySize = 0.30f;
            raySize += IsHorizontal(side) ? RayExtraSizeH : RayExtraSizeV;
            return CheckRays(side, raySize, minAngle, maxAngle);
        }

        public MKHitInfo CheckRays(MKSide side, float raySize, float minAngle,
            float maxAngle)
        {
            var isH = IsHorizontal(side);
            var rayCount = isH ? HorizontalRaysCount : VerticalRaysCount;
            raySize += isH ? RayExtraSizeH : RayExtraSizeV;
            return CheckRays(side, raySize, rayCount, minAngle, maxAngle);
        }



    }
}
