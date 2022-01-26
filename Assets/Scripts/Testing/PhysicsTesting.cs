using UnityEngine;

public class PhysicsTesting : MonoBehaviour
{    
    public Vector3 originPoint;

    Vector3 direction;
    RaycastHit hit;

    //void Start()
    //{
    //    direction = -Vector3.right;

    //    Physics.Raycast(originPoint, direction, out hit);
    //}

    void FixedUpdate()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
            Debug.DrawLine(ray.origin, hit.point);

    }
}
