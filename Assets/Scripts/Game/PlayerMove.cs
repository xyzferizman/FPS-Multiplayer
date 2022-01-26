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
    private float checkSphereRadius = 0.15f;
    private float speedBoostFactor = 1.5f;
    private float speedBoostDuration = 3f;
    private float wallsBoostDuration = 4f;
    private bool canGoThroughWalls = false;

    private float lastUpdateY;
    
    public LayerMask groundMask;    
    public Transform groundCheck;
    public float gravity = -15f;
    public float movementSpeed = 10f;
    public float jumpInitialSpeed = 10f;
    
    private void Start()
    {
        playerHalfHeight = playerHeight / 2f;
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
            //Debug.Log("ABOVE GROUND");
            ApplyGravityFall();
        }
        else
        {
            //Debug.Log("MOVE NORMALLY");
            mouseXInput = Input.GetAxis("Horizontal");
            mouseZInput = Input.GetAxis("Vertical");

            velocityXZ = transform.right * mouseXInput + transform.forward * mouseZInput;
            if (mouseXInput != 0f && mouseZInput != 0f)
                velocityXZ /= 1.41421f; // if both inputs active, then divide by sqrt(2) to normalize (so it doesn't speed up when moving diagonally)
            
            if (velocityY.y < -1.5f ) 
                velocityY.y = -1.5f;
            
            if (Input.GetKeyDown(KeyCode.Space))
                velocityY.y += jumpInitialSpeed;
        }

        controller.Move( ((velocityXZ * movementSpeed) + velocityY) * Time.deltaTime);

        // WORK IN PROGRESS - da se napravi da nebude Y koordinata 1.08 nego 1.00

        //if ( !AboveGround() && transform.position.y == lastUpdateY)
        //    controller.Move( new Vector3(0f, -(transform.position.y - playerHalfHeight), 0f));
    }

    private void ApplyGravityFall()
    {
        velocityY += Vector3.up * gravity * Time.deltaTime;
    }

    private bool AboveGround()
    {
        // ako ( malo iznad tla ) && ( nema niceg ispod ) ... onda je "u zraku" i treba applyat gravitaciju
        bool ret = ((transform.position.y - playerHalfHeight) >= 0.03f) && !Physics.CheckSphere(groundCheck.position, checkSphereRadius, groundMask);
        //Debug.Log(ret);
        return ret;
    }

    float startTime_speed;
    float startTime_walls;

    internal void GetSpeedBoost()
    {
        movementSpeed *= speedBoostFactor;
        Invoke("RemoveSpeedBoost", speedBoostDuration);
        Debug.Log("Speed boost added, speed is now " + movementSpeed);
        startTime_speed = Time.time;        
    }
    
    internal void GetWallBoost()
    {
        canGoThroughWalls = true;
        Invoke("RemoveWallsBoost", wallsBoostDuration);
        Debug.Log("Walls boost added");
        startTime_walls = Time.time;
    }

    void RemoveSpeedBoost()
    {
        movementSpeed /= speedBoostFactor;
        Debug.Log("Speed boost removed, speed is now " + movementSpeed + " time elapsed = " + (Time.time - startTime_speed));
    }

    void RemoveWallsBoost()
    {
        canGoThroughWalls = false;
        Debug.Log("Walls boost removed, time elapsed = " + (Time.time - startTime_walls));
    }

    
}
