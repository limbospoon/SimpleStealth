using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public enum MovementState
{
    WALKING,
    CROUCHING,
    LEANING
}

public enum CrouchState
{
    CROUCHED,
    STANDING,
    TRANSITIONING
}

public enum LeanState 
{
    LEFT,
    RIGHT,
    IDLE
}


[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour, IMovement
{

    //Player States
    public MovementState moveState;
    public CrouchState crouchState;
    public LeanState leanState;

    //MovementSettings
    public float walkSpeed = 10.0f;
    public float gravity = 9.8f;

    //Camera
    public float yawSens;
    public float pitchSens;
    public float minPitch;
    public float maxPitch;

    //Crouch
    public bool bisCrouching = false;
    public bool toggleCrouch = false;
    public float crouchSpeed = 5.0f;
    public float crouchHeight = 0.5f;
    public float crouchTransitionSpeed = 10.0f;
    public float camOffset = -0.5f;
    private float originalHeight;
    private Vector3 camStandPos;
    private Vector3 camCrouchPos;


    private Vector2 moveAxisValue;
    private Vector2 lookAxisValue;
    private float moveSpeed;
    //Components
    private PlayerInput inputComp;
    private Camera cam;
    private CharacterController cc;
    private Animator camAnim;
    private MovementState prevState;

    public void Crouch()
    {
        StopCoroutine(DoUnCrouch());
        StartCoroutine(DoCrouch());
    }

    public void UnCrouch()
    {
        StopCoroutine(DoCrouch());
        StartCoroutine(DoUnCrouch());
    }

    void ToggleCrouch()
    {
        if(crouchState == CrouchState.CROUCHED)
            StartCoroutine(DoUnCrouch());
        else if(crouchState == CrouchState.STANDING)
            StartCoroutine(DoCrouch());
    }

    IEnumerator DoCrouch()
    {
        crouchState = CrouchState.TRANSITIONING;
        do
        {
            cam.transform.Translate(Vector3.down * crouchTransitionSpeed * Time.deltaTime);
            yield return 0;

        }while(cam.transform.localPosition.y >= camCrouchPos.y);

        if(moveState != MovementState.LEANING)
        {
            moveState = MovementState.CROUCHING;
        }
        crouchState = CrouchState.CROUCHED;
        cam.transform.localPosition = camCrouchPos;
        cc.height = crouchHeight;
        yield return 0;
    }

    IEnumerator DoUnCrouch()
    {
        crouchState = CrouchState.TRANSITIONING;
        do
        {
            cam.transform.Translate(Vector3.up * crouchTransitionSpeed * Time.deltaTime);
            yield return 0;

        }while(cam.transform.localPosition.y <= camStandPos.y);

        if(moveState != MovementState.LEANING)
        {
            moveState = MovementState.WALKING;
        }

        crouchState = CrouchState.STANDING;
        cam.transform.localPosition = camStandPos;
        cc.height = originalHeight;
        yield return 0;
    }


    public void Lean(float direction)
    {
        if(direction > 0)
        {
            camAnim.ResetTrigger("Idle");
            camAnim.ResetTrigger("LeanLeft");
            camAnim.SetTrigger("LeanRight");
            leanState = LeanState.RIGHT;
        }
        else if(direction < 0)
        {
            camAnim.ResetTrigger("Idle");
            camAnim.ResetTrigger("LeanRight");
            camAnim.SetTrigger("LeanLeft");
            leanState = LeanState.LEFT;
        }
        moveState = MovementState.LEANING;
    }

    public void StopLean()
    {
        camAnim.ResetTrigger("LeanRight");
        camAnim.ResetTrigger("LeanLeft");
        camAnim.SetTrigger("Idle");
        leanState = LeanState.IDLE;
        if(crouchState == CrouchState.CROUCHED)
            moveState = MovementState.CROUCHING;
        else
            moveState = MovementState.WALKING;
    }

    void Leaning()
    {
        RaycastHit hit;
        Vector3 dir = Vector3.zero;
        if(leanState == LeanState.LEFT)
            dir = -transform.right;
        else if(leanState == LeanState.RIGHT)
            dir = transform.right;

        if(Physics.Raycast(transform.position, dir, out hit, 1.0f))
        {
            StopLean();
        }
    }

    public void Look()
    {
        float pitch = lookAxisValue.y * pitchSens * Time.deltaTime;
        float yaw = lookAxisValue.x * yawSens * Time.deltaTime;

        Vector3 eulerAngle = transform.localEulerAngles;
        Vector3 camEulerAngle = cam.transform.localEulerAngles;
        
        if(eulerAngle.x > 180.0f)
            eulerAngle.x -= 360.0f;

        if(camEulerAngle.x > 180.0f)
            camEulerAngle.x -= 360.0f;

        eulerAngle.x = 0.0f;
        eulerAngle.y = Mathf.Clamp(eulerAngle.y + yaw, -360.0f, 360.0f);
        camEulerAngle.x = Mathf.Clamp(camEulerAngle.x - pitch, minPitch, maxPitch);

        transform.localEulerAngles = eulerAngle;
        cam.transform.localEulerAngles = camEulerAngle;
    }

    public void Move()
    {
        moveSpeed = (moveState != MovementState.WALKING) ? crouchSpeed : walkSpeed;

        float g = (cc.isGrounded) ? 0.0f : gravity;

        Vector3 inputDir = new Vector3(moveAxisValue.x, 0.0f, moveAxisValue.y);

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        forward *= inputDir.z;
        right *= inputDir.x;
        
        Vector3 moveDir = (forward + right);
        moveDir.Normalize();

        moveDir.y -= g;

        cc.Move(moveDir * moveSpeed * Time.deltaTime);
    }

    public void OnLook(InputAction.CallbackContext ctx)
    {
        lookAxisValue = ctx.ReadValue<Vector2>();
    }

    public void OnMove(InputAction.CallbackContext ctx)
    {
        moveAxisValue = ctx.ReadValue<Vector2>();
    }

    public void OnCrouch(InputAction.CallbackContext ctx)
    {
        if(ctx.performed && !toggleCrouch)
            Crouch();
        else if(ctx.performed && toggleCrouch)
            ToggleCrouch();
        
        if(ctx.canceled && !toggleCrouch)
            UnCrouch();
    }

    public void OnLean(InputAction.CallbackContext ctx)
    {
        float f = ctx.ReadValue<float>();
        if(ctx.performed)
            Lean(f);
        
        if(ctx.canceled)
            StopLean();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputComp = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;

        cam = transform.GetComponentInChildren<Camera>();
        cc = GetComponent<CharacterController>();

        originalHeight = cc.height;
        camStandPos = cam.transform.localPosition;
        camCrouchPos = camStandPos + new Vector3(0, camOffset, 0);

        camAnim = transform.GetComponentInChildren<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Look();
    }

    void FixedUpdate()
    {
        if(leanState != LeanState.IDLE)
            Leaning();
    }
}
