using UnityEngine;
using MLAPI;
using Photon.Pun;

public class CursorLocking : MonoBehaviour
{
    private string myNetType;
    private GameObject localPlayer;

    public void SetLocalPlayer(GameObject _localPlayer)
    {
        localPlayer = _localPlayer;
    }

    #region novo
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) )
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                if (localPlayer != null)
                {
                    localPlayer.GetComponentInChildren<PlayerShoot>().enabled = false;
                    localPlayer.GetComponentInChildren<PlayerLook>().enabled = false;
                }                    
            }
            else if (Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                if (localPlayer != null)
                {
                    localPlayer.GetComponentInChildren<PlayerShoot>().enabled = true;
                    localPlayer.GetComponentInChildren<PlayerLook>().enabled = true;
                }
            }
        }
    }
    #endregion

}
