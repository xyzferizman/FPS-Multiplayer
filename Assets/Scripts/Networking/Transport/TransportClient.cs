using UnityEngine;
using Unity.Networking.Transport;
using System.Collections.Generic;
using System;

public class TransportClient : MonoBehaviour
{
    public NetworkDriver m_Driver;
    public NetworkConnection m_Connection;

    public Dictionary<int, Transport_Player> players;
    public List<Transport_Player> playersList;

    internal Dictionary<int, Dictionary<int, Transport_Projectile>> projectiles;

    private TransportSpawner transportSpawner;
    internal Respawn respawner;

    private bool isHost;

    //int counter = 1;

    private bool isClientReadyToReceiveData;
    private bool isClientReadyToSendData;
    private bool onGUIcanStart;

    private bool localPlayerSet;
    private bool shouldSyncOnSpawn = true;
    private bool someBool;
    private bool clientSendingFirstTime = true;
    private bool destroyed = true;
    //bool keyPressedThisUpdate = true;
    
    public int myUniqueId;

    internal Transport_Player localPlayer = null;

    void Start()
    {
        #region data_struct_init
        transportSpawner = GetComponent<TransportSpawner>();
        respawner = GetComponent<Respawn>();
        players = new Dictionary<int, Transport_Player>();
        playersList = new List<Transport_Player>();
        projectiles = new Dictionary<int, Dictionary<int, Transport_Projectile>>();
        #endregion

        if (GetComponent<TransportServer>() != null) isHost = true;

        NetworkConfigParameter param = new NetworkConfigParameter();
        param.disconnectTimeoutMS = 1000000; // for dev purposes
        param.maxConnectAttempts = 5000;

        m_Driver = NetworkDriver.Create(param);
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        m_Connection = m_Driver.Connect(endpoint);
        //Debug.Log("Client: trying to connect to server...");
        onGUIcanStart = true;
    }

    internal void SetHostObject(Transport_Player myPlayer)
    {
        myPlayer.gameObject.AddComponent<CursorLocking>();
        players.Add(myPlayer.uniqueId, myPlayer);
        playersList.Add(myPlayer);
        localPlayer = myPlayer;
    }

    public void OnDestroy()
    {
        if (m_Driver.IsCreated)
            m_Driver.Dispose();
    }

    void Update()
    {
        m_Driver.ScheduleUpdate().Complete();

        if (!m_Connection.IsCreated)
        {
            return;
        }
        //counter++;
        DataStreamReader stream;
        DataStreamWriter writer;
        NetworkEvent.Type cmd;

        #region receiving                  
        while ((cmd = m_Connection.PopEvent(m_Driver, out stream)) != NetworkEvent.Type.Empty)
        {
            // kada klijent dobi potvrdu od servera da je spojen
            if (cmd == NetworkEvent.Type.Connect)
            {
                #region receiving_connection_approval_from_server
                //Debug.Log("Now connected to server, TransportClient");
                // poslati serveru zahtjev za spawnanjem player objecta
                m_Driver.BeginSend(m_Connection, out writer);
                int _keycode = -1;
                //Debug.Log("Client: Writing message code=" + _keycode + " to stream for REQUEST_SPAWN");
                writer.WriteInt(_keycode);  // -1 - KEYCODE for requesting player object spawn
                m_Driver.EndSend(writer);
                //keyPressedThisUpdate = false;

                // sad treba ocekivati podatke sa servera o game state-u, pa ce ga moci lokalno replicirati
                #endregion
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                int messageKeyCode;
                messageKeyCode = stream.ReadInt();
                //Debug.Log("Client: READING keycode = " + messageKeyCode);
                
                //Debug.Log("Client: Reading (messageCode) = " + messageKeyCode + " from stream");

                if (messageKeyCode == -1)
                {
                    #region receiving_spawn_response_from_server
                    if (!isClientReadyToReceiveData)
                    {
                        myUniqueId = stream.ReadInt();
                        //Debug.Log("Client: Reading (myUniqueId) = " + myUniqueId + " from stream");

                        // ugasi player skripte na lokalnim kopijama remote player objecta
                        if (isHost)
                        {

                        }

                        isClientReadyToReceiveData = true;
                        //Debug.Log("Client: isClientReady = true");
                    }
                    #endregion
                }
                else if (messageKeyCode == -4)
                {
                    destroyed = true;
                    players.Remove(localPlayer.uniqueId);
                    playersList.Remove(localPlayer);
                    Destroy(localPlayer.gameObject);
                    respawner.InitializeRespawnCountdown();
                }
                else if ( messageKeyCode == -5)
                {
                    int clientIdForDestruction = stream.ReadInt();
                    if (players.TryGetValue(clientIdForDestruction, out Transport_Player currPlayer123))
                        playersList.Remove(currPlayer123);
                    else
                        Debug.LogError("didnt find player to destroy, shouldve found it");
                    players.Remove(clientIdForDestruction);                    
                    Destroy(localPlayer.gameObject);
                }
                else if (messageKeyCode == -2 && isClientReadyToReceiveData)
                {
                    //Debug.Log("Client: Receiving DATA_PACKET from server");


                    #region receiving_data_for_players
                    
                    int sizeOfPlayers = stream.ReadInt();
                    //Debug.Log("Client: READING sizeOfPlayers = " + sizeOfPlayers);

                    int currentId;
                    // updejtaj stanje na klijentu za svakog prisutnog igraca
                    Transport_Player currentPlayer;
                    //Debug.Log("Client: Iterating over players loop --------------");
                    for (int i = 0; i < sizeOfPlayers; ++i)
                    {
                        currentId = stream.ReadInt();
                        //Debug.Log("Client: READING currentId = " + currentId);

                        if ( !isHost )  // client code
                        {
                            if (!players.ContainsKey(currentId) )
                            {
                                //Debug.Log("players Dict doesnt contain " + currentId + ", spawning & adding");
                                // spawnat ce ga s random pozicijom i rotacijom, iako mu vec znamo koje treba, POTENCIJALNI IMPROVE: odma ovdje postaviti poziciju i rotaciju 
                                currentPlayer = transportSpawner.SpawnPlayerObject(currentId).GetComponent<Transport_Player>();
                                destroyed = false;
                                players.Add(currentId, currentPlayer);
                                playersList.Add(currentPlayer);


                                if (currentPlayer.uniqueId != myUniqueId)
                                {
                                    //Debug.Log("Client: Destroying player scripts for id = " + currentPlayer.uniqueId);
                                    currentPlayer.GetComponent<PlayerMove>().enabled = false;
                                    currentPlayer.GetComponentInChildren<PlayerShoot>().enabled = false;
                                    Destroy(currentPlayer.GetComponentInChildren<Camera>().gameObject);
                                    //Debug.Log("Client: player object with uniqueId=" + currentPlayer.uniqueId + " should be modified properly");
                                }
                                else
                                {
                                    localPlayer = currentPlayer;
                                    currentPlayer.gameObject.AddComponent<CursorLocking>();
                                    shouldSyncOnSpawn = true;
                                }

                            }                            
                        }
                        else   // host code (not happening anymore since host isnt syncing his own client, can probably delete)
                        {
                            if (!players.ContainsKey(currentId))
                            {
                                //Debug.Log("players Dict doesnt contain " + currentId + ", spawning & adding");
                                // nadji ga
                                currentPlayer = FindCurrentPlayer(currentId);
                                // ubaci u players dictionary
                                players.Add(currentId, currentPlayer);
                                playersList.Add(currentPlayer);
                                // ugasi sve sto treba( ako nije moj )
                                if (currentPlayer.uniqueId != myUniqueId)
                                {
                                    //Debug.Log("currentplayer.uniqueid = " + currentPlayer.uniqueId + "not equal to myUniqueId = " + myUniqueId);
                                    if ( currentPlayer.GetComponent<PlayerMove>() != null)
                                        currentPlayer.GetComponent<PlayerMove>().enabled = false;
                                    currentPlayer.GetComponentInChildren<PlayerShoot>().enabled = false;
                                    try
                                    {
                                        if (currentPlayer.GetComponentInChildren<Camera>() != null)
                                            Destroy(currentPlayer.GetComponentInChildren<Camera>().gameObject);
                                    }
                                    catch (NullReferenceException ex)
                                    {
                                        
                                    }                                    
                                }
                            }

                        }

                        players.TryGetValue(currentId, out currentPlayer);
                        //Debug.Log("Client: reading data for CURRENT ID = " + currentId);
                        //Debug.Log("Client: Going to overwrite pos & rot, currentId=" + currentId + "currentPlayer.id=" + currentPlayer.uniqueId + ", currentPlayer=" + currentPlayer);

                        if ( currentId != myUniqueId || ( (currentId == myUniqueId)  && shouldSyncOnSpawn ))  // if receiving server data for myself, don't sync position and rotation
                        {
                            //if ( myUniqueId == 2 && currentId == 2)
                                //Debug.Log("getting synced by server, SHOULDNT HAPPEN more than once");
                            if (shouldSyncOnSpawn)
                            {
                                //Debug.Log("Client: doing 1st sync for my id = " + myUniqueId);
                                shouldSyncOnSpawn = false;
                                someBool = true;
                            }

                            Transform currentPlayerTransform = currentPlayer.transform;
                            // Receive position data
                            float newX, newY, newZ;
                            newX = stream.ReadFloat();
                            newY = stream.ReadFloat();
                            newZ = stream.ReadFloat();
                            Vector3 newPos = new Vector3(newX, newY, newZ);
                            //Debug.Log("Client: READING position = " + newPos);

                            // Receive rotation data
                            newX = stream.ReadFloat();
                            newY = stream.ReadFloat();
                            newZ = stream.ReadFloat();
                            float newW = stream.ReadFloat();
                            Quaternion newQuat = new Quaternion(newX, newY, newZ, newW);
                            //Debug.Log("Client: READING rotation = " + newQuat);

                            currentPlayerTransform.SetPositionAndRotation(newPos, newQuat);
                            currentPlayer.SetUpdated();
                            //if (counter % 200 == 0 && currentId == 2)
                            //    Debug.Log("client reading his own data, SHOULDNT HAPPEN");
                            
                            if ( someBool )
                            {                                
                                isClientReadyToSendData = true;
                            }
                        }

                        // Receive health
                        currentPlayer.health = stream.ReadFloat();
                        currentPlayer.GetComponentInChildren<HealthBarScript>().transform.localScale = new Vector3(currentPlayer.health / 100f, 0.15f, 0.01f);
                        //Debug.Log("Client: READING health = " + currentPlayer.health);

                        //currentPlayer.SetUpdated();
                    }
                    //Debug.Log("Client: Done iterating over players loop ---------");
                    
                    #endregion

                    #region receiving_data_for_projectiles
                    
                    //Debug.Log("Client: receiving proj data");
                    int numberOfClients = stream.ReadInt(); // for iterating over all projectiles
                    //Debug.Log("Client: READING numberOfClients = " + numberOfClients);

                    for (int j = 0; j < numberOfClients; ++j)
                    {
                        int currentClientId = stream.ReadInt();
                        //Debug.Log("Client: READING current originating client id = " + currentClientId);

                        if (currentClientId == myUniqueId)  // ako prihvaca podatke za sebe, onda je poslano manje podataka (samo da se uniste)
                        {
                            //Debug.Log("Client: receiving proj data on originating client just info to destroy own projectiles");
                            int numberOfProjectilesToBeDestroyed = stream.ReadInt();
                            //Debug.Log("Client: READING amount of projectiles-to-be-destroyed on originating client = " + numberOfProjectilesToBeDestroyed);
                            Dictionary<int, Transport_Projectile> myProjectiles;
                            for (int k = 0; k < numberOfProjectilesToBeDestroyed; ++k)
                            {
                                int currentProjUniqId = stream.ReadInt();
                                //Debug.Log("Client: READING proj unique id on originating client = " + currentProjUniqId);

                                if (projectiles.TryGetValue(currentClientId, out myProjectiles))
                                {
                                    if ( myProjectiles.ContainsKey(currentProjUniqId) )
                                    {
                                        myProjectiles.TryGetValue(currentProjUniqId, out Transport_Projectile currentProj);
                                        Destroy(currentProj.gameObject);
                                        myProjectiles.Remove(currentProjUniqId);
                                    }
                                }
                                else
                                {
                                    projectiles.Add(currentClientId, new Dictionary<int, Transport_Projectile>());
                                }
                            }
                        }
                        else   // inace saljemo vise podataka
                        {
                            //Debug.Log("Client: receiving proj data on non-originating client");
                            // koliko projektila primiti, (za svakog originating clienta)
                            int numberOfProjectilesToReceive = stream.ReadInt();
                            //Debug.Log("Client: READING number of projectiles to receive (for current originating client) = " + numberOfProjectilesToReceive);

                            Dictionary<int, Transport_Projectile> projectilesOfRemoteOriginatingClient;
                            if (!projectiles.TryGetValue(currentClientId, out projectilesOfRemoteOriginatingClient))
                            {
                                projectilesOfRemoteOriginatingClient = new Dictionary<int, Transport_Projectile>();
                                projectiles.Add(currentClientId, projectilesOfRemoteOriginatingClient);
                            }

                            for (int k = 0; k < numberOfProjectilesToReceive; ++k)
                            {
                                int uqIdForProjectile = stream.ReadInt();
                                float PnewX, PnewY, PnewZ;
                                PnewX = stream.ReadFloat();
                                PnewY = stream.ReadFloat();
                                PnewZ = stream.ReadFloat();
                                Vector3 newPos = new Vector3(PnewX, PnewY, PnewZ);
                                //Debug.Log("Server: READING unique proj id (on originating client) = " + uqIdForProjectile);
                                //Debug.Log("Server: READING position = " + new Vector3(PnewX, PnewY, PnewZ));

                                Transport_Projectile currTransportProjectile;
                                // ako projektil ne postoji na lokalnom klijentu, spawnaj ga s dobivenim podacima
                                if ( !projectilesOfRemoteOriginatingClient.TryGetValue(uqIdForProjectile, out currTransportProjectile ) )   
                                {                                    
                                    GameObject newProj = transportSpawner.SpawnProjectile(newPos, new Vector3(0f,0f,0f), currentClientId);  // originating client postavljen ovdje
                                    currTransportProjectile = newProj.GetComponent<Transport_Projectile>();
                                    currTransportProjectile.uniqueProjectileId = uqIdForProjectile; // unique projectile id (za svakog originating klijenta) postavljen ovdje
                                    projectilesOfRemoteOriginatingClient.Add(uqIdForProjectile, currTransportProjectile);                                    
                                }
                                else
                                {
                                    currTransportProjectile.transform.position = new Vector3(PnewX, PnewY, PnewZ);
                                }
                                currTransportProjectile.SetUpdated();   // ako kreiran ili ažuriran, markiraj kao "updated" da ga se ne unisti
                            }
                        }
                    }
                    #endregion
                }
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client: Client got disconnected from server");
                m_Connection = default(NetworkConnection);
            }


        } // WHILE POP EVENT - END
        #endregion


        // TREBA UNISTITI SAMO ONE koji ne pripadaju lokalnom klijentu !!!
        // PRETPOSTAVKA: ako klijent nije dobio Update dataset za player objecta, taj player object je unisten ILI je njegov lokalni klijent disconnectan
        #region destrukcija_neazuriranih_objekata
        // unistenje igraca
        //DestroyNonUpdated_Players();    // ako nije tvoj vlastiti, i nije updated, unisti


        Dictionary<int, List<int>> projectilesForDestruction = new Dictionary<int, List<int>>();    // kljuc - client id, value - lista kljuceva projektila za unistenje na kraju
        Dictionary<int, Transport_Projectile> dict;
        // unistenje projektila
        foreach (var entry in projectiles)
        {
            if (entry.Key == myUniqueId)    // ako njegovi vlastiti projektili, skippaj korak
                continue;
            dict = entry.Value;
            if (dict == null) continue;
            foreach (var projectile in dict)
            {
                if (!projectile.Value.updated)
                {
                    //dict.Remove(projectile.Key);
                    if (!projectilesForDestruction.TryGetValue(entry.Key, out List<int> listica))
                    {
                        listica = new List<int>();
                        projectilesForDestruction.Add(entry.Key, listica);
                    }
                    listica.Add(projectile.Key);
                }
                else
                    projectile.Value.updated = false;
            }
        }

        foreach (var entry in projectilesForDestruction)
        {
            int clientId = entry.Key;
            List<int> currentList = entry.Value;
            projectiles.TryGetValue(clientId, out dict);
            foreach (int projectileId in currentList)
            {
                dict.TryGetValue(projectileId, out Transport_Projectile projToDestroy);
                Destroy(projToDestroy.gameObject);
                dict.Remove(projectileId);
            }
        }
        #endregion
        
        #region sending(local client data to server)
        if ( isClientReadyToSendData && !isHost && !destroyed )
        {
            if ( clientSendingFirstTime )
            {
                //Debug.Log("Client: sending own data to server");
                clientSendingFirstTime = false;
            }
            m_Driver.BeginSend(m_Connection, out writer);

            // write message keycode = -3
            int client_data_code = -3;
            writer.WriteInt(client_data_code);

            // write my identity (da bude rigidnije i brze)
            writer.WriteInt(myUniqueId);

            #region syncing_player_transform
            Vector3 myPosition = localPlayer.transform.position;
            // write position
            writer.WriteFloat(myPosition.x);
            writer.WriteFloat(myPosition.y);
            writer.WriteFloat(myPosition.z);

            //if (counter % 200 == 0)
            //    Debug.Log("Sending to server position: " + myPosition);

            Quaternion myRotation = localPlayer.transform.rotation;
            // write rotation
            writer.WriteFloat(myRotation.x);
            writer.WriteFloat(myRotation.y);
            writer.WriteFloat(myRotation.z);
            writer.WriteFloat(myRotation.w);
            #endregion

            #region syncing_projectiles
            
            Dictionary<int, Transport_Projectile> myProjectiles;
            if (!projectiles.TryGetValue(myUniqueId, out myProjectiles))
            {
                myProjectiles = new Dictionary<int, Transport_Projectile>();
                projectiles.Add(myUniqueId, myProjectiles);
            }
                
            writer.WriteInt(myProjectiles.Count);
            // writing projectile state
            foreach ( var p in myProjectiles )
            {
                writer.WriteInt(p.Key);
                writer.WriteFloat(p.Value.transform.position.x);
                writer.WriteFloat(p.Value.transform.position.y);
                writer.WriteFloat(p.Value.transform.position.z);
            }
            
            #endregion
            m_Driver.EndSend(writer);
        }     
        
        #endregion

        //keyPressedThisUpdate = false;
    } // UPDATE - END

    // POTENCIJALNI PROBLEM: ako u SAMO 1 Update ciklusu klijent nije primio data za nekog playera, pretpostavlja da je unisten na serveru (sto ako se paket izgubio, kasni...)
    void DestroyNonUpdated_Players()
    {
        List<int> playersToDestroy = new List<int>();
        // find those for destruction
        foreach (var entry in players)
        {
            if (entry.Key == myUniqueId) continue;
            if (!entry.Value.updated)
            {
                playersToDestroy.Add(entry.Key);
            }
            else
            {
                entry.Value.updated = false;
            }
        }

        foreach ( int playerKey in playersToDestroy )
        {
            if ( players.TryGetValue(playerKey, out Transport_Player playerToDestroy))
            {
                players.Remove(playerKey);
                playersList.Remove(playerToDestroy);
                Destroy(playerToDestroy.gameObject);
            }
            else
            {
                Debug.LogError("didnt find player key, should be able to find it");
            }
            
        }
    }

    //void ResetUpdated_Players()
    //{
    //    foreach (var p in players.Values)
    //        p.updated = false;
    //}

    Transport_Player FindCurrentPlayer(int uniqueId)
    {
        Transport_Player[] trans_players = FindObjectsOfType<Transport_Player>();
        foreach ( var p in trans_players )
        {
            if (p.uniqueId == uniqueId)
                return p;
        }
        Debug.LogError("PROBLEM: on client, didnt find proper player object");
        return null;
    }
}

