using MaximovInk.MKPlatformer;
using UnityEngine;

namespace MaximovInk
{
    public class MKCHorizontalMovementMomentum : MKCHorizontalMovement
    {
        protected float _momentum;

        public override void Process(float deltaTime)
        {
            var xForce = _moveInput * _speed * CustomMultiplier;

            var grounded = Controller.IsGrounded;

            // Momentum handling
            if (_lastGrounded != grounded)
            {
                if (_lastGrounded) // Going into air
                {
                    _momentum = Controller.Velocity.x; // Preserve ground momentum
                    _lastHorizontalForce = xForce;
                }
                else // Landing from air
                {
                    _momentum = Mathf.Clamp(_momentum, -_speed, _speed); // Clamp to max speed for realism
                    Controller.SetVelocityX(_momentum);
                }
            }

            var acceleration = AirAcceleration;
            var deceleration = AirDeceleration;

            // Adjust momentum based on new input
            if (grounded)
            {
                var surfaceParams = GetCurrentSurfaceParameters();
                acceleration = surfaceParams.Acceleration;
                deceleration = surfaceParams.Deceleration * (1 + surfaceParams.FrictionCoefficient);

                // Here we make the movement more responsive by significantly influencing momentum with input
                if (Mathf.Abs(xForce) > MOVEMENT_THRESHOLD)
                {
                    _momentum = Mathf.Lerp(_momentum, xForce, acceleration * deltaTime * 3f); // Increased responsiveness
                }
                else
                {
                    _momentum = Mathf.Lerp(_momentum, 0, deceleration * deltaTime * 2f); // Faster stop when no input
                }

                Controller.SetVelocityX(_momentum);
            }
            else // In air
            {
                // Use air control to blend between current momentum and desired input
                _momentum = Mathf.Lerp(_momentum, xForce, _airControl * deltaTime * 2f); // More air control
                Controller.SetVelocityX(_momentum);
            }

            _isMoving = Mathf.Abs(Controller.Velocity.x) > MOVEMENT_THRESHOLD && grounded;

            var moveNormal = Controller.CollisionState.Below.Normal;

            if (Mathf.Abs(xForce) > MOVEMENT_THRESHOLD)
            {
                if (_slopesStick
                    && Controller.IsGroundedAndSlopeOk
                    && Mathf.Abs(moveNormal.y) > SLOPE_STICK_NORMAL_Y_THRESHOLD
                    && Controller.CollisionState.SlopeAngle > SLOPE_STICK_THRESHOLD)
                {
                    SlopeStickMovement(moveNormal, _momentum, grounded ? acceleration : AirAcceleration);
                    _isMoving = true;
                }
                else if (Controller.IsGroundedAndSlopeOk
                         || !Controller.IsGrounded
                         || (Controller.IsGrounded && _canMoveOnBadSlopes))
                {
                    DefaultMovement(_momentum, grounded ? acceleration : AirAcceleration);
                    _isMoving = true;
                }
            }

            _lastGrounded = grounded;

            if (_applyMoveDirection && _isMoving)
                ApplyMovementDirection();

            UpdateState();
        }

    }
}
