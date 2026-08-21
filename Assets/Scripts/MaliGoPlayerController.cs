using UnityEngine;
using MaliGo.Characters;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Rigidbody))]
public class MaliGoPlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Sprite")]
    public CharacterSpriteController spriteController;
    
    private CharacterController characterController;
    private Vector3 moveDirection;
    private bool isMoving;

    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        gameObject.tag = "Player";
    }
    
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        if (spriteController == null)
            spriteController = GetComponentInChildren<CharacterSpriteController>();
    }
    
    void Update()
    {
        HandleMovement();
    }
    
    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        moveDirection = new Vector3(horizontal, 0, vertical).normalized;
        
        isMoving = moveDirection.magnitude >= 0.1f;

        if (isMoving)
        {
            characterController.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        if (spriteController != null)
            spriteController.UpdateAnimation(new Vector2(horizontal, vertical), isMoving);
    }
    
    // Method to interact with buildings/objects
    public void InteractWithBuilding(string buildingType)
    {
        Debug.Log($"Interacting with {buildingType}");
        // Add interaction logic here
    }
}