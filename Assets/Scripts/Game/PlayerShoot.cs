using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject projectilePrefab;

    private float speed = 12f;
    private Transform cameraTransform;

    private MLAPI_Player myMlapiPlayer;
    private PUN_Player myPunPlayer;
    private Transport_Player myTransportPlayer;

    private float cooldownTime = 0.25f;
    private float lastShootTime = -0.15f;

    private string myNetType;

    private void Start()
    {
        cameraTransform = transform.parent.GetComponentInChildren<Camera>().transform;
        if ( cameraTransform == null )
        {
            Debug.LogError("PlayerShoot, Start: couldn't get camera transform.");
        }
        
        // figuring out the network type
        myNetType = PlayerPrefs.GetString("networkType");

        if ( myNetType == "mlapi")
        {
            myMlapiPlayer = GetComponentInParent<MLAPI_Player>();
            if (myMlapiPlayer == null)
            {
                Debug.LogError("PlayerShoot, Start: couldn't get mlapi player component.");
            }
        }
        else if ( myNetType == "pun_v2" )
        {
            myPunPlayer = GetComponentInParent<PUN_Player>();
        }
        else if ( myNetType == "transport" )
        {
            myTransportPlayer = GetComponentInParent<Transport_Player>();
        }
        else
        {
            Debug.LogError("PROBLEM in PlayerShoot, myNetType not recognized");
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if ((Time.realtimeSinceStartup - lastShootTime) < cooldownTime)
                return;

            lastShootTime = Time.realtimeSinceStartup;

            if ( myNetType == "mlapi")
            {
                myMlapiPlayer.NetworkShoot(speed);
            }
            else if ( myNetType == "pun_v2")
            {
                myPunPlayer.NetworkShoot(speed);
            }
            else if ( myNetType == "transport" )
            {
                myTransportPlayer.NetworkShoot(speed);
            }
        }
    }

    
}
