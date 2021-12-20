using UnityEngine;
using MLAPI.NetworkVariable;

public class PlayerMove : MonoBehaviour
{
    CharacterController controller;
    private Vector3 velocityY;
    private Vector3 velocityXZ;
    private readonly float playerHeight = 2f;
    private float playerHalfHeight;
    private float mouseXInput, mouseZInput;    

    public float gravity = -15f;
    public float movementSpeed = 10f;
    public float jumpInitialSpeed = 10f;
    
    private void Start()
    {
        playerHalfHeight = playerHeight / 2;
        velocityY = new Vector3(0f,0f,0f);
        controller = GetComponent<CharacterController>();

        if ( controller == null )
        {
            Debug.LogError("PlayerMove.Start method didnt manage to find CharacterController on gameObject " + gameObject.name);
        }
    }

    // Update is called once per frame
    private void Update()
    {      
        if ( AboveGround() )
        {
            ApplyGravityFall();
        }
        else
        {
            mouseXInput = Input.GetAxis("Horizontal");
            mouseZInput = Input.GetAxis("Vertical");



            velocityXZ = transform.right * mouseXInput + transform.forward * mouseZInput;
            if (mouseXInput != 0f && mouseZInput != 0f)
                velocityXZ /= 1.41421f; // if both inputs active, then divide by sqrt(2) to normalize (so it doesn't speed up when moving diagonally)
            if (velocityY.y != 0f ) velocityY.y = 0f;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                velocityY.y += jumpInitialSpeed;
            }            
        }

        controller.Move( ((velocityXZ * movementSpeed) + velocityY) * Time.deltaTime);
    }

    private void ApplyGravityFall()
    {

        velocityY += Vector3.up * gravity * Time.deltaTime;
    }

    private bool AboveGround()
    {
        float yVar = transform.position.y - playerHalfHeight;

        if (yVar >= 0.15f) return true;
        return false;
    }
}
