using UnityEngine;
using UnityEngine.SceneManagement;

using MLAPI;
using MLAPI.NetworkVariable;
using MLAPI.Spawning;
using MLAPI.Messaging;

using System.Collections.Generic;

public class MLAPI_Player : NetworkBehaviour
{
    private PlayerMove moveScript;
    private PlayerShoot shootScript;
    private Camera camera;
    private float health;
    private Transform healthBarTransform;
    private NetworkObject netObj;
    private HashSet<int> recentlyCollidedWith;
    private int respawnTimer = 5;
    private NetworkGameManager myNetGameManager;
    private Respawn respawner;
    private bool disconnectCallbackAlreadyHappened;

    NetworkStarter ns;


    internal bool canGoNextStep = true;
    double time1, time2;

    //private void Awake()
    //{
    //    if (!Application.isEditor)
    //        time2 = Time.realtimeSinceStartupAsDouble;

    //    ns = FindObjectOfType<NetworkStarter>();
    //    time1 = ns.time1;

    //    Debug.Log("Time for spawning MLAPI player = " + 1000 * (time2 - time1) + "ms");
    //}

    NetworkVariableFloat Health = new NetworkVariableFloat(new NetworkVariableSettings
    {
        WritePermission = NetworkVariablePermission.ServerOnly,
        ReadPermission = NetworkVariablePermission.Everyone
    });

    private void SpawnNetworkGameManager()
    {
        ulong gameManagerPrefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("NetworkGameManager");
        GameObject myPrefab = NetworkManager.Singleton.NetworkConfig.NetworkPrefabs.Find(prefab => prefab.Prefab.GetComponent<NetworkObject>().PrefabHash.Equals(gameManagerPrefabHash)).Prefab;

        GameObject gameManager = Instantiate(myPrefab, new Vector3(0f,0f,0f), Quaternion.identity);
        gameManager.GetComponent<NetworkObject>().Spawn();
    }

    // moja custom funkcija spojena na callback OnClientDisconnectCallback
    // OnClientDisconnectCallback se zove na klijentu AKO:
        // 1) server proaktivno disconnectao klijenta
        // 2) server se shut downao
    void OnClientDisconnected(ulong clientid)
    {
        Debug.Log("on client disconnected called for cliendid = " + clientid + " on client player object " + OwnerClientId);

        if (disconnectCallbackAlreadyHappened)
        {
            Debug.Log("return condition passed (disconnect callback already happened)");
            return;
        }        

        disconnectCallbackAlreadyHappened = true;

        if (IsHost)
            return;

        PlayerPrefs.SetInt("Server_DCClient_or_Shutdown",1);
        SceneManager.LoadScene(0);  // ucitaj main menu
        Debug.Log("You got disconnected by the server or server shutdown happened.");
    }

    public override void NetworkStart()
    {
        camera = GetComponentInChildren<Camera>();
        moveScript = GetComponent<PlayerMove>();
        shootScript = GetComponentInChildren<PlayerShoot>();
        healthBarTransform = GetComponentInChildren<HealthBar>().transform;
        netObj = GetComponent<NetworkObject>();
        respawner = FindObjectOfType<Respawn>();

        if (IsOwner) // inace se poziva na svim MLAPI_Player instancama u lokalnoj sceni
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        if ( !IsLocalPlayer )
        { 
            Destroy(camera.gameObject);
            moveScript.enabled = false;
            shootScript.enabled = false;
        }
        else // if local player
        {
            FindObjectOfType<CursorLocking>().SetLocalPlayer(gameObject);

            myNetGameManager = FindObjectOfType<NetworkGameManager>();
            if (myNetGameManager == null && IsServer)
            {
                SpawnNetworkGameManager();
                myNetGameManager = FindObjectOfType<NetworkGameManager>();
            }
            // ako je local player
            myNetGameManager.SetLocalMlapiPlayer(this); 

            Camera respawnCamera = myNetGameManager.GetRespawnCamera();

            if (respawnCamera != null)
            {
                respawnCamera.gameObject.SetActive(false);
                Destroy(respawnCamera);
            }
        }

        if ( IsServer )
        {
            if ( myNetGameManager == null ) myNetGameManager = FindObjectOfType<NetworkGameManager>();
            recentlyCollidedWith = new HashSet<int>();
            RandomizePosition();
            health = 100f;
            Health.Value = health;              
        }
        
        Debug.Log("player object " + OwnerClientId + ": is my net game manager null? " + (myNetGameManager == null));

        Health.OnValueChanged += UpdateHealthBar;
    }

    private void OnCollisionEnter(Collision collision)
    {        
        if ( IsServer)
        {
            // recentlyCollidedWith je HashSet<int>
            if (recentlyCollidedWith.Contains(collision.collider.gameObject.GetInstanceID()))
                return;
            else
                recentlyCollidedWith.Add(collision.collider.gameObject.GetInstanceID());

            Debug.Log("OnCollisionEnter on MLAPI_Player entered, NetworkObject: " + netObj.NetworkObjectId, this);
            if (collision.collider.tag == "Projectile" /*&& !collision.collider.GetComponent<NetworkObject>().IsOwner*/ ) // if hits a projectile AND projectile isnt of that player object
            {
                //Debug.Log("projectile hit registered.");
                health -= 10f;
                Health.Value = health;
            }            
        }  
    }

    void RandomizePosition()
    {
        Vector3 randomPosition = new Vector3(Random.Range(-34f,34f),1f, Random.Range(-34f, 34f));
        float randomYRotation = Random.Range(0f,360f);

        transform.position = randomPosition;
        transform.rotation = Quaternion.Euler(0f,randomYRotation,0f);
    }

    public void NetworkShoot(float projectileSpeed)
    {
        Vector3 position = camera.transform.position + camera.transform.forward * 0.68f;
        Vector3 velocity = camera.transform.forward * projectileSpeed;

        if ( IsServer ) // we're using host
        {
            SpawnProjectile(position, velocity, NetworkManager.Singleton.LocalClientId);
        }
        else
        {
            // poslat rpc hostu da spawna? mozda network transform rijesi problem
            SpawnProjectileForClient_ServerRpc(position, velocity, NetworkManager.Singleton.LocalClientId);
        }
    }

    [ServerRpc]
    private void SpawnProjectileForClient_ServerRpc(Vector3 position, Vector3 velocity, ulong remoteClientId)
    {
        SpawnProjectile(position, velocity, remoteClientId);
    }
    
    // will run only on server/host
    private void SpawnProjectile(Vector3 position, Vector3 velocity, ulong clientId)
    {
        ulong projectilePrefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("Projectile");
        GameObject myPrefab = NetworkManager.Singleton.NetworkConfig.NetworkPrefabs
            .Find(prefab => prefab.Prefab.GetComponent<NetworkObject>().PrefabHash.Equals(projectilePrefabHash)).Prefab;

        GameObject projectile = Instantiate(myPrefab, position, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().velocity = velocity;
        // can only be called from the server
        projectile.GetComponent<NetworkObject>().Spawn();
    }
   
    void UpdateHealthBar(float prevHealth, float nextHealth)
    {
        if ( nextHealth <= 0f )
        {
            if ( IsServer )
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { OwnerClientId }
                    }
                };
                myNetGameManager.PlayerDestroyed_ClientRpc(clientRpcParams);
            }
            
        }

        float changedHealth = nextHealth - prevHealth;
        healthBarTransform.localScale += new Vector3(changedHealth / 100f, 0f ,0f);
    }
    
    internal void CallDespawn()
    {
        Debug.Log("despawn called");

        myNetGameManager.thirdClientId = FindThirdClient(netObj.OwnerClientId);

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { FindThirdClient( OwnerClientId ) }
            }
        };
        myNetGameManager.SetClientParams(clientRpcParams);
        myNetGameManager.SendRpcToThirdClientTime1_ClientRpc(clientRpcParams);
        //Debug.Log("time1 written to Time1 network variable");
        netObj.Despawn();
        Destroy(gameObject);
    }

    private ulong FindThirdClient(ulong ownerClient_id)
    {
        foreach ( var entry in NetworkManager.ConnectedClients )
        {
            if (entry.Key != ownerClient_id && entry.Key != NetworkManager.ServerClientId)
            {
                return entry.Key;
            }
        }
        return (ulong)1234567;
    }

    // ocekuje se poziv kada server despawna klijentskog playera ili svog vlastitog
    private void OnDestroy()
    {
        if ( IsLocalPlayer )
        {            
            if ( !IsServer )
                myNetGameManager.SendRpcToThirdClientTime2_ServerRpc();

            //Debug.Log("server rpc should be called");
            respawner.InitializeRespawnCountdown();
        }            
    }
}
