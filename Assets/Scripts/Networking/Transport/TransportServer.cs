using UnityEngine;
using Unity.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using System;

public class TransportServer : MonoBehaviour
{
    public NetworkDriver m_Driver;
    private NativeList<NetworkConnection> m_Connections;
    private TransportSpawner transportSpawner;   
    private TransportClient myClient;

    public Dictionary<int, Transport_Player> players;  // network connection unique id, relevant player data ( to be disributed to clients )
    public List<Transport_Player> playersList;
    private Dictionary<NetworkConnection, int> connPlayer;
    private Dictionary<NetworkConnection, int> syncOnSpawn;

    internal Dictionary<int, Dictionary<int, Transport_Projectile>> projectiles; // outer int, clientId on server, inner int, projectile id for specific client
    internal Dictionary<int, List<int>> notifyClientsToDestroyProjectiles;

    internal List<int> shouldBeDestroyed_Players;

    Dictionary<NetworkConnection, int> readyClients;

    private int playerIndex = 1;
    private int projectileIndex = 1;
    private int counter = 1;

    public Transform _currentPlayerTransform;

    private bool clientReady = false;
    private bool firstTimeClientDataReceived = true;
    bool keyPressedThisUpdate = false;

    public Transport_Player currentTransportPlayer;

    NetworkConnection currentConnection;
    
    DataStreamReader stream;
    

    void Start()
    {
        shouldBeDestroyed_Players = new List<int>();
        transportSpawner = GetComponent<TransportSpawner>();

        NetworkConfigParameter param = new NetworkConfigParameter();
        param.disconnectTimeoutMS = 1000000; 

        m_Driver = NetworkDriver.Create(param);
        var endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = 9000;

        if (m_Driver.Bind(endpoint) != 0)
            Debug.Log("Server: Failed to bind to port " + endpoint.Port);
        else
            m_Driver.Listen();

        #region data_structs_init
        m_Connections = new NativeList<NetworkConnection>(16, Allocator.Persistent);
        players = new Dictionary<int, Transport_Player>();
        playersList = new List<Transport_Player>();
        projectiles = new Dictionary<int, Dictionary<int, Transport_Projectile>>();
        notifyClientsToDestroyProjectiles = new Dictionary<int, List<int>>();
        connPlayer = new Dictionary<NetworkConnection, int>();
        syncOnSpawn = new Dictionary<NetworkConnection, int>();

        readyClients = new Dictionary<NetworkConnection, int>();
        #endregion

        myClient = transportSpawner.HostReadyCallback();
    }
        
    private Transport_Player SpawnPlayerForNewClient(NetworkConnection c)
    {
        //Debug.Log("Server: SpawnPlayerForNewClient, starting");
        int currPlayerIndex = playerIndex++;
        GameObject newPlayerObj = transportSpawner.SpawnPlayerObject(currPlayerIndex); // zasad imati ovu metodu
        Transport_Player transp_player = newPlayerObj.GetComponent<Transport_Player>();
        if ( FindObjectsOfType<Transport_Player>().Length == 1 )    // onda je ovo prvi spawnani player, i onda smo na hostu
            myClient.SetHostObject(transp_player);  // myClient je razlicit od null samo na hostu
                
        players.Add(currPlayerIndex, transp_player);
        playersList.Add(transp_player);

        if ( !transp_player.Equals(myClient.localPlayer) )
        {
            //Debug.Log("Server: destroying player scripts for id = " + transp_player.uniqueId );
            transp_player.GetComponent<PlayerMove>().enabled = false;
            transp_player.GetComponentInChildren<PlayerShoot>().enabled = false;
            //Destroy(transp_player.GetComponent<CharacterController>());
            Destroy(transp_player.GetComponentInChildren<Camera>().gameObject);
            Destroy(transp_player.GetComponent<PlayerMove>());
        }

        //myClient.players.Add(currPlayerIndex, transp_player);
        
        
        //Debug.Log("Returning from SpawnPlayerForNewClient, currPlayerIndex = " + currPlayerIndex + ", Transport_Player.uniqueId = " + transp_player.uniqueId);
        return transp_player;
    }

    // Server update loop
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.G))
        //    keyPressedThisUpdate = true;

        m_Driver.ScheduleUpdate().Complete();

        // Clean up connections
        for (int i = 0; i < m_Connections.Length; i++)
        {
            currentConnection = m_Connections[i];

            if (!currentConnection.IsCreated)
            {
                m_Connections.RemoveAt(i);
                --i;
            }
        }

        // Accept new connections
        NetworkConnection c;
        while ((c = m_Driver.Accept()) != default(NetworkConnection))
        {
            m_Connections.Add(c);
            //Debug.Log("Server: Accepted a connection");
        }

        // FOR EACH connection, refresh game-state, check messages from that client        
        #region for_loop_connections
        DataStreamWriter writer;
        //if ( keyPressedThisUpdate )
        //    Debug.Log("Server: For loop over all connections:");
        for (int i = 0; i < m_Connections.Length; i++)
        {
            currentConnection = m_Connections[i];
            if (!currentConnection.IsCreated)
                continue;

            int someInt;
            connPlayer.TryGetValue(currentConnection, out someInt);
            //if ( keyPressedThisUpdate )
            //    Debug.Log("Server: Current connection for client " + someInt);

            #region receiving
            // querying the driver for events that might have happened since the last update
            NetworkEvent.Type cmd;
            while ((cmd = m_Driver.PopEventForConnection(currentConnection, out stream)) != NetworkEvent.Type.Empty)
            {
                if (cmd == NetworkEvent.Type.Data)
                {
                    int code = stream.ReadInt();  // read keycode
                    //if ( code != -3)
                    //    Debug.Log("Server: Reading message code=" + code + " from stream.");
                    if (code == -1) // spawn new player object
                    {
                        #region receiving_spawn_request_from_client
                        Transport_Player newTransportPlayer = SpawnPlayerForNewClient(currentConnection);
                        //Debug.Log("Server, sending uniqueId to client, newTransportPlayer.uniqueId = " + newTransportPlayer.uniqueId);
                        m_Driver.BeginSend(currentConnection, out writer);
                        int _keycode = -1;
                        //Debug.Log("Server: Writing message code=" + _keycode + " to stream");
                        writer.WriteInt(_keycode);  // 1 - KEYCODE for requesting player object spawn, odgovor klijentu
                        //Debug.Log("Server: Writing player's unique id=" + newTransportPlayer.uniqueId + " to stream");
                        writer.WriteInt(newTransportPlayer.uniqueId);  // kada klijent primi ovo, znat ce koji id mu je dodijeljen                        
                        m_Driver.EndSend(writer);
                        connPlayer.Add(currentConnection, newTransportPlayer.uniqueId);
                        syncOnSpawn.Add(currentConnection, newTransportPlayer.uniqueId);
                        readyClients.Add(currentConnection, newTransportPlayer.uniqueId);
                        #endregion
                    }
                    else if ( code == -3 && !OwnConnection(currentConnection, myClient.myUniqueId))   // receive position and rotation data from client
                    {
                        #region receiving_data_from_client
                        
                        counter++;
                        Dictionary<int, Transport_Projectile> projDict;
                        Transport_Projectile transp_projectile;
                        // SHOULD RECEIVE HERE, NOT SEND, just copypasted
                        //m_Driver.BeginSend(m_Connection, out writer);                        
                        int clientIdentity = stream.ReadInt();

                        if (firstTimeClientDataReceived)
                        {
                            //Debug.Log("Server: receiving client data from client " + clientIdentity);
                            firstTimeClientDataReceived = false;
                        }


                        //bool isItFound = players.TryGetValue(clientIdentity, out currentTransportPlayer);
                        //if (!isItFound)
                        //    Debug.LogError("Server: WARNING: client sent his id = " +clientIdentity+", but can't find it in players Dict");

                        currentTransportPlayer = FindPlayerObjForId(clientIdentity);

                        // read position
                        float _newX, _newY, _newZ;
                        _newX = stream.ReadFloat();
                        _newY = stream.ReadFloat();
                        _newZ = stream.ReadFloat();
                        Vector3 newClientPosition = new Vector3(_newX, _newY, _newZ);
                        //newClientPosition.x = stream.ReadFloat();
                        //newClientPosition.y = stream.ReadFloat();
                        //newClientPosition.z = stream.ReadFloat();

                        // read rotation
                        float newX, newY, newZ, newW;
                        newX = stream.ReadFloat();
                        newY = stream.ReadFloat();
                        newZ = stream.ReadFloat();
                        newW = stream.ReadFloat();
                        Quaternion myRotation = new Quaternion(newX, newY, newZ, newW);
                        //currentTransportPlayer.transform.rotation.Set(newX, newY, newZ, newW);

                        //Vector3 oldPos = currentTransportPlayer.transform.position;

                        //currentTransportPlayer.transform.SetPositionAndRotation(newClientPosition, myRotation);
                        //currentTransportPlayer.transform.position = newClientPosition;
                        //currentTransportPlayer.transform.rotation = myRotation;

                        _currentPlayerTransform = currentTransportPlayer.gameObject.transform;

                        //currPlayTransform.position = newClientPosition;
                        //currPlayTransform.rotation = myRotation;

                        _currentPlayerTransform.SetPositionAndRotation(newClientPosition, myRotation);
                        //currentTransportPlayer.GetComponent<CharacterController>().Move(newClientPosition - oldPos);

                        #region receiving_client_projectile_data
                        
                        int numberOfProjectilesForClient = stream.ReadInt();
                        for ( int j = 0; j < numberOfProjectilesForClient; ++j)
                        {
                            int currProjectileId = stream.ReadInt();
                            float projX = stream.ReadFloat();
                            float projY = stream.ReadFloat();
                            float projZ = stream.ReadFloat();
                            if (!projectiles.TryGetValue(clientIdentity, out projDict))
                            {
                                projDict = new Dictionary<int, Transport_Projectile>();
                                projectiles.Add(clientIdentity, projDict);
                                //Debug.LogError("PROBLEM: Server received client data for projectiles, can't get clientId in projectiles outer Dict");
                            }
                            
                            if (!projDict.TryGetValue(currProjectileId, out transp_projectile)) {
                                // ako ovo false, znaci da ne postoji, znaci addati ga
                                transp_projectile = transportSpawner.SpawnProjectile(new Vector3(projX, projY, projZ), new Vector3(0f,0f,0f), clientIdentity).GetComponent<Transport_Projectile>();
                                transp_projectile.originatingClientId = clientIdentity;
                                transp_projectile.uniqueProjectileId = currProjectileId;
                                projDict.Add(transp_projectile.uniqueProjectileId, transp_projectile);
                            }
                            else
                            {   // ako ga je pronasao, onda postoji, pa ga samo updejtaj
                                transp_projectile.transform.position = new Vector3(projX, projY, projZ);
                            }

                        }
                        
                        #endregion

                        //m_Driver.EndSend(writer);
                        #endregion
                    }
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    m_Connections[i] = default(NetworkConnection);
                    //currentConnection = default(NetworkConnection);
                    readyClients.Remove(currentConnection);
                    connPlayer.Remove(currentConnection);
                }
            }

            #endregion

            // ako kojim slucajem klijent na konekciji nije ready, preskociti
            if (!readyClients.ContainsKey(currentConnection))
                continue;


            // OVO DOLJE, COMMENTED FOR DEV PURPOSES

            // netreba sinkronizirati vlastitog lokalnog klijenta
            if (OwnConnection(currentConnection, myClient.myUniqueId))
                continue;

            int currClient;
            connPlayer.TryGetValue(currentConnection, out currClient);
            //Debug.Log("Server: Writing to connection of client " + currClient);

            #region sending
            if ( true )
            {
                m_Driver.BeginSend(currentConnection, out writer);
                int keycode = -2;
                writer.WriteInt(keycode);    // write message keycode to stream
                //Debug.Log("Server: WRITING message of keycode = " + keycode);

                Vector3 _position;
                Quaternion _quaternion;
                float _health = -1;

                #region sending_data_for_players                
                writer.WriteInt(players.Count);
                //Debug.Log("Server. WRITING player count = " + players.Count);
                //petlja koja salje podatke o igracima
                //Debug.Log("Server: Iterating over players");
                foreach (var entry in players)
                {
                    //if ( zelim slati igracu cija je konekcija ) continue; (ne salji poziciju i rotaciju, salji samo health), PROBLEM, kako ce klijent znati prepoznati ovo "modificirano" slanje?
                    _position = entry.Value.transform.position;
                    _quaternion = entry.Value.transform.rotation;
                    _health = entry.Value.health;

                        // Send player object data to clients (all but itself/host)   
                        // Send client id
                    //Debug.Log("Server: WRITING client id, " + entry.Key);
                    writer.WriteInt(entry.Key);

                    //ne slati klijentu njegovu vlastitu poziciju i rotaciju, samo health
                    //osim 1-time sync nakon spawna
                    if (!OwnConnection(currentConnection, entry.Key) ||
                        (OwnConnection(currentConnection, entry.Key) && syncOnSpawn.ContainsKey(currentConnection)))
                    {
                        if ((OwnConnection(currentConnection, entry.Key) && syncOnSpawn.ContainsKey(currentConnection)))
                        {
                            syncOnSpawn.Remove(currentConnection);
                            //Debug.Log("Server: doing 1st sync for id = " + entry.Key);
                        }

                        if (OwnConnection(currentConnection, entry.Key) && entry.Key == 2)
                            //Debug.Log("Sending transform data to own client 2, SHOULDNT HAPPEN");

                        //Send position data
                        writer.WriteFloat(_position.x);
                        writer.WriteFloat(_position.y);
                        writer.WriteFloat(_position.z);
                        //Debug.Log("Server: WRITING position = " + new Vector3(_position.x, _position.y, _position.z));

                        //Send rotation data
                        writer.WriteFloat(_quaternion.x);
                        writer.WriteFloat(_quaternion.y);
                        writer.WriteFloat(_quaternion.z);
                        writer.WriteFloat(_quaternion.w);
                        //Debug.Log("Server: WRITING rotation = " + new Vector4(_quaternion.x, _quaternion.y, _quaternion.z, _quaternion.w));
                    }

                    //Send health
                    writer.WriteFloat(_health);
                    //Debug.Log("Server: WRITING health = " + _health);
                }                
                #endregion

                #region sending_data_for_projectiles
                
                
                int numberOfClients = projectiles.Count;
                writer.WriteInt(numberOfClients);
                //Debug.Log("Server: WRITING number of clients = " + numberOfClients);

                //Debug.Log("Server: iterating over projectiles dictionary.");
                // petlja koja salje podatke o projektilima
                foreach (var dataForClient in projectiles)    // iterating over dictionaries for each client
                {
                    writer.WriteInt(dataForClient.Key);    // upisi client id ciji su ovi projektili
                    //Debug.Log("Server: WRITING for originating client id = " + dataForClient.Key);
                    if (OwnConnection(currentConnection, dataForClient.Key)) // ako saljemo na konekciju tog vlastitog klijenta, samo info za lokalno unistenje na klijentu
                    {
                        //Debug.Log("Server: sending proj data to originating client, just info to destroy own projectiles");
                        // onda samo posalji info o unistenjima
                        List<int> listOfDestroyedProjectilesForClient;
                        if (!notifyClientsToDestroyProjectiles.TryGetValue(dataForClient.Key, out listOfDestroyedProjectilesForClient))
                        {
                            listOfDestroyedProjectilesForClient = new List<int>();
                            notifyClientsToDestroyProjectiles.Add(dataForClient.Key, listOfDestroyedProjectilesForClient);
                        }
                        
                        writer.WriteInt(listOfDestroyedProjectilesForClient.Count); // amount of projectiles-to-be-destroyed on the originating client
                        //Debug.Log("Server: WRITING amount of projectiles-to-be-destroyed on originating client = " + listOfDestroyedProjectilesForClient.Count);
                        foreach (int projUniqId in listOfDestroyedProjectilesForClient)
                        {
                            writer.WriteInt(projUniqId);    // kad originating client primi ove id-eve, zna da mora unistiti i azurirati svoje projektile
                            //Debug.Log("Server: WRITING proj unique id on originating client = " + projUniqId);
                        }
                        listOfDestroyedProjectilesForClient.Clear();
                    }
                    else   // inace saljemo vise podataka
                    {
                        //Debug.Log("Server: sending proj data to non-originating client");
                        writer.WriteInt(dataForClient.Value.Count);    // koliko projektila poslati, za originating clienta
                        //Debug.Log("Server: WRITING number of projectiles to send (for current originating client) = " + dataForClient.Value.Count);
                        foreach (var p in dataForClient.Value)
                        {
                            writer.WriteInt(p.Key);  // unique id za projektil (na svom vlastitom klijentu)
                            writer.WriteFloat(p.Value.transform.position.x);
                            writer.WriteFloat(p.Value.transform.position.y);
                            writer.WriteFloat(p.Value.transform.position.z);
                            //Debug.Log("Server: WRITING unique proj id (on originating client) = " + p.Key);
                            //Debug.Log("Server: WRITING position = " + p.Value.transform.position);
                        }
                    }
                }
                
                #endregion

                m_Driver.EndSend(writer);
                //Debug.Log("Server: END sending for connection[" + i + "] ---------");
                
            }
            #endregion

        }   // FOR LOOP, CONNECTIONS
        #endregion

        #region destroying_players
        // destroying players-to-be-destroyed in this frame
        foreach (var playerKey in shouldBeDestroyed_Players)
        {
            if (playerKey != myClient.myUniqueId)
            {   // ako nije hostov igrac
                PlayerDestroyed_NotifyClient(playerKey);
                if (!players.TryGetValue(playerKey, out Transport_Player playerToBeDestroyed))
                    Debug.LogError("didnt find player, should be able to find it");

                players.Remove(playerKey);
                playersList.Remove(playerToBeDestroyed);

                myClient.players.Remove(playerKey);
                myClient.playersList.Remove(playerToBeDestroyed);

                Destroy(playerToBeDestroyed.gameObject);
                SendDestroyedPlayerInfoToAllBut(playerKey);
                shouldBeDestroyed_Players.Remove(playerKey);
            }

            // ako je, samo Destroy (tamo gore u kodu) i sam ce se unistiti na ostalim klijentima, zapoceti proceduru respawna
            else    // ako je unisten lokalni igrac
            {
                // kickstart respawn procedure
                if (!players.TryGetValue(playerKey, out Transport_Player playerToBeDestroyed))
                    Debug.LogError("didnt find player, should be able to find it");

                myClient.respawner.InitializeRespawnCountdown();

                players.Remove(playerKey);
                playersList.Remove(playerToBeDestroyed);

                myClient.players.Remove(playerKey);
                myClient.playersList.Remove(playerToBeDestroyed);

                Destroy(playerToBeDestroyed.gameObject);
                SendDestroyedPlayerInfoToAllBut(playerKey);
                shouldBeDestroyed_Players.Remove(playerKey);
            }
        }
        #endregion

        #region destroying_projectiles
        // 
        #endregion 
    }   // UPDATE METHOD

    void SendDestroyedPlayerInfoToAllBut(int clientId)
    {
        foreach ( var conn in m_Connections )
        {
            if (!connPlayer.TryGetValue(conn, out int someclientId))
                Debug.LogError("didnt find entry in connPlayer, shouldve found it");

            if (someclientId == clientId) continue; // ne slati vlasniku
            if (OwnConnection(conn, myClient.myUniqueId)) continue; // ne slati sebi samom

            m_Driver.BeginSend(conn, out DataStreamWriter writer);
            writer.WriteInt(-5);    // kod poruke za unistenje remote kopije igraca
            writer.WriteInt(clientId);    // podatak kojeg igraca unistiti
            m_Driver.EndSend(writer);

        }
    }

    // kad smo obradili sve konekcije u ovoj Update iteraciji, mozemo izbaciti unistene objekte
    // POTENCIJAL ZA IMPROVEMENT: ako Unity Transport aplikacijski sloj koristi UDP transportni, moguci gubici poruka koji mogu srusiti program
    // napraviti klijenta da je robustan za te pogreske
    // ako je bas potrebna sigurnost primitka, implementirati dodatni omotac oko slanja (nekakve potvrde primitka itd)        
    // UPDATE METHOD

    bool OwnConnection(NetworkConnection c, int id)
    {// TODO
        int retVal = -123;
        if (!connPlayer.TryGetValue(c, out retVal))
            Debug.LogError("PROBLEM IN OwnConnection, couldnt get value for given connection");
        if (id == retVal) return true;
        return false;
    }

    Transport_Player FindPlayerObjForId(int id)
    {
        Transport_Player[] trans_players = FindObjectsOfType<Transport_Player>();
        foreach (var p in trans_players)
        {
            if (p.uniqueId == id)
                return p;
        }
        Debug.LogError("PROBLEM: on server, didnt find proper player object");
        return null;
    }

    //void DestroyNonUpdated()
    //{
    //    foreach (var entry in players)
    //    {
    //        if (!entry.Value.updated)
    //        {
    //            Destroy(entry.Value);
    //            players.Remove(entry.Key);
    //        }
    //    }
    //}

    void ResetUpdated()
    {
        foreach (var p in players.Values)
            p.updated = false;
    }

    private void OnDestroy()
    {
        if (m_Driver.IsCreated)
            m_Driver.Dispose();
        if (m_Connections.IsCreated)
            m_Connections.Dispose();
    }

    internal void PlayerDestroyed_NotifyClient(int uniqueClientId)
    {
        NetworkConnection con1 = default(NetworkConnection);
        foreach ( var entry in connPlayer )
        {
            if ( entry.Value == uniqueClientId )
            {
                con1 = entry.Key;
                break;
            }                
        }

        m_Driver.BeginSend(con1, out DataStreamWriter writer);
        writer.WriteInt(-4);    // unisti se, zapocni respawn proceduru
        m_Driver.EndSend(writer);
    }
}
