using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    //Assingables
    [SerializeField]
    private Transform playerCam;
    [SerializeField]
    private Transform orientation;

    //Other
    private Rigidbody rb;

    //Rotation and look
    private float xRotation;
    private float sensitivity = 50f;
    private float sensMultiplier = 2f;

    //Movement
    private float moveSpeed = 4500;
    private float maxSpeed = 25;
    private bool grounded;
    public LayerMask ground;

    public float counterMovement = 0.175f;
    private float threshold = 0.01f;
    private float maxSlopeAngle = 35f;

    //Crouch & Slide
    private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    private float slideForce = 500f;
    private float slideCounterMovement = 0.1f;
    private float slopeSlideForce = 3000f;

    //Jumping
    [SerializeField]
    private int maxJumps = 2, jumpsLeft = 2;
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f, doubleJumpDamping = 0.65f;
    private float jumpForce = 550f;

    //Wallrunning
    public LayerMask whatIsWall;
    public float wallrunForce;
    public float maxWallrunTime;
    public float maxWallrunSpeed;
    bool isWallRight;
    bool isWallLeft;
    bool isWallRunning;
    public float maxWallRunCameraTilt;
    public float wallRunCameraTilt;

    //Input
    float x, y;
    bool jumping, jumpBuffered, sprinting, crouching;

    //Sliding
    private Vector3 normalVector = Vector3.up;
    private Vector3 wallNormalVector;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerScale = transform.localScale;
    }

    private void FixedUpdate()
    {
        Walking();
    }

    private void Update()
    {
        //Input
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumping = Input.GetButton("Jump");
        if (Input.GetButtonDown("Jump") && jumpsLeft > 0)
        {
            jumpBuffered = true;
        }

        //Crouching
        crouching = Input.GetKey(KeyCode.LeftControl);
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartCrouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopCrouch();
        }

        //Cursor Lock (PRESS C TO LOCK CURSOR)
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
            } else
            {
                Cursor.lockState = CursorLockMode.None;
            }
            Cursor.visible = !Cursor.visible;
        }

        Look();
        CheckForWall();
        WallRunInput();
    }

    private void Walking()
    {
        //Extra gravity
        rb.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(x, y, mag);

        //If ready to jump and jump buffered and you have jumps
        if (readyToJump && jumpBuffered && jumpsLeft > 0)
        {
            Jump();
        }

        //Set max speed
        float maxSpeed = this.maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (crouching && grounded && readyToJump)
        {
            rb.AddForce(Vector3.down * Time.deltaTime * slopeSlideForce);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (x > 0 && xMag > maxSpeed)
        {
            x = 0;
        }
        if (x < 0 && xMag < -maxSpeed) {
            x = 0;
        }
        if (y > 0 && yMag > maxSpeed)
        {
            y = 0;
        }
        if (y < 0 && yMag < -maxSpeed)
        {
            y = 0;
        }

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (grounded && crouching) multiplierV = 0f;

        //Apply forces to move player
        rb.AddForce(orientation.transform.forward * y * moveSpeed * Time.deltaTime * multiplier * multiplierV);
        rb.AddForce(orientation.transform.right * x * moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump()
    {
        if (readyToJump)
        {
            jumpBuffered = false;
            jumpsLeft--;
            readyToJump = false;

            //Add jump forces
            rb.AddForce(Vector2.up * jumpForce * 1.5f);
            rb.AddForce(normalVector * jumpForce * 0.5f);

            //Redirect momentum on double jump
            if (!grounded)
            {
                rb.velocity = (orientation.transform.forward * y  + orientation.transform.right * x).normalized * rb.velocity.magnitude * doubleJumpDamping;
            }

            //If jumping while falling, reset y velocity.
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
            {
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            }
            else if (rb.velocity.y > 0)
            {
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            }

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        //Walljump
        if (isWallRunning)
        {
            readyToJump = false;

            //normal jump
            if (isWallLeft && !Input.GetKey(KeyCode.D))
            {
                rb.AddForce(Vector2.up * jumpForce * 0.5f);
                rb.AddForce(normalVector * jumpForce * 0.5f);
                rb.AddForce(orientation.right * jumpForce * 1f);
            }

            if (isWallRight && !Input.GetKey(KeyCode.A))
            {
                rb.AddForce(Vector2.up * jumpForce * 0.5f);
                rb.AddForce(normalVector * jumpForce * 0.5f);
                rb.AddForce(-orientation.right * jumpForce * 1f);
            }

            //sideways wallhop
            if (isWallRight || isWallLeft && Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) rb.AddForce(-orientation.up * jumpForce * 1f);
            if (isWallRight && Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * jumpForce * 3.2f);
            if (isWallLeft && Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * jumpForce * 3.2f);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void StartCrouch()
    {
        transform.localScale = crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
        if (rb.velocity.magnitude > 0.5f)
        {
            if (grounded)
            {
                rb.AddForce(orientation.transform.forward * slideForce);
            }
        }
    }

    private void StopCrouch()
    {
        transform.localScale = playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private float desiredX;
    private void Look()
    {
        //Camera rotation
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime * sensMultiplier;
        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, wallRunCameraTilt);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);

        //While wallrunning
        if (System.Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallRight)
        {
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        }
        if (System.Math.Abs(wallRunCameraTilt) < maxWallRunCameraTilt && isWallRunning && isWallLeft)
        {
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        }

        //Tilts camera back after done wallrunning
        if (wallRunCameraTilt > 0 && !isWallRunning && !isWallRight)
        {
            wallRunCameraTilt -= Time.deltaTime * maxWallRunCameraTilt * 2;
        }
        if (wallRunCameraTilt < 0 && !isWallRunning && !isWallLeft)
        {
            wallRunCameraTilt += Time.deltaTime * maxWallRunCameraTilt * 2;
        }
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!grounded || jumping) {
            return;
        }

        //Slow down sliding
        if (crouching)
        {
            rb.AddForce(moveSpeed * Time.deltaTime * -rb.velocity.normalized * slideCounterMovement);
            return;
        }

        //Counter movement
        if (Mathf.Abs(mag.x) > threshold && Mathf.Abs(x) < 0.05f || (mag.x < -threshold && x > 0) || (mag.x > threshold && x < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.right * Time.deltaTime * -mag.x * counterMovement);
        }
        if (Mathf.Abs(mag.y) > threshold && Mathf.Abs(y) < 0.05f || (mag.y < -threshold && y > 0) || (mag.y > threshold && y < 0))
        {
            rb.AddForce(moveSpeed * orientation.transform.forward * Time.deltaTime * -mag.y * counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(rb.velocity.x, 2) + Mathf.Pow(rb.velocity.z, 2))) > maxSpeed)
        {
            float fallspeed = rb.velocity.y;
            Vector3 n = rb.velocity.normalized * maxSpeed;
            rb.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    private void WallRunInput()
    {
        //Start Wallrun
        if (Input.GetKey(KeyCode.D) && isWallRight) StartWallRun();
        if (Input.GetKey(KeyCode.A) && isWallLeft) StartWallRun();
    }

    private void StartWallRun()
    {
        rb.useGravity = false;
        isWallRunning = true;

        if (rb.velocity.magnitude <= maxWallrunSpeed)
        {
            rb.AddForce(orientation.forward * wallrunForce * Time.deltaTime);

            //Make sure char sticks to wall
            if (isWallRight)
            {
                rb.AddForce(orientation.right * wallrunForce / 5 * Time.deltaTime);
            } else
            {
                rb.AddForce(-orientation.right * wallrunForce / 5 * Time.deltaTime);
            }
        }
    }

    private void StopWallRun()
    {
        rb.useGravity = true;
        isWallRunning = false;
    }

    private void CheckForWall()
    {
        isWallRight = Physics.Raycast(transform.position, orientation.right, 1f, whatIsWall);
        isWallLeft = Physics.Raycast(transform.position, -orientation.right, 1f, whatIsWall);

        //leave wall run
        if (!isWallLeft && !isWallRight) StopWallRun();
        //reset double jump
        if (isWallLeft || isWallRight) jumpsLeft = maxJumps;
    }

    public Vector2 FindVelRelativeToLook()
    {
        float lookAngle = orientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = rb.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

    private bool cancellingGrounded;
    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (ground != (ground | (1 << layer))) {
            return; 
        }

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                jumpsLeft = maxJumps;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }

    public void SetJumpsLeft(int jumps){
        jumpsLeft = jumps;
    }
    public int getJumpsLeft(){
        return jumpsLeft;
    }
    public void SetMaxJumps(int jumps){
        maxJumps = jumps;
    }

}
