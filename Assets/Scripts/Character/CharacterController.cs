using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{
    #region variables 
    private CharacterInputs input; // Input class reference

    [SerializeField] private GameObject AttackBox;
    [SerializeField] private float attackCD;
    [SerializeField] private float attackDuration;
    private float attackTimer;
    private float attackCDTimer = 0;
    private bool attack = false;
    private bool attacking = false;


    private Rigidbody2D rb; // Player's rigidbody refrence

    private bool dead = false;

    #region Ground Variables

    [Header("Ground Check Values")]
    [SerializeField] private BoxCollider2D coll; // Player's collison refrence
    [SerializeField] private LayerMask groundLayers; // Ground layers refrence
    [SerializeField] private bool groundCheckGizmo; // Check if we want to see gizmo for ground checks
    private float lastOnGround = 0.0f;
    private Color groundGizmo = Color.red; //Colour value of the ground check gizmo
    #endregion

    #region Movement Variables

    [Header("Movement values")]
    [SerializeField, Range(0.0f, 20.0f)] float runAccel; // Acceleration rate of the player
    [SerializeField, Range(0.0f, 20.0f)] private float maxRunSpeed; // desire speed for the player to move at
    private float runAccelAmount; // Represent the current acceleration amount
    [SerializeField, Range(0.0f, 20.0f)] private float runDeccel; // Decceleration rate of the player
    private float runDeccelAmount; // Represents the current decceleration amount

    private float direction; // The directoin the player is moving in
    #endregion

    #region Jump Variables

    [Header("Jump values")]
    [SerializeField, Range(0.0f, 20.0f)] private float jumpHight; //The hight of the player's jump
    [SerializeField, Range(0.0f, 20.0f)] private float jumpTimeToApex; //Time between applying jump and reaching the apex, also used it gravityStrength, jumpForce
    [SerializeField, Range(0.01f, 0.75f)] private float coyoteTime; // represents how long th eplayer can be off a platform and still jump
    [SerializeField, Range(0.01f, 0.75f)] private float jumpTimeBuffer; // represents a buffer on when the player jumps to allow them extra time to actually be allowed to
    private float lastPressedJump = 0.0f; // represents the last time jump button was pressed
    private float jumpForce; // The force applied to the player (upwards) when they jump
    private float gravityStrength; // The downward fource (gravity) use in jumpForce, gravityScale
    private float gravityScale; // represents the gravity scale this is to be set to
    private bool isJumping = false;// check if the player is jumping
    private bool canJumpCut = false; // check if the player can jump cut
    private bool jumpCut;// check if the player has chosen to jump cut
    #endregion

    #region Dash Variables

    [Header("Dash Values")]
    [SerializeField] private float dashPower;
    [SerializeField] private float dashCDTime;
    [SerializeField, Range(0.1f, 1.0f)] private float dashTimeBuffer;
    [SerializeField] private float dashTime;
    private float lastFacedDirection = 1;
    private float dashTimeCounter = 0.0f;
    private float dashCD = 0.0f;
    private float lastPressedDash = 0.0f;
    private bool isDashing = false;
    private bool dashed = false;
    private bool dashImmunity = false;

    #endregion

    #region Time Warp Variables
    [Header ("Time Warp Values")]
    [SerializeField, Range(0f, 1f)] private float slowedDownTimeScale;

    #endregion

    #endregion
    #region Setups

    //runst first thing on scritp
    private void Awake()
    {
        input = new CharacterInputs();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    // Updates when inspector changes
    private void OnValidate()
    {
        rb = GetComponent<Rigidbody2D>();

        gravityStrength = -(2 * jumpHight) / (jumpTimeToApex * jumpTimeToApex);
        gravityScale = gravityStrength / Physics2D.gravity.y;

        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        runAccelAmount = (50 * runAccel) / maxRunSpeed;
        runDeccelAmount = (50 * runDeccel) / maxRunSpeed;


        SetGravityScale(gravityScale);
    }

    // Sets the gravity scale of rigidbody
    private void SetGravityScale (float scale)
    {
        rb.gravityScale = scale;
    }

    //Updates when object is enabled
    private void OnEnable()
    {
        input.Enable();

        //Performes when move key is down, and stops when move key is let go
        input.Player.Move.performed += OnMovement;
        input.Player.Move.canceled += OnMovementCanceled;
    }

    // Updates when object is disabled
    private void OnDisable()
    {
        input.Disable();
        input.Player.Move.performed -= OnMovement;
        input.Player.Move.canceled -= OnMovementCanceled;
    }

    #endregion

    #region Updates

    // Updates at a fixed rate, use mainly for physics updating
    private void FixedUpdate()
    {
        if (!dead)
        {
            if (!isDashing)
                Movement();
        }
    }

    // Updates every frame, use mainly for frame specfic actions
    private void Update()
    {
        if (!dead)
        {
            #region Timers
            lastOnGround -= Time.deltaTime;
            lastPressedJump -= Time.deltaTime;
            lastPressedDash -= Time.deltaTime;
            dashCD -= Time.deltaTime;
            dashTimeCounter -= Time.deltaTime;
            attackCDTimer -= Time.deltaTime;
            attackTimer -= Time.deltaTime;
            #endregion

            if (lastPressedDash > 0 && CanDash())
            {
                isDashing = true;
                canJumpCut = false;
                jumpCut = false;
                isJumping = false;
                dashImmunity = true;
                attackCDTimer = 0;

                gameObject.layer = LayerMask.NameToLayer("Dash Immune");

                Dash();
                SetGravityScale(0);
            }

            if (!isDashing)
            {
                //Check if player can jump
                if (lastPressedJump > 0 && lastOnGround > 0)
                {
                    isJumping = true;
                    canJumpCut = true;
                    jumpCut = false;
                    isDashing = false;

                    Jump();
                }

                else if (jumpCut)
                {
                    isJumping = false;
                    isDashing = false;

                    SetGravityScale(gravityScale * 2);
                }

                if (IsGrounded())
                {
                    lastOnGround = coyoteTime;
                    isJumping = false;
                    jumpCut = false;
                    isDashing = false;
                    dashed = false;

                    SetGravityScale(gravityScale);
                }

                else if (isJumping && rb.velocity.y <= 0)
                {
                    isJumping = false;
                    canJumpCut = false;
                    isDashing = false;
                }

                if (attack)
                {
                    AttackBox.SetActive(true);

                    attackCDTimer = attackCD;
                    attack = false;
                    attacking = true;
                    attackTimer = attackDuration;
                }
                else if (attacking && attackTimer <= 0)
                {
                    attacking = false;
                    AttackBox.SetActive(false);
                }
            }
            else if (dashTimeCounter <= 0)
            {
                gameObject.layer = LayerMask.NameToLayer("Character");
                dashImmunity= false;
                isDashing = false;
                rb.velocity = Vector2.zero;
                SetGravityScale(gravityScale);
            }
        }
    }

    #endregion

    #region Run

    // Triggers when movement input starts
    private void OnMovement(InputAction.CallbackContext value)
    {
        if (!dead)
        {
            direction = value.ReadValue<float>();
            lastFacedDirection = direction;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);
        }
    }

    // Triggers when movement input stops
    private void OnMovementCanceled(InputAction.CallbackContext value)
    {
        if (!dead)
        {
            direction = 0;
        }
    }

    //Preforms player movement
    private void Movement()
    {
        float targetSpeed = maxRunSpeed * direction;

        // I currently don't think this matters
        targetSpeed = Mathf.Lerp(rb.velocity.x, targetSpeed, 1);

        float accelRate;

        if(IsGrounded()) 
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccelAmount : runDeccelAmount;

        //finds how much fource needs to be applied for acceleartion.
        //first finds differnce between target speed and current speed,
        //next multiplies the diffrence by the accelaraction rate to find how much fource needs to be added.
        float speedDif = targetSpeed - rb.velocity.x;
        float movement = speedDif * runAccelAmount;

        //Applies the movement value as a fource on the x axis
        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    #endregion

    #region Jump

    // Triggers when jump input changes state
    public void OnJump(InputAction.CallbackContext context)
    {
        if (!dead)
        {
            lastPressedJump = 0;
            lastOnGround = 0;

            if (context.performed)
            {
                lastPressedJump = jumpTimeBuffer;

            }
            if (context.canceled)
            {
                if (canJumpCut)
                {
                    jumpCut = true;
                }
            }
        }
    }



    // Applies jump
    private void Jump()
    {
        lastPressedJump = 0;
        lastOnGround = 0;

        float force = jumpForce;
        if(rb.velocity.y < 0)
            force -= rb.velocity.y;

        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    #endregion

    #region Dash
    public void OnDash(InputAction.CallbackContext context)
    {
        if (!dead)
        {
            if (context.performed)
                lastPressedDash = dashTimeBuffer;
        }
    }

    private void Dash()
    {

        lastPressedDash = 0;
        dashCD = dashCDTime;
        dashTimeCounter = dashTime;
        dashed = true;

        rb.velocity = Vector2.zero;

        if (direction != 0)
            rb.AddForce((Vector2.right * direction) * dashPower, ForceMode2D.Impulse);
        else
            rb.AddForce((Vector2.right * lastFacedDirection) * dashPower, ForceMode2D.Impulse);
    }

    private bool CanDash()
    {
        if (lastOnGround >= 0)
        {
            if (dashCD <= 0)
            {
                return true;
            }
        }
        else if (!dashed)
        {
            return true;
        }

        return false;
    }

    #endregion


    public void Attack(InputAction.CallbackContext context)
    {
        if(context.performed && attackCDTimer <= 0)
        attack = true;
    }

    // Updates Gizmos 
    private void OnDrawGizmos()
    {
        if(groundCheckGizmo)
        {
            Debug.DrawRay(coll.bounds.center + new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + 0.01f), groundGizmo); ;
            Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, 0), Vector2.down * (coll.bounds.extents.y + 0.01f), groundGizmo);
            Debug.DrawRay(coll.bounds.center - new Vector3(coll.bounds.extents.x, coll.bounds.extents.y + 0.01f), Vector2.right * (coll.bounds.extents.x * 2), groundGizmo);
        }
    }

    public void OnTimeWarp(InputAction.CallbackContext context)
    {
        if (!dead)
        {
            if (context.performed)
            {
                Time.timeScale = slowedDownTimeScale;
                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }
            if (context.canceled)
            {
                Time.timeScale = 1;
            }
        }
    }
    //Checks if the player is grounded
    private bool IsGrounded()
    {
        bool result = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size , 0, Vector2.down, .1f, groundLayers);
        if (result)
            groundGizmo = Color.green;
        else 
            groundGizmo = Color.red;

        return result;
    }

    public void Hit()
    {
        if (!dashImmunity)
        {
            dead = true;
            Time.timeScale = 0;
        }
    }

    public void ResetLevel(InputAction.CallbackContext context)
    {
        if(context.performed)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
        }
    }
}
