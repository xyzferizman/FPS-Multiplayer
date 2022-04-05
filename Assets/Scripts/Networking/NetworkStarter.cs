using UnityEngine;

public class NetworkStarter : MonoBehaviour
{
    public MLAPI_Starter mlapiScript;
    public CheckForTransportServer transportScript;
    public PUN_Starter pun_starterScript;

    private void Start()
    {
        string netType = PlayerPrefs.GetString("networkType");

        // hardcoded poslije diplomskog
        netType = "mlapi";

        if (netType.Equals("mlapi"))
            mlapiScript.enabled = true;
           
        else if (netType.Equals("transport"))
            transportScript.enabled = true;

        else if (netType.Equals("pun_v2"))
            pun_starterScript.enabled = true;

        else
            Debug.LogError("NetworkStarter Error: Wrong NetType received.");
    }
}
