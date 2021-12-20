using UnityEngine;
using UnityEngine.UI;
using MLAPI;
using MLAPI.Messaging;
using MLAPI.Connection;
using MLAPI.Spawning;
using System.Collections.Generic;

public class NetworkGameManager : NetworkBehaviour
{
    private Text countdownText;
    private Button respawnButton;
    private GameObject UIPanel;
    private readonly int maxCountDownValue = 5;
    private int currentCountDownValue;
    private Camera respawnCamera;
    private bool spawnCameraWhenOldDespawned = false;
    private Vector3 newCamPos;
    private Quaternion newCamRot;
    private MLAPI_Player myLocalMlapiPlayer;
    private bool callServerRpc1 = false;
    private Respawn respawner;

    internal List<double> listOfRPCRoundTimes;

    internal ulong thirdClientId;
    internal double time1, time2;
    internal double time11, time22;
    ClientRpcParams testParams123;

    private void Start()
    {
        listOfRPCRoundTimes = new List<double>();
        UIPanel = FindObjectOfType<RespawnUI>().gameObject;
        countdownText = UIPanel.transform.GetChild(0).gameObject.GetComponent<Text>();
        respawnButton = UIPanel.transform.GetChild(1).gameObject.GetComponent<Button>();
        respawner = FindObjectOfType<Respawn>();
    }

    [ClientRpc]
    internal void PlayerDestroyed_ClientRpc(ClientRpcParams clientRpcParams = default)
    {
        // detach camera
        respawnCamera = myLocalMlapiPlayer.GetComponentInChildren<Camera>();    
        respawnCamera.transform.parent = null;

        // detachati kameru svom lokalnom player objectu, pa javiti serveru da moze unistiti player object
        CanDestroyPlayerObject_ServerRpc(NetworkManager.Singleton.LocalClientId);

    }
        
    [ServerRpc(RequireOwnership = false)]
    internal void CanDestroyPlayerObject_ServerRpc(ulong clientToBeDespawned, ServerRpcParams serverRpcParams = default)
    {
        NetworkClient netClientToDestroy;
        NetworkManager.Singleton.ConnectedClients.TryGetValue(clientToBeDespawned, out netClientToDestroy);
        netClientToDestroy.PlayerObject.GetComponent<MLAPI_Player>().CallDespawn();        
    }

    [ServerRpc(RequireOwnership = false)]
    internal void RespawnClientPlayer_ServerRpc(ulong requestingClientId)
    {
        ulong playerPrefabHash = NetworkSpawnManager.GetPrefabHashFromGenerator("PlayerCharacter");
        GameObject myPrefab = NetworkManager.Singleton.NetworkConfig.NetworkPrefabs
            .Find(prefab => prefab.Prefab.GetComponent<NetworkObject>().PrefabHash.Equals(playerPrefabHash)).Prefab;

        GameObject playerObj = Instantiate(myPrefab, RandomPosition(), RandomRotation());
        playerObj.GetComponent<NetworkObject>().SpawnAsPlayerObject(requestingClientId);
    }

    [ClientRpc]
    internal void SendRpcToThirdClientTime1_ClientRpc(ClientRpcParams someParams)
    {
        time1 = Time.realtimeSinceStartupAsDouble;
        Debug.Log("1st rpc called");
    }

    [ServerRpc(RequireOwnership = false)]
    internal void SendRpcToThirdClientTime2_ServerRpc()
    {        
        //Debug.Log("2nd rpc called");
        SendRpcToThirdClientTime2_ClientRpc(testParams123);
    }

    [ClientRpc]
    internal void SendRpcToThirdClientTime2_ClientRpc(ClientRpcParams someParams)
    {
        time2 = Time.realtimeSinceStartupAsDouble;
        Debug.Log("2nd rpc called");
        Debug.Log("MLAPI time for destroying remote player = " + 1000 * (time2 - time1 - 0.04) + "ms");
    }

    [ServerRpc(RequireOwnership = false)]
    internal void Test_ServerRpc()
    {
        Debug.Log("test_server_rpc");
        Test_ClientRpc();
    }

    [ClientRpc]
    internal void Test_ClientRpc()
    {
        time22 = Time.realtimeSinceStartupAsDouble;
        Debug.Log("test_client_rpc");
        listOfRPCRoundTimes.Add(time22-time11);
    }
       
    Vector3 RandomPosition()
    {
        return new Vector3(Random.Range(-14f, 14f), 1f, Random.Range(-14f, 14f));
    }

    Quaternion RandomRotation()
    {
        float randomYRotation = Random.Range(0f, 360f);
        return Quaternion.Euler(0f, randomYRotation, 0f);
    }
        
    public void SetLocalMlapiPlayer(MLAPI_Player mlapi_player)
    {
        myLocalMlapiPlayer = mlapi_player;
    }

    public Camera GetRespawnCamera()
    {
        return respawnCamera;
    }

    internal void SetClientParams(ClientRpcParams paramsToSet)
    {
        testParams123 = paramsToSet;
    }

}
