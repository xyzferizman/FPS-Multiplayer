using UnityEngine;
using System.Collections.Generic;

public class Transport_Projectile : MonoBehaviour
{
    internal int originatingClientId;
    internal int uniqueProjectileId;
    bool isHost = false;
    TransportServer myServer;

    Dictionary<int, Transport_Projectile> innerDict;    // samo za drzanje referenci dolje u kodu

    internal bool updated;

    public Transport_Projectile()
    {

    }

    public Transport_Projectile(int origClId, int uqProjId)
    {
        originatingClientId = origClId;
        uniqueProjectileId = uqProjId;
    }

    internal void SetUpdated()
    {
        updated = true;
    }

    private void Start()
    {
        myServer = FindObjectOfType<TransportServer>();

        if ( myServer != null)
            isHost = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ( isHost )
        {
            // javi strukturi podataka da je unisten taj specifican projektil
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if ( isHost )
        {
            //Debug.Log("Transport_Projectile, OnDestroy(): origClientId = " + originatingClientId);
            if (!myServer.projectiles.TryGetValue(originatingClientId, out innerDict))
                Debug.LogError("Transport_Projectile, OnDestroy method, clientid=" + originatingClientId + ", projectileId=" + uniqueProjectileId 
                    + ", projectiles dictionary for client doesn't exist and it should");
            //Debug.Log("inner dict is null?" + (innerDict is null));
            //Debug.Log("inner dict contains uniqueProjId = " + uniqueProjectileId + " ... :" + innerDict.ContainsKey(uniqueProjectileId));
            innerDict.Remove(uniqueProjectileId);   // removed on server, how to notify originating client?
            List<int> helperList;
            if (myServer.notifyClientsToDestroyProjectiles.TryGetValue(originatingClientId, out helperList))
                helperList.Add(uniqueProjectileId);
            else
            {
                List<int> anotherHelperList = new List<int>();
                myServer.notifyClientsToDestroyProjectiles.Add(originatingClientId, anotherHelperList);
                anotherHelperList.Add(uniqueProjectileId);
            }
            // to notify originating clients of their destroyed projectiles so they update their data
        }


    }
}
