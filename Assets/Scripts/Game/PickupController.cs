using UnityEngine;

public class PickupController : MonoBehaviour
{
    float height = 1.2f;
    float speedFactor = 1.5f;
    Vector3 speedFactorVec;
    bool goingUp = true;
    float initialPositionY;
    GameObject collidedPlayerObj;

    [SerializeField]
    PickupTypes pickupType;

    private void Start()
    {
        SetAnimateParameters();
    }

    void Update()
    {
        AnimatePickup();
    }

    private void OnTriggerEnter(Collider other)
    {
        if ( other.tag == "Player" )
        {
            // play pickup sound
            other.GetComponentInChildren<Camera>().GetComponent<AudioSource>().Play();

            // apply boost
            collidedPlayerObj = other.gameObject;
            BoostController bc = collidedPlayerObj.GetComponent<BoostController>();
            //

            if ( pickupType == PickupTypes.Speed )
                bc.GetBoost("speed");   
            
            else if (pickupType == PickupTypes.Damage)
                bc.GetBoost("damage");
            
            else if (pickupType == PickupTypes.Walls)
                bc.GetBoost("walls");

            // destroy pickup object
            Destroy(gameObject);
        }
    }

    void SetAnimateParameters()
    {
        speedFactorVec = new Vector3(0f, speedFactor, 0f);
        initialPositionY = transform.position.y;
    }

    void AnimatePickup()
    {
        transform.position += speedFactorVec * Time.deltaTime;

        if ((transform.position.y > (initialPositionY + height)) && goingUp)
        {
            speedFactorVec *= -1f;
            goingUp = false;
        }
        else if (transform.position.y < initialPositionY && !goingUp)
        {
            speedFactorVec *= -1f;
            goingUp = true;
        }
    }
}
