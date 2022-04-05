using MLAPI;
using MLAPI.Transports.UNET;
using UnityEngine;

public class MLAPI_Starter : MonoBehaviour
{
    public float delay = 2f;

    private void Start()
    {
        //GetComponent<NetworkManager>().enabled = true;
        //GetComponent<UNetTransport>().enabled = true;

        NetworkManager.Singleton.StartClient();
        Invoke(nameof(CheckIfClientConnected), delay);
    }
    
    private void CheckIfClientConnected()
    {
        if ( !NetworkManager.Singleton.IsConnectedClient )
        {
            NetworkManager.Singleton.StopClient();
            NetworkManager.Singleton.StartHost();
        }
            
    }
}
