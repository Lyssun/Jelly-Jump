using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Player2D : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 15f;
    [SerializeField]
    private float jumpForce = 15f;

    [SerializeField]
    private float jumpMaxChargeTime = 2;

    [SerializeField]
    private Transform feetPos;

    [SerializeField]
    private Transform wallPos;

    [SerializeField]
    private float checkRadius;

    [SerializeField]
    private LayerMask ground;

    [SerializeField]
    private bool lookingRight = true;


    private float moveInput;

    private Rigidbody2D rb;

    private bool isOnGround = false;
    private bool isTouchingWall = false;
    private bool isJumping;
    
    private float jumpChargeTime;

    //private SpriteRenderer sp;

    private void Start ()
    {
        rb = GetComponent<Rigidbody2D>();

        //sp = GetComponent<SpriteRenderer>();
	}

    private void FixedUpdate()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    private void Update()
    {
        isOnGround = Physics2D.OverlapCircle(feetPos.position, checkRadius, ground);
        isTouchingWall = Physics2D.OverlapCircle(wallPos.position, checkRadius, ground);
        if (!lookingRight && moveInput > 0)
        {
            transform.eulerAngles = Vector3.zero;
            //sp.flipX = false;
            lookingRight = true;
        }
        else
        {
            if (lookingRight && moveInput < 0)
            {
                transform.eulerAngles = new Vector3(0, 180, 0);
                //sp.flipX = true;
                lookingRight = false;
            }
        }

        if(isOnGround && Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
            jumpChargeTime = jumpMaxChargeTime;
            rb.velocity = Vector2.up * jumpForce;
        }

        if (Input.GetKey(KeyCode.Space) && isJumping)
        {
            if (jumpChargeTime > 0)
            {
                rb.velocity = Vector2.up * jumpForce;
                jumpChargeTime -= Time.deltaTime;
            }
            else
                isJumping = false;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }

        // WallJump
        if( isTouchingWall && Input.GetKeyDown(KeyCode.Space))
        {
            //rb.velocity = new Vector2((lookingRight ? -1 : 1) *moveSpeed, jumpForce);
            rb.AddForce(new Vector2((lookingRight ? -1 : 1) * 500, 0));
            Debug.Log(rb.velocity);
        }


    }
}
