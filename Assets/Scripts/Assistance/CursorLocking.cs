using UnityEngine;
using MLAPI;
using Photon.Pun;

public class CursorLocking : NetworkBehaviour
{
    private string myNetType;

    private void Start()
    {
        // figuring out the network type
        myNetType = PlayerPrefs.GetString("networkType");

        if (myNetType == "mlapi")
        {
            if (!IsLocalPlayer)
            {
                enabled = false;
            }
        }
        else if (myNetType == "pun_v2")
        {
            // if not pun local player, disable script
            if (!GetComponent<PhotonView>().AmOwner)
                enabled = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                GetComponentInChildren<PlayerShoot>().enabled = false;
                GetComponentInChildren<PlayerLook>().enabled = false;
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                GetComponentInChildren<PlayerShoot>().enabled = true;
                GetComponentInChildren<PlayerLook>().enabled = true;
            }
        }
    }

}
