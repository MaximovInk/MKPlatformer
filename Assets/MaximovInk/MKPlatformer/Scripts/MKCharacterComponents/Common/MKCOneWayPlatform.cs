using System.Collections.Generic;
using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    public class MKCOneWayPlatform : MKControllerComponent
    {
        [SerializeField] private MKCharacterButton _downButton = new() { ID = "OneWay" };
        [SerializeField] private string _oneWayTag = "OneWayPlatform";

        private readonly List<Collider2D> _ignoredPlatforms = new();

        private readonly Collider2D[] _raycastCache = new Collider2D[7];

        private void AddIfOneWayPlatform(Collider2D collider)
        {
            if (collider.CompareTag(_oneWayTag))
            {
                AddOneWayPlatform(collider);
            }
        }

        private void AddOneWayPlatform(Collider2D collider)
        {
            if (_ignoredPlatforms.Contains(collider)) return;

            _ignoredPlatforms.Add(collider);

            Collision.AddIgnoreCollider(collider);
        }

        private void RemoveOneWayPlatform(Collider2D collider)
        {
            _ignoredPlatforms.Remove(collider);

            Collision.RemoveIgnoreCollider(collider);
        }

        public override void HandleInput(ICharacterInput input)
        {
            base.HandleInput(input);

            _downButton.ReadFrom(input);
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var invokeOneWay = _downButton.ReadState();
            var obstaclesMask = Raycast.ObstaclesMask;
            var state = Controller.CollisionState;
            var velocity = Controller.Velocity;

            if (_ignoredPlatforms.Count > 0)
            {
               // var size = Physics2D.OverlapBoxNonAlloc(transform.position + (Vector3)Collision.Offset, Collision.Size * 1.1f, 0, _raycastCache, obstaclesMask);

               var filter = new ContactFilter2D().NoFilter();
               filter.SetLayerMask(obstaclesMask);

               var size= Physics2D.OverlapBox(transform.position + (Vector3)Collision.Offset, Collision.Size * 1.2f, 0,
                    filter, _raycastCache);

               for (int i = 0; i < _ignoredPlatforms.Count; i++)
               {
                   var platform = _ignoredPlatforms[i];

                   if (platform == state.Above.Collider) continue;
                   if (platform == state.Below.Collider) continue;

                   bool remove = true;

                   for (int j = 0; j < size; j++)
                   {
                       var resultCollider = _raycastCache[j];

                       if(Collision.IsThis(resultCollider))continue;

                       if (resultCollider == platform)
                       {

                           remove = false;
                           break;
                       }
                   }

                   if (remove)
                   {
                      //  Debug.Log($"remove {platform.name}");
                        RemoveOneWayPlatform(platform);
                       i--;
                   }
               }


            }

            if (invokeOneWay && state.Below.IsColliding)
            {
                var bottom = state.Below.Collider;
                AddIfOneWayPlatform(bottom);
            }

            if ((velocity.y > 0 || Mathf.Abs(velocity.x) > 0f) && state.Above.IsColliding)
            {
                var top = state.Above.Collider;
               // Debug.Log($"add {top.gameObject.name}");
                AddIfOneWayPlatform(top);
            }

            var characterMin = Controller.transform.position.y + Collision.Offset.y - Collision.Size.y / 2.5f;

            if ((velocity.x < 0) && state.Left.IsColliding && (state.Left.Point.y > characterMin || invokeOneWay))
            {
                var left = state.Left.Collider;
                AddIfOneWayPlatform(left);
            }

            if ((velocity.x > 0) && state.Right.IsColliding && (state.Right.Point.y > characterMin || invokeOneWay))
            {
                var right = state.Right.Collider;
                AddIfOneWayPlatform(right);
            }

            _downButton.Flush();
            
        }
    }
}
