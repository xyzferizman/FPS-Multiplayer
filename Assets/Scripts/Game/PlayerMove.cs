using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController controller;
    private Vector3 velocityY;
    private Vector3 velocityXZ;
    private readonly float playerHeight = 2f;
    private float playerHalfHeight;
    private float mouseXInput, mouseZInput;
    private float checkSphereRadius = 0.15f;
    
    public LayerMask groundMask;    
    public Transform groundCheck;
    public float gravity = -15f;
    public float movementSpeed = 10f;
    public float jumpInitialSpeed = 10f;
    public bool canGoThroughWalls = false;

    public readonly float baseMovementSpeed = 10f;
    
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

    public void AllowMovingThroughWalls()
    {
        Physics.IgnoreLayerCollision(6, 7, true);
    }

    public void DisableGoingThroughWalls()
    {
        // eventualni kod za pozicioniranje lika izvan objekta prije nego se kolizije opet aktiviraju

        #region Solution #1 - instant snap

        #region Solution 1.1 - instant snap u tocku odmah izvan collidera, na pravcu od Player Objecta do najblize tocke collidera

        #region determiniranje jeli Player Object u collideru

        bool dodirujeCollider = false;
        bool isUnutarObstacleObjekta = false;
        if ( dodirujeCollider)
        {

        }
        else if ( isUnutarObstacleObjekta )
        {

        }



        #endregion

        Rigidbody rb;
        CharacterController charContr = GetComponent<CharacterController>();
           // treba ovo nekako odrediti

        if ( isUnutarObstacleObjekta )
        {
            //c.ClosestPoint
        }
        #endregion

        #endregion

        #region Solution #2 - lerping



        #endregion

        Physics.IgnoreLayerCollision(6, 7, false);
    }
}
