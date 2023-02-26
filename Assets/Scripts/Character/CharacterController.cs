using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterController : MonoBehaviour
{
    private CharacterInputs input; // Input class reference

    private Rigidbody2D rb; // Player's rigidbody refrence

    [Header("Ground Check Values")]
    [SerializeField] private CapsuleCollider2D coll; // Player's collison refrence
    [SerializeField] private LayerMask groundLayers; // Ground layers refrence
    [SerializeField] private bool groundCheckGizmo; // Check if we want to see gizmo for ground checks
    private Color groundGizmo = Color.red; //Colour value of the ground check gizmo

    [Header("Movement values")]
    [SerializeField, Range(0.0f, 20.0f)] float runAccel; // Acceleration rate of the player
    [SerializeField, Range(0.0f, 20.0f)] private float maxRunSpeed; // desire speed for the player to move at
    private float runAccelAmount; // Represent the current acceleration amount
    [SerializeField, Range(0.0f, 20.0f)] private float runDeccel; // Decceleration rate of the player
    private float runDeccelAmount; // Represents the current decceleration amount

    private float direction; // The directoin the player is moving in

    [Header("Jump values")]
    [SerializeField, Range(0.0f, 20.0f)] private float jumpHight; //The hight of the player's jump
    [SerializeField, Range(0.0f, 20.0f)] private float jumpTimeToApex; //Time between applying jump and reaching the apex, also used it gravityStrength, jumpForce
    private float jumpForce; // The force applied to the player (upwards) when they jump
    private float gravityStrength; // The downward fource (gravity) use in jumpForce, gravityScale
    private float gravityScale; // represents the gravity scale this is to be set to
    private bool jumping = false;// check if the player is jumping <----

    private void Awake()
    {
        input = new CharacterInputs();
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<CapsuleCollider2D>();
    }

    private void Start()
    {
        gravityStrength = -(2 * jumpHight) / (jumpTimeToApex * jumpTimeToApex);
        gravityScale = gravityStrength / Physics2D.gravity.y;

        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;

        runAccelAmount = (50 * runAccel) / maxRunSpeed;
        runDeccelAmount = (50* runDeccel) / maxRunSpeed;


        SetGravityScale(gravityScale);
    }

    private void SetGravityScale (float scale)
    {
        rb.gravityScale = scale;
    }

    private void OnValidate()
    {
        gravityStrength = -(2 * jumpHight) / (jumpTimeToApex * jumpTimeToApex);



        jumpForce = Mathf.Abs(gravityStrength) * jumpTimeToApex;
    }

    private void OnEnable()
    {
        input.Enable();

        //Performes when move key is down, and stops when move key is let go
        input.Player.Move.performed += OnMovement;
        input.Player.Move.canceled += OnMovementCanceled;
    }

    private void OnDisable()
    {
        input.Enable();
        input.Player.Move.performed -= OnMovement;
        input.Player.Move.canceled -= OnMovementCanceled;
    }

    private void FixedUpdate()
    {
        Movement();

        if (jumping && IsGrounded())
        {
            Jump();
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if(context.performed) 
        {
            jumping = true;

            Debug.Log(jumping);
        }
        if(context.canceled) 
        {
            jumping= false;

            Debug.Log(jumping);
        }
    }

    private void OnMovement(InputAction.CallbackContext value)
    {
        direction = value.ReadValue<float>();
    }

    private void OnMovementCanceled(InputAction.CallbackContext value)
    {
        direction = 0;
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

    private void Jump()
    {
        float force = jumpForce;
        if(rb.velocity.y < 0)
            force -= rb.velocity.y;

        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
    }

    private void OnDrawGizmos()
    {
        if(groundCheckGizmo)
        {

        }
    }

    //Checks if the player is grounded
    private bool IsGrounded()
    {
        bool result = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size , 0, Vector2.down, .01f, groundLayers);
        if (result)
            groundGizmo = Color.green;
        else 
            groundGizmo = Color.red;

        return result;
    }

    

}
