using UnityEngine;

namespace Advanced2DPlayerController
{
    [RequireComponent(typeof(Advanced2DController))]
    public class Advanced2DPlayer : MonoBehaviour
    {

        [SerializeField]
        private float maxJumpHeight = 4f;
        [SerializeField]
        private float minJumpHeight = 1f;
        [SerializeField]
        private float timeToJumpApex = .4f;
        [SerializeField]
        private float moveSpeed = 15f;

        [SerializeField]
        private float wallSlideSpeedMax = 10f;
        [SerializeField]
        private Vector2 wallJumpClimb;
        [SerializeField]
        private Vector2 wallJumpOff;
        [SerializeField]
        private Vector2 wallLeap;
        [SerializeField]
        private float wallStickTime = .25f;


        [SerializeField]
        private SpriteRenderer playerColor;


        private float timeToWallUnstick;


        private float accelerationTimeAirBorne = .2f;
        private float accelerationTimeGrounded = .1f;

        private float maxJumpVelocity;
        private float minJumpVelocity;
        private float gravity;
        private Vector3 velocity;
        private Advanced2DController controller;
        private Vector2 input;
        private float targetVelocityX;
        private float velocityXSmoothing;

        private Animator anim;


        private void Start()
        {
            anim = GetComponent<Animator>();
            controller = GetComponent<Advanced2DController>();
            #region Math For Gravity And Jumping Velocity
            /*
             * See The Kinematic Equations
             * Known: Jump Height, Time to jump Apex
             * Solve for: Gravity, Jump Velocity
             * =====================================
             * 
             * GRAVITY CALCULATION
             *  
             * DeltaMovement =  VelocityInitial * Time + ( (Acceleration * (Time ²))/2)
             * 
             * JumpHeight = (Gravity * (TimeToJumpApex ²))/2
             * 
             * 2 * JumpHeight = Gravity * (TimeToJumpApex ²)
             * 
             * (2 * JumpHeight) / Gravity = TimeToJumpApex ²
             *  
             * Gravity / (2 * JumpHeight)  = 1 / (TimeToJumpApex ²)
             *  
             * Gravity = (2 * JumpHeight) / (TimeToJumpApex ²)
             * 
             * 
             * =====================================
             * 
             * JUMP VELOCITY CALCULATION
             * 
             * VelocityFinal = VelocityInitial + Acceleration * Time
             * 
             * JumpVelocity = Gravity * TimeToJumpApex
             * 
            */
            #endregion
            gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
            maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
            minJumpVelocity = Mathf.Sqrt(2* Mathf.Abs(gravity) * minJumpHeight);

        }

        private void Update()
        {

            input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            bool wallSliding = false;
            int wallDirX = (controller.Collisions.left) ? -1 : 1;


            targetVelocityX = input.x * moveSpeed;
            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.Collisions.below) ? accelerationTimeGrounded : accelerationTimeAirBorne);

            if ((controller.Collisions.left || controller.Collisions.right) && !controller.Collisions.below )
            {
                wallSliding = true;

                if (controller.LookRight)
                    anim.Play("RightWall");
                else
                    anim.Play("LeftWall");
                if (velocity.y < -wallSlideSpeedMax)
                {
                    velocity.y = -wallSlideSpeedMax;
                }

                if(timeToWallUnstick > 0)
                {
                    velocityXSmoothing = 0;
                    velocity.x = 0;
                    if (input.x != wallDirX && input.x != 0)
                        timeToWallUnstick -= Time.deltaTime;
                    else
                        timeToWallUnstick = wallStickTime;
                }
                else
                {
                    timeToWallUnstick = wallStickTime;
                }
            }


            if (Input.GetButtonDown("Jump") )
            {
                if (wallSliding)
                {
                    anim.Play("Jump");
                    if (wallDirX == input.x)
                    {
                        velocity.x = -wallDirX * wallJumpClimb.x;
                        velocity.y = wallJumpClimb.y;
                    }
                    else if (input.x == 0)
                    {
                        velocity.x = -wallDirX * wallJumpOff.x;
                        velocity.y = wallJumpOff.y;
                    }
                    else
                    {
                        velocity.x = -wallDirX * wallLeap.x;
                        velocity.y = wallLeap.y;
                    }
                }
                else if (controller.Collisions.below)
                {
                    velocity.y = maxJumpVelocity;
                    anim.Play("Jump");
                }
            }
            if (Input.GetButtonUp("Jump"))
            {
                if(velocity.y > minJumpVelocity)
                    velocity.y = minJumpVelocity;
            }

            //velocity.x = input.x * moveSpeed;

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime,input,anim);

            if (controller.Collisions.above || controller.Collisions.below)
            {
                //Do not accumulate gravity when on ground.
                velocity.y = 0;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if(collision.gameObject.tag == "Paint")
            {
                
                collision.gameObject.GetComponent<SpriteRenderer>().color = playerColor.color;
            }
        }
    }
}
