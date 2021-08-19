using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class AnimationAndMovementController : MonoBehaviour
{

  // Declare reference variables
  PlayerInput playerInput;
  CharacterController characterController;
  Animator animator;

  // Variables to store optimized setter/getter parameter IDs
  int isWalkingHash;
  int isRunningHash;


  // Variables to store player input values
  Vector3 currentMovementInput;
  Vector3 currentMovement;
  Vector3 currentRunMovement;
  bool isMovementPressed;
  bool isRunPressed;

  // Constants
  float rotationFactorPerFrame = 15.0f;
  float walkMultiplier = 3.0f;
  float runMultiplier = 7.0f;
  int zero = 0;
  float gravity = -9.8f;
  float groundedGravity = -.05f;

  // Jumping variables
  bool isJumpPressed = false;
  float initialJumpVelocity;
  float maxJumpHeight = 6.0f;
  float maxJumpTime = 0.75f * 1.2f;
  bool isJumping = false;
  int isJumpingHash;
  bool isJumpAnimating = false;

  // Awake is called earlier than start
  void Awake()
  {
    // Set initial reference variables
    playerInput = new PlayerInput();
    characterController = GetComponent<CharacterController>();
    animator = GetComponent<Animator>();

    // Set parameter hash references
    isWalkingHash = Animator.StringToHash("isWalking");
    isRunningHash = Animator.StringToHash("isRunning");
    isJumpingHash = Animator.StringToHash("isJumping");

    // Set player input callbacks
    playerInput.CharacterControls.Move.started += onMovementInput;
    playerInput.CharacterControls.Move.canceled += onMovementInput;
    playerInput.CharacterControls.Move.performed += onMovementInput;
    playerInput.CharacterControls.Run.started += onRun;
    playerInput.CharacterControls.Run.canceled += onRun;
    playerInput.CharacterControls.Jump.started += onJump;
    playerInput.CharacterControls.Jump.canceled += onJump;

    setupJumpVariables();
  }

  void setupJumpVariables()
  {
    float timeToApex = maxJumpTime / 2;
    gravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
    initialJumpVelocity = (2 * maxJumpHeight) / timeToApex;
  }

  void onJump(InputAction.CallbackContext context)
  {
    isJumpPressed = context.ReadValueAsButton();
    Debug.Log(isJumpPressed);
  }

  void onRun(InputAction.CallbackContext context)
  {
    isRunPressed = context.ReadValueAsButton();
  }

  void onMovementInput(InputAction.CallbackContext context)
  {
    currentMovementInput = context.ReadValue<Vector2>();
    currentMovement.x = currentMovementInput.x * walkMultiplier;
    currentMovement.z = currentMovementInput.y * walkMultiplier;
    currentRunMovement.x = currentMovementInput.x * runMultiplier;
    currentRunMovement.z = currentMovementInput.y * runMultiplier;
    isMovementPressed = currentMovementInput.x != zero || currentMovementInput.y != zero;
    Debug.Log(currentMovementInput);
  }

  void handleAnimation()
  {
    bool isWalking = animator.GetBool(isWalkingHash);
    bool isRunning = animator.GetBool(isRunningHash);

    if (isMovementPressed && !isWalking)
    {
      animator.SetBool(isWalkingHash, true);
    }
    else if (isMovementPressed && !isRunning)
    {
      animator.SetBool(isWalkingHash, false);
    }

    if ((isMovementPressed && isRunPressed) && !isRunning)
    {
      animator.SetBool(isRunningHash, true);
    }
    else if ((!isMovementPressed || !isRunPressed) && isRunning)
    {
      animator.SetBool(isRunningHash, false);
    }
  }

  void handleRotation()
  {
    Vector3 positionToLookAt;
    // The change in position the character should point to
    positionToLookAt.x = currentMovement.x;
    positionToLookAt.y = zero;
    positionToLookAt.z = currentMovement.z;

    // Current rotation of character
    Quaternion currentRotation = transform.rotation;

    if (isMovementPressed)
    {
      // Creates new rotation based on where the player is currently pressing
      Quaternion targetRotation = Quaternion.LookRotation(positionToLookAt);
      transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame * Time.deltaTime);
    }
  }

  // Apply proper gravity
  void handleGravity()
  {
    if (characterController.isGrounded)
    {
      if (isJumpAnimating)
      {
        animator.SetBool(isJumpingHash, false);
        isJumpAnimating = false;
      }

      currentMovement.y = groundedGravity;
      currentRunMovement.y = groundedGravity;
    }
    else
    {
      float previousYVelocity = currentMovement.y;
      float newYVelocity = currentMovement.y + (gravity * Time.deltaTime);
      float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
      currentMovement.y = nextYVelocity;
      currentRunMovement.y = nextYVelocity;
    }
  }

  void handleJump()
  {
    if (!isJumping && characterController.isGrounded && isJumpPressed)
    {
      animator.SetBool(isJumpingHash, true);
      isJumpAnimating = true;
      isJumping = true;
      currentMovement.y = initialJumpVelocity * 0.5f;
      currentRunMovement.y = initialJumpVelocity * 0.5f;
    }
    else if (!isJumpPressed && isJumping && characterController.isGrounded)
    {
      isJumping = false;
    }
  }

  // Update is called once per frame
  void Update()
  {
    handleAnimation();
    handleRotation();

    if (isRunPressed)
    {
      characterController.Move(currentRunMovement * Time.deltaTime);
    }
    else
    {
      characterController.Move(currentMovement * Time.deltaTime);
    }

    handleGravity();
    handleJump();
  }



  void OnEnable()
  {
    playerInput.CharacterControls.Enable();
  }

  void OnDisable()
  {
    playerInput.CharacterControls.Disable();
  }
}
