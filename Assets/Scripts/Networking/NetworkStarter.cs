using MLAPI;
using UnityEngine;

public class NetworkStarter : MonoBehaviour
{
    public CheckForTransportServer transportScript;
    public PUN_Starter pun_starterScript;

    public double time1;

    private void Start()
    {
        string netType = PlayerPrefs.GetString("networkType");

        //// FOR TESTING PURPOSES
        //netType = "transport";
        //PlayerPrefs.SetString("networkType","transport");

        

        if ( netType.Equals("mlapi"))
        {
            //if (!Application.isEditor)
            //    time1 = Time.realtimeSinceStartupAsDouble;
            //NetworkManager.Singleton.gameObject.SetActive(true);
            // pokusaj se spojiti na server/hosta kao client            
            NetworkManager.Singleton.StartClient();

            // ako nejde, postani host na kojeg se moze spojiti (PRETPOSTAVKA: ako nejde, nema servera/hosta)
            Invoke("CheckIfClientConnected",2f);
        }
        else if ( netType.Equals("transport"))
        {
            if (!Application.isEditor)
                time1 = Time.realtimeSinceStartupAsDouble;
            // start transport
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
