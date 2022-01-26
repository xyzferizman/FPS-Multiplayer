using MLAPI;
using UnityEngine;

public class NetworkStarter : MonoBehaviour
{
    public CheckForTransportServer transportScript;
    public PUN_Starter pun_starterScript;

    private void Start()
    {
        string netType = PlayerPrefs.GetString("networkType");

        //// FOR TESTING PURPOSES
        //netType = "transport";
        //PlayerPrefs.SetString("networkType","transport");

        // hardcoded poslije diplomskog
        netType = "mlapi";

        if ( netType.Equals("mlapi"))
        {
            //if (!Application.isEditor)
            //    time1 = Time.realtimeSinceStartupAsDouble;
            //NetworkManager.Singleton.gameObject.SetActive(true);
            // pokusaj se spojiti na server/hosta kao client            
            Debug.Log("mlapi chosen");
            NetworkManager.Singleton.StartClient();

            // ako nejde, postani host na kojeg se moze spojiti (PRETPOSTAVKA: ako nejde, nema servera/hosta)
            Invoke("CheckIfClientConnected",2f);
        }
        else if ( netType.Equals("transport"))
        {            
            // start transport
            Debug.Log("transport chosen, enabling starter script");
            transportScript.enabled = true;
        }
        else if ( netType.Equals("pun_v2"))
        {
            //if (!Application.isEditor)
            //    time1 = Time.realtimeSinceStartupAsDouble;
            // start pun v2
            Debug.Log("pun 2 chosen, enabling starter script");
            pun_starterScript.enabled = true;            
        }
        else
        {
            // error: wrong netType received
        }
    }

    // ovo je bad coding practice, trebalo bi premjestit u MLAPI-specific kod, ne u ovaj "general" kod
    private void CheckIfClientConnected()   // za mlapi
    {
        Debug.Log("check if client connected");
        // if not connected, assume there is no server/host, so start as a host (for other clients to connect to)
        if (!NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.StopClient();
            NetworkManager.Singleton.StartHost();
        }
    }
}
