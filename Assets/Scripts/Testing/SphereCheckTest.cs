using UnityEngine;

public class SphereCheckTest : MonoBehaviour
{
    bool boolVar;
    
    public LayerMask groundMask;
    public Transform groundCheck;
    public float groundCheckRadius = 0.4f;

    private void Start()
    {

    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            boolVar = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);

            if (boolVar)
                Debug.Log("CheckSphere -- TRUE");
            else
                Debug.Log("CheckSphere -- FALSE");
        }
            
    }
}
