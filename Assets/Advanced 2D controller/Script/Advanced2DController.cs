using UnityEngine;
namespace Advanced2DPlayerController
{
    public class Advanced2DController : AdvancedRaycastController
    {


        public struct CollisionInfo
        {
            public bool above, below, left, right, climbingSlope, descendingSlope;
            public float slopeAngle, slopeAngleOld;
            public Vector3 velocityOld;
            public void Reset()
            {
                above = below = left = right = climbingSlope = descendingSlope = false;
                slopeAngleOld = slopeAngle;
                slopeAngle = 0;
            }
        }


        [SerializeField]
        private Transform transformShadow;

        [SerializeField]
        private float maxClimbAngle = 80f;
        [SerializeField]
        private float maxDescendAngle = 80f;

        private bool lookRight = true;
        
        private CollisionInfo collisions;

        private Vector2 playerInput;


        public CollisionInfo Collisions
        {
            get
            {
                return collisions;
            }

            set
            {
                collisions = value;
            }
        }

        public Vector2 PlayerInput
        {
            get
            {
                return playerInput;
            }

            set
            {
                playerInput = value;
            }
        }

        public bool LookRight
        {
            get
            {
                return lookRight;
            }

            set
            {
                lookRight = value;
            }
        }

        public override void Awake()
        {
            base.Awake();
            lookRight = true;
        }

        public void Move(Vector3 velocity, Vector2 input, Animator anim)
        {
            UpdateRaycastOrigins();
            playerInput = input;
            collisions.Reset();

            collisions.velocityOld = velocity;
            if (velocity.y < 0)
                DescendSlope(ref velocity);
            if (velocity.x != 0)
            {
                if(Mathf.Sign(velocity.x) > 0)
                {
                    lookRight = true;
                    transformShadow.eulerAngles = new Vector3(0, 0, 0);
                }
                else
                {
                    lookRight = false;
                    transformShadow.eulerAngles = new Vector3(0, 180, 0);
                }

            }
            HorizontalCollisions(ref velocity);
            if (velocity.y != 0)
                VerticalCollisions(ref velocity);
            if (collisions.below)
            {
                if ((velocity.x <= 0.01f && velocity.x >= -0.01f))
                    anim.Play("Idle",-1,0);
                else
                {
                    anim.Play("Move");
                }
            }
            transform.Translate(velocity);
        }


        #region Collision
        private void HorizontalCollisions(ref Vector3 velocity)
        {
            float directionX = lookRight ? 1 : -1;
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            if(Mathf.Abs(velocity.x) < skinWidth)
                rayLength = 2 * skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                if (hit)
                {
                    if (hit.collider.tag == "Through")
                        if (hit.distance == 0)
                            continue;
                    // Get the angle of the surface we hit by using the normal for the surface we hit and the normal of the ground
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                    if (i == 0 && slopeAngle <= maxClimbAngle)
                    {
                        if (collisions.descendingSlope)
                        {
                            collisions.descendingSlope = false;
                            velocity = collisions.velocityOld;
                        }
                        float distanceToSlopeStart = 0;
                        if (slopeAngle != collisions.slopeAngleOld)
                        {
                            distanceToSlopeStart = hit.distance - skinWidth;
                            velocity.x -= distanceToSlopeStart * directionX;
                        }
                        ClimbSlope(ref velocity, slopeAngle);
                        velocity.x += distanceToSlopeStart * directionX;
                    }

                    if (!collisions.climbingSlope || slopeAngle > maxClimbAngle)
                    {
                        velocity.x = (hit.distance - skinWidth) * directionX;
                        rayLength = hit.distance;

                        if (collisions.climbingSlope)
                        {
                            velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                        }

                        collisions.left = directionX == -1;
                        collisions.right = directionX == 1;
                    }
                }
            }
        }

        private void VerticalCollisions(ref Vector3 velocity)
        {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

                if (hit)
                {
                    if (hit.collider.tag == "Through")
                    {
                        if (directionY == 1 || hit.distance == 0)
                            continue;
                        if (playerInput.y == -1)
                            continue;
                    }
                    
                    velocity.y = (hit.distance - skinWidth) * directionY;
                    rayLength = hit.distance;

                    if (collisions.climbingSlope)
                    {
                        velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                    }

                    collisions.below = directionY == -1;
                    collisions.above = directionY == 1;
                }
            }

            if (collisions.climbingSlope)
            {
                float directionX = Mathf.Sign(velocity.x);
                rayLength = Mathf.Abs(velocity.x) + skinWidth;
                Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
                if (hit)
                {
                    float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                    if (slopeAngle != collisions.slopeAngle)
                    {
                        velocity.x = (hit.distance - skinWidth) * directionX;
                        collisions.slopeAngle = slopeAngle;
                    }
                }
            }
        }
        #endregion



        #region Slope
        private void ClimbSlope(ref Vector3 velocity, float slopeAngle)
        {
            float moveDistance = Mathf.Abs(velocity.x);
            float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

            if (velocity.y < climbVelocityY)
            {
                velocity.y = climbVelocityY;
                velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                collisions.below = true;
                collisions.climbingSlope = true;
                collisions.slopeAngle = slopeAngle;
            }
        }


        private void DescendSlope(ref Vector3 velocity)
        {
            float directionX = Mathf.Sign(velocity.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, 20f, collisionMask);

            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != 0 && slopeAngle <= maxDescendAngle)
                {
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                        {
                            float moveDistance = Mathf.Abs(velocity.x);
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                            velocity.y -= descendVelocityY;

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                        }
                    }
                }
            }
        }
        #endregion
    }
}