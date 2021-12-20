using UnityEngine;

public class TransportSpawner : MonoBehaviour
{
    public GameObject transportPlayerPrefab;
    public GameObject transportProjectilePrefab;

    private int projectileIndex = 1;
    TransportClient myClient;

    bool uniqueIdSet = false;
    bool isHost;
        
    public void StartClient()
    {
        //Debug.Log("starting client");
        gameObject.AddComponent(typeof(TransportClient));
    }

    public void StartHost()
    {
        //Debug.Log("starting host");
        gameObject.AddComponent(typeof(TransportServer));
        isHost = true;
    }

    public GameObject SpawnProjectile(Vector3 position, Vector3 velocity, int unique_cliendId)
    {
        GameObject projectile = Instantiate(transportProjectilePrefab, position, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().velocity = velocity;
        projectile.GetComponent<Transport_Projectile>().uniqueProjectileId = projectileIndex++;
        projectile.GetComponent<Transport_Projectile>().originatingClientId = unique_cliendId;
        return projectile;
    }

    public GameObject SpawnPlayerObject(int uniqueIndex)
    {
        if ( GetComponent<TransportServer>() != null )
            Debug.Log("Host: Spawn function called", this);
        else
            Debug.Log("Client /w uniqueIndex=" + uniqueIndex + ": Spawn function called", this);

        if (isHost && !uniqueIdSet )
        {
            myClient.myUniqueId = uniqueIndex;
            uniqueIdSet = true;
        }            
        GameObject playerObject = Instantiate(transportPlayerPrefab, RandomPosition(), RandomRotation());
        playerObject.GetComponent<Transport_Player>().uniqueId = uniqueIndex;
        playerObject.GetComponent<Transport_Player>().health = 100f;
        Debug.Log("Returning player object with uniqueId = " + playerObject.GetComponent<Transport_Player>().uniqueId);
        // health bar set by default
        return playerObject;
    }

    // zove server kad je spreman, pa inicira spawnanje svog clienta (jer je host)
    internal TransportClient HostReadyCallback()
    {
        Debug.Log("adding client to host");
        myClient = (TransportClient)gameObject.AddComponent(typeof(TransportClient));
        return myClient;
    }

    //internal void CallbackWhenServerReady()
    //{
    //    // kada host spreman, onda spawnaj host-ovog igraca
    //    SpawnPlayerObject();        
    //}

    Vector3 RandomPosition()
    {
        return new Vector3(Random.Range(-14f, 14f), 1f, Random.Range(-14f, 14f));
    }

    Quaternion RandomRotation()
    {
        float randomYRotation = Random.Range(0f, 360f);
        return Quaternion.Euler(0f, randomYRotation, 0f);
    }


}
