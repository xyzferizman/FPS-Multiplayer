using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    public float mouseSensitivity = 125f;

    public Transform playerBody;
    public Transform gun;

    float xRotation = 0f;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {      
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        if ( gun != null )
            gun.transform.localRotation = Quaternion.Euler(0f, -90f, -90f - xRotation);

        transform.localRotation = Quaternion.Euler(xRotation, 0f ,0f);
        if ( playerBody != null )
            playerBody.Rotate(Vector3.up * mouseX);
    }
}
