using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class PUN_Starter : MonoBehaviourPunCallbacks
{
    private string roomName = "myRoomName123";

    public GameObject playerPrefab;

    void Start()
    {
        print("Connecting to Photon...");
        //Debug.Log("send rate: " + PhotonNetwork.SendRate);
        //Debug.Log("serialization rate: " + PhotonNetwork.SerializationRate);
        PhotonNetwork.SendRate = 50;    // 30
        PhotonNetwork.SerializationRate = 20;   // 10
        PhotonNetwork.NickName = MasterManager.GameSettings.NickName;
        PhotonNetwork.GameVersion = MasterManager.GameSettings.GameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby();
            //Debug.Log("lobby joined");
        }
        Debug.Log("cloud region: " + PhotonNetwork.CloudRegion);
        
        RoomOptions options = new RoomOptions();
        options.BroadcastPropsChangeToAll = true;
        options.MaxPlayers = 6;        
        Debug.Log("joining or creating a room");

        Debug.Log("count of rooms: " + PhotonNetwork.CountOfRooms);

        PhotonNetwork.JoinOrCreateRoom(roomName, options, TypedLobby.Default);
        Invoke("InvokedMethod",7f);
    }

    void InvokedMethod()
    {
        Debug.Log("count of rooms, 7 sec after joinorcreate: " + PhotonNetwork.CountOfRooms);
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

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        Debug.Log("Failed to connect to Photon: " + cause.ToString(), this);
    }

    public override void OnCreatedRoom()
    {
        // ovo je master client
        //MasterManager.NetworkInstantiate(playerPrefab, RandomPosition(), RandomRotation());
        Debug.Log("room created");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("on join room failed, Debug message: " + message);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("joined room " + PhotonNetwork.CurrentRoom);
        MasterManager.NetworkInstantiate(playerPrefab, RandomPosition(), RandomRotation());
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("room list update called");
        Debug.Log("room list count: " + roomList.Count);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby.");
    }

    public override void OnLeftLobby()
    {
        Debug.Log("Left lobby.");
    }

    internal void RespawnPlayer()
    {
        Camera cam = FindObjectOfType<Camera>();
        Debug.Log("PUN_Starter.RespawnPlayer... is cam null? " + (cam == null));
        Destroy(cam.gameObject);
        //while ( cam.gameObject != null )
        //{
        //    // just wait for camera to perish
        //}
        Debug.Log("after destroy, cam.gameObject = " + cam.gameObject);
        MasterManager.NetworkInstantiate(playerPrefab, RandomPosition(), RandomRotation());
    }
}

