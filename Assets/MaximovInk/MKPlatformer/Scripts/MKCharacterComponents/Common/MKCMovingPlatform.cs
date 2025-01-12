using UnityEngine;

namespace MaximovInk.MKPlatformer
{
    /*
        public void SetVelocityX(float x)
        {
            _velocity.x = x ;
        }

        public void SetVelocityY(float y)
        {
            _velocity.y = y ;
        }

        public void AddVelocityX(float x)
        {
            _velocity.x += x; 
        }

        public void AddVelocityY(float y)
        {
            _velocity.y += y;
        }

        public void SetExternalVelocity(Vector2 velocity)
        {
            _externalVelocity = velocity;
        }

        public void SetExternalVelocityX(float x)
        {
            _externalVelocity.x = x;
        }

        public void SetExternalVelocityY(float y)
        {
            _externalVelocity.y = y;
        }

        private Vector2 PushVelocityToRb()
        {
            _rigidbody.linearVelocity = _tRotation * (_velocity + _externalVelocity);
        }
     */


    public class MKCMovingPlatform : MKControllerComponent
    {
        [SerializeField] private bool _saveForceOnExit = true;
        [SerializeField] private bool _stickToPlatform = true;
        [SerializeField] private float _stickForce = 0.1f;

        private IMovingPlatform _currentPlatform;
        [SerializeField] private float _platformExitDelay = 0.1f;

        private Vector3 _lastPlatformPosition;
        private Vector3 _platformVelocity;
        private bool _isOnPlatform;

        private float _platformExitTimer;

        private IMovingPlatform FindPlatform()
        {
            if (Controller.IsCollisionBelow)
            {
                var platform = Controller.CollisionState.Below.Collider.gameObject.GetComponent<IMovingPlatform>();

                if (platform != null)
                {
                    return platform;
                }
            }

            return null;
        }

        private void EnterPlatform()
        {
            _lastPlatformPosition = _currentPlatform.Position;
        }

        private void ProcessPlatform()
        {
            var platformPosition = _currentPlatform.Position;
            var delta = platformPosition - _lastPlatformPosition;
            _platformVelocity = new Vector2(delta.x / Controller.DeltaTime, delta.y / Controller.DeltaTime);
          
            Controller.SetExternalVelocityX(_platformVelocity.x);
            Controller.SetExternalVelocityY(Controller.IsJumping? 0 : _platformVelocity.y);

            if (_stickToPlatform && !Controller.IsJumping)
            {
                if (_platformVelocity.y < 0)
                {
                    Controller.AddVelocityY(Mathf.Min(_platformVelocity.y, -_stickForce));
                }
            }


            _lastPlatformPosition = platformPosition;
        }

        private void LeavePlatform()
        {
            if(_saveForceOnExit)
                Controller.AddVelocity(_platformVelocity * 1.5f);

            Controller.SetExternalVelocity(Vector2.zero);
        }

        public override void Process(float deltaTime)
        {
            base.Process(deltaTime);

            var foundedPlatform = FindPlatform();

            if (foundedPlatform != null)
            {
                _currentPlatform = foundedPlatform;
                _platformExitTimer = 0f;
                if (!_isOnPlatform)
                {
                    EnterPlatform();
                }

                _isOnPlatform = true;
            }
            else if (_isOnPlatform)
            {
                _platformExitTimer += Time.deltaTime;
                if (_platformExitTimer >= _platformExitDelay)
                {
                    if (_currentPlatform != null)
                    {
                        LeavePlatform();
                    }

                    _isOnPlatform = false;
                    _currentPlatform = null;
                }
            }

            if (_isOnPlatform)
            {
                ProcessPlatform();
            }
        }

    }
}