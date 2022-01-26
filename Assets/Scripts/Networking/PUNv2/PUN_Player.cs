using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;

public class PUN_Player : MonoBehaviourPunCallbacks//, IPunOwnershipCallbacks
{
    private PlayerLook lookScript;
    private PlayerMove moveScript;
    private PlayerShoot shootScript;
    private Camera camera;
    private HashSet<int> recentlyCollidedWith;
    private Transform healthBarTransform;
    private float health;
    private Respawn respawner;
    private PhotonView projectilePhotonView;
    private float oldHealth;
    private float healthDiff;
    private bool shouldStartRespawnRoutine;
    private bool healthSynced = false;

    PUN_Starter myPunStarter;

    NetworkStarter ns;
    double time1, time2;
    double _time1;

    //private void Awake()
    //{
    //    if (!Application.isEditor)
    //        time2 = Time.realtimeSinceStartupAsDouble;

    //    ns = FindObjectOfType<NetworkStarter>();
    //    time1 = ns.time1;

    //    Debug.Log("Time for spawning PUN2 player = " + 1000 * (time2 - time1) + "ms");
    //}

    // I guess using normal Start is ok
    void Start()
    {

        myPunStarter = FindObjectOfType<PUN_Starter>();
        camera = GetComponentInChildren<Camera>();
        moveScript = GetComponent<PlayerMove>();
        lookScript = GetComponentInChildren<PlayerLook>();
        shootScript = GetComponentInChildren<PlayerShoot>();        
        healthBarTransform = GetComponentInChildren<HealthBar>().transform;
        respawner = FindObjectOfType<Respawn>();
        recentlyCollidedWith = new HashSet<int>();

        if ( !photonView.AmOwner )
        {
            Destroy(camera.gameObject);
            moveScript.enabled = false;
            shootScript.enabled = false;
            photonView.RPC("AskOwnerForHealth", photonView.Owner, PhotonNetwork.LocalPlayer);
        }
        else
        {
            // this is called on 1st spawn/respawn of networked object
            Debug.Log("Client " + photonView.OwnerActorNr);
            health = 100f;
            // health bar default scale 1.0
        }

        
    }

    [PunRPC]
    private void AskOwnerForHealth(Player playerToRespondTo)
    {
        // give health value to requesting client program
        photonView.RPC("SyncHealthForRemoteCopy", playerToRespondTo, health);
    }

    [PunRPC]
    private void SyncHealthForRemoteCopy(float _health)
    {
        health = _health;
        healthBarTransform.localScale = new Vector3(health / 100f, healthBarTransform.localScale.y, healthBarTransform.localScale.z);
        healthSynced = true;
    }

    public void NetworkShoot(float projectileSpeed)
    {
        Vector3 position = camera.transform.position + camera.transform.forward * 0.68f;
        Vector3 velocity = camera.transform.forward * projectileSpeed;
        //photonView.RPC("RPC_NetworkShoot", PhotonNetwork.MasterClient, position, velocity);

        // s ovom implementacijom svaki klijent spawna svoje projektile
        GameObject projectile = MasterManager.instance.GetProjectilePrefab();
        projectile = MasterManager.NetworkInstantiate(projectile, position, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().velocity = velocity;

    }
        
    // executes on master client
    [PunRPC]
    private void RPC_NetworkShoot(Vector3 position, Vector3 velocity)
    {
        // send RPC to master client to spawn and shoot projectile
        GameObject projectile = MasterManager.instance.GetProjectilePrefab();

        // promijeniti ovo, staviti u if-ove ako zelimo simulirati servera sa Master Clientom
        projectile = MasterManager.NetworkInstantiate(projectile, position, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().velocity = velocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        collision.collider.gameObject.GetComponent<MeshRenderer>().enabled = false;
        collision.collider.gameObject.GetComponent<SphereCollider>().enabled = false;
        collision.collider.gameObject.GetComponent<Rigidbody>().detectCollisions = false;

        if ( PhotonNetwork.IsMasterClient )
        {
            if (recentlyCollidedWith.Contains(collision.collider.gameObject.GetInstanceID()))
                return;
            else
                recentlyCollidedWith.Add(collision.collider.gameObject.GetInstanceID());

            if (collision.collider.tag == "Projectile" /*&& !collision.collider.GetComponent<NetworkObject>().IsOwner*/ ) // if hits a projectile AND projectile isnt of that player object
            {
                projectilePhotonView = collision.collider.gameObject.GetComponent<PhotonView>();
                projectilePhotonView.RequestOwnership();

                photonView.RPC("RPC_ChangeHealthOfPlayer", RpcTarget.All);

                //Debug.Log("On Client " + photonView.OwnerActorNr);
            }
        }
    }

    [PunRPC]
    private void RPC_ChangeHealthOfPlayer()
    {
        oldHealth = health;
        health -= 10f;
        healthDiff = health - oldHealth;
        healthBarTransform.localScale += new Vector3(healthDiff / 100f, 0f, 0f);        

        if (health <= 0f && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_CallClientDetachCamera", photonView.Owner);
        }

    }

    // executes on client, destroys camera, destroys object(since client-auth and has ownership of player object, start respawn routine)
    [PunRPC]
    private void RPC_CallClientDetachCamera()
    {
        oldHealth = health;
        health -= 10f;
        healthDiff = health - oldHealth;
        healthBarTransform.localScale += new Vector3(healthDiff / 100f, 0f, 0f);
        //Debug.Log("RPC_CallClientDetachCamera");
        camera.transform.parent = null;
        shouldStartRespawnRoutine = true;
        photonView.RPC("RPC_SendRPCWithTime", RpcTarget.All);
        PhotonNetwork.Destroy(photonView);        
    }
        
    // executes on all clients
    [PunRPC]
    private void RPC_SendRPCWithTime()
    {
        time1 = Time.realtimeSinceStartupAsDouble;
        Debug.Log("time1 set ");
    }

    [PunRPC]
    private void RPC_SendRPCWithTime2()
    {
        time2 = Time.realtimeSinceStartupAsDouble;
        Debug.Log("time2 set " );

        Debug.Log("PUN time for destroying remote player = " + 1000 * (time2 - time1) + "ms");
    }

    private void OnDestroy()
    {
        if ( PhotonNetwork.IsMasterClient )
            photonView.RPC("RPC_SendRPCWithTime2", RpcTarget.All);

        time2 = Time.realtimeSinceStartupAsDouble;
        Debug.Log("PUN time for destroying remote player = " + 1000 * (time2 - time1) + "ms");
        
        if (shouldStartRespawnRoutine)
        {
            respawner.InitializeRespawnCountdown();
            shouldStartRespawnRoutine = false;
        }
    }

}
