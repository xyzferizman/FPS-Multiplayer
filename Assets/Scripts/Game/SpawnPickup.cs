using UnityEngine;
using MLAPI;
using System;

public class SpawnPickup : MonoBehaviour
{
    public GameObject speedPickup;
    public GameObject damagePickup;
    public GameObject wallPickup;
    public float pickupRadius = 1.2f;
    public float pickupSpawnDelay = 4f;
    public LayerMask pickupSpawnLayerMask;

    bool itsSpawnTime = false;
    bool correctSpawnLocation = false;
    float lastSpawnTime = 0f;
    GameObject pickupToSpawnPrefab;
    GameObject pickupToSpawn;
    ushort spawnCounter = 1;

    ulong speedPickupHash;
    ulong damagePickupHash;
    ulong wallsPickupHash;

    private void Start()
    {
        speedPickupHash = speedPickup.GetComponent<NetworkObject>().PrefabHash;
        damagePickupHash = damagePickup.GetComponent<NetworkObject>().PrefabHash;
        wallsPickupHash = wallPickup.GetComponent<NetworkObject>().PrefabHash;
    }

    // Update is called once per frame
    void Update()
    {
        if ( NetworkManager.Singleton.IsServer )
        {
            if ((Time.time - lastSpawnTime) >= pickupSpawnDelay)
            {
                itsSpawnTime = true;
                lastSpawnTime = Time.time;
            }

            if (itsSpawnTime)
            {
                itsSpawnTime = false;
                SpawnRandomPickup();
            }
        }
    }

    void SpawnRandomPickup()
    {
        System.Random r = new System.Random();

        int randomNumber = r.Next(3);
        ulong prefabHash = 1;

        if (randomNumber == 0)
        {
            prefabHash = speedPickupHash;
        }
        else if (randomNumber == 1)
        {
            prefabHash = damagePickupHash;
        }
        else if (randomNumber == 2)
        {
            prefabHash = wallsPickupHash;
        }

        pickupToSpawnPrefab = NetworkManager.Singleton.NetworkConfig.NetworkPrefabs
            .Find(prefab => prefab.Prefab.GetComponent<NetworkObject>().PrefabHash.Equals(prefabHash)).Prefab;

        // CONSTRAINTS na poziciju spawna ... x = [-33.5,+33.5], y = 1, z = [-33.5,+33.5]
        
        //pickupToSpawn.GetComponent<NetworkObject>().Spawn();

        // pokusavaj naci "validnu konfiguraciju" spawnanja pickupa DOKLE GOD je ne nadjes
        // STO je "validna konfiguracija"? 
        // u X radijusu = (radijus pickup coina + jos maaaalo vise) radijusu od determinirane spawn tocke nema:
        // 1) player objecta
        // 2) objekta prepreke

        int i = 0;
        int safety = 1000;
        Vector3 current_position;
        while ( i < safety)
        {
            current_position = GetRandomSpawnPosition();

            bool isTouchingBadLayers = Physics.CheckSphere(current_position, pickupRadius, pickupSpawnLayerMask);

            if (!isTouchingBadLayers)
            {
                //Debug.Log("Spawn " + spawnCounter + ", is there collision of sphere with certain layers? = " + result);
                pickupToSpawn = Instantiate(pickupToSpawnPrefab, current_position, Quaternion.identity);
                pickupToSpawn.name = pickupToSpawn.name + ", Spawn " + (spawnCounter++);
                pickupToSpawn.GetComponent<NetworkObject>().Spawn();
                break;
            }
            i++;
        }
    }

    Vector3 GetRandomSpawnPosition()
    {        
        return new Vector3(UnityEngine.Random.Range(-33.5f, 33.5f), 1f, UnityEngine.Random.Range(-33.5f, 33.5f));
    }
}
