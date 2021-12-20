using UnityEngine;
using System.Collections.Generic;

public class Transport_Player : MonoBehaviour
{
    public int uniqueId;
    internal float health;
    internal bool updated = false;  // bitno samo za "prave" klijente (da nije host client)
    private TransportClient myClient = null;
    private TransportServer myServer = null;
    private Camera camera;
    private TransportSpawner transportSpawner;
    private Transform healthBarTransform;
    public HashSet<(int clientId, int projectileId)> recentlyCollidedWith;
    private bool isHost;
    Respawn respawner;

    NetworkStarter ns;
    double time1, time2;

    private void Awake()
    {
        if (!Application.isEditor)
            time2 = Time.realtimeSinceStartupAsDouble;

        ns = FindObjectOfType<NetworkStarter>();
        time1 = ns.time1;

        Debug.Log("Time for spawning Transport player = " + 1000 * (time2 - time1) + "ms");
    }

    private void Start()
    {
        respawner = FindObjectOfType<Respawn>();
        recentlyCollidedWith = new HashSet<(int clientId, int projectileId)>();
        myServer = FindObjectOfType<TransportServer>();
        if ( myServer != null)
            isHost = true;

        // only if local player object this will run
        TransportClient someClient = FindObjectOfType<TransportClient>();
        if (someClient.myUniqueId == uniqueId)
        {
            myClient = someClient;
        }
        else
            //Debug.Log("client id compare didnt pass: transport player id: " + uniqueId + ", client.uniqueid = " + someClient.myUniqueId);
            
        if (myClient != null) // ako je ovo lokalni player
            camera = GetComponentInChildren<Camera>();

        transportSpawner = FindObjectOfType<TransportSpawner>();

        healthBarTransform = GetComponentInChildren<HealthBarScript>().transform;
    }

    internal void SetUpdated()
    {
        updated = true;
    }

    internal void NetworkShoot(float projectileSpeed)
    {
        Vector3 position = camera.transform.position + camera.transform.forward * 0.68f;
        Vector3 velocity = camera.transform.forward * projectileSpeed;

        Transport_Projectile newProjectile = transportSpawner.SpawnProjectile(position, velocity, uniqueId).GetComponent<Transport_Projectile>();

        Dictionary<int, Transport_Projectile> helpDict;
        if (!myClient.projectiles.TryGetValue(uniqueId, out helpDict))
        {
            helpDict = new Dictionary<int, Transport_Projectile>();
            myClient.projectiles.Add(uniqueId, helpDict);
            helpDict.Add(newProjectile.uniqueProjectileId, newProjectile);  // u novododani dict dodaj trenutni projektil
                                                                            //Debug.Log("added inner dict for client " + uniqueId);
            if (isHost)
            {
                //Debug.Log("Transport_Player, isHost segment");
                if (!myServer.projectiles.TryGetValue(uniqueId, out helpDict))
                {
                    helpDict = new Dictionary<int, Transport_Projectile>();
                    myServer.projectiles.Add(uniqueId, helpDict);                    
                }
                helpDict.Add(newProjectile.uniqueProjectileId, newProjectile);

                //Debug.Log("adding to inner dict of client " + uniqueId + ", projectile with id " + newProjectile.uniqueProjectileId);
            }
        }
        else
        {
            helpDict.Add(newProjectile.uniqueProjectileId, newProjectile);

            if (isHost)
            {
                if ( !myServer.projectiles.TryGetValue(uniqueId, out helpDict))
                {
                    helpDict = new Dictionary<int, Transport_Projectile>();
                    myServer.projectiles.Add(uniqueId, helpDict);
                }
                //Debug.Log("adding to inner dict of client " + uniqueId + ", projectile with id " + newProjectile.uniqueProjectileId);
                helpDict.Add(newProjectile.uniqueProjectileId, newProjectile);
            }            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ( isHost )   // identify collisions only on host
        {
            if ( collision.collider.tag == "Projectile" )
            {
                int clientId_ = collision.collider.gameObject.GetComponent<Transport_Projectile>().originatingClientId;
                int projId_ = collision.collider.gameObject.GetComponent<Transport_Projectile>().uniqueProjectileId;
                (int clientId, int projectileId) projTuple = (clientId_, projId_);


                if (recentlyCollidedWith.Contains( projTuple ))
                {
                    return;
                }
                else
                {                    
                    recentlyCollidedWith.Add( projTuple );
                    //Debug.Log("unique projectile added.");
                }
                
                //Debug.Log("on collision checkpoint");
                health -= 10f;
                healthBarTransform.localScale = new Vector3( health / 100f, 0.15f, 0.01f);

                if ( health <= 0 )
                {
                    //Destroy(gameObject);    // destroy locally, non-owner clients - no more sending data so auto destroy, owner-client, message

                    myServer.shouldBeDestroyed_Players.Add(uniqueId);   // predispose for destruction at the end of server-loop
                }
            }
        }
    }

    private void OnDestroy()
    {
        //if ( isHost )
        //{
        //    if (myClient.localPlayer.uniqueId != uniqueId)    // ako nije hostov igrac
        //        myServer.PlayerDestroyed_NotifyClient(uniqueId);
        //    // ako je, samo Destroy (tamo gore u kodu) i sam ce se unistiti na ostalim klijentima, zapoceti proceduru respawna
        //    else    // ako je unisten lokalni igrac
        //    {
        //        // kickstart respawn procedure
        //        respawner.InitializeRespawnCountdown();

        //        myServer.players.Remove(uniqueId);
        //        myServer.playersList.Remove(this);

        //        myClient.players.Remove(uniqueId);
        //        myClient.playersList.Remove(this);
        //    }
        //}

    }
}
