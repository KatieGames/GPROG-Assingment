using UnityEngine;
using System;
using System.Linq;


[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    // Movement Options
    [Header("Movement Options")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float originalHeight = 2f;

    // cooldown between footstep sounds
    [SerializeField] private float footstepCooldown = 1.3f;


    // Ground Options
    [Header("Ground Options")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private Transform groundCheck;

    // Mouse Look Options
    [Header("Mouse Look Options")]
    [SerializeField] private float lookSpeedX = 2f;
    [SerializeField] private float lookSpeedY = 2f;
    [SerializeField] private float upperLookLimit = 80f;
    [SerializeField] private float lowerLookLimit = 80f;

    // Misc vars
    private Rigidbody rb;
    private Player player;
    private Vector3 moveInput;
    private bool isGrounded;
    private float rotationX = 0f;
    private float lastFootstepTime = 0f;
    private float defaultStepVolume;
    private bool sprinting = false;

    [HideInInspector] public event Action<Vector3> OnSoundMade;


    private void Awake()
    {
        // get needed components and do initial setup
        rb = GetComponent<Rigidbody>();
        player = GetComponentInParent<Player>();
        rb.freezeRotation = true;

        // get default sound volume from the audio manager
        defaultStepVolume = AudioManager.instance.oneShotSounds.FirstOrDefault(sound => sound.name == "footstep").source.volume;
    }

    private void Update()
    {
        HandleInput();
        HandleMouseLook();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleGroundCheck();
    }

    private void HandleInput()
    {
        // get input axis
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        moveInput = new Vector3(moveX, 0, moveZ);

        // check for movement modifiers. Crouch, jump, sprint
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !player.isCrouched)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded)
        {
            player.isCrouched = true;
            transform.localScale = new Vector3(1f, crouchHeight / originalHeight, 1f);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            player.isCrouched = false;
            transform.localScale = Vector3.one;
        }
    }

    private void HandleMovement()
    {
        // calculate movement direction
        Vector3 moveDirection = moveInput;
        moveDirection = transform.TransformDirection(moveDirection);

        // apply sprint multiplier if shift key is held down
        float currentSpeed = moveSpeed;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed += sprintMultiplier;
            sprinting = true;
        }
        else{sprinting = false;}

        // apply movement
        Vector3 targetVelocity = moveDirection * currentSpeed;
        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0;
        rb.AddForce(velocityChange, ForceMode.VelocityChange);

        if(moveDirection != Vector3.zero)
        {
            Footstep();
        }
    }

    private void Footstep()
    {
        OnSoundMade?.Invoke(transform.position);
        // play sound with randomised pitch for variance

        if (Time.time - lastFootstepTime >= footstepCooldown)
        {
            // get the sound
            Sound footstepSound = AudioManager.instance.oneShotSounds.FirstOrDefault(sound => sound.name == "footstep");

            if(sprinting)
            {
                footstepSound.source.volume = defaultStepVolume + 0.3f;
            }
            else if(player.isCrouched)
            {
                footstepSound.source.volume = defaultStepVolume - 0.2f;
            }
            else
            {
                footstepSound.source.volume = defaultStepVolume;
            }

            if (footstepSound != null)
            {
                // modify pitch randomly
                footstepSound.source.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            }

            // play the sound
            AudioManager.instance.PlayOneShotSound("footstep");

            // update the last footstep time to the current time
            lastFootstepTime = Time.time;
        }
    }

    private void HandleGroundCheck()
    {
        // do a sphere check to confirm player is on the ground
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    private void HandleMouseLook()
    {
        // get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * lookSpeedX;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);

        // rotate the player body in y axis
        transform.Rotate(Vector3.up * mouseX);
    }

    // draws a sphere in editor to show where the ground check is
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
