using UnityEngine;
using Unity.Networking.Transport;

public class CheckForTransportServer : MonoBehaviour
{
    private NetworkDriver m_Driver;
    private NetworkConnection m_Connection;
    private TransportSpawner transportSpawner;

    private bool driverReady = false;

    void Start()
    {
        transportSpawner = GetComponent<TransportSpawner>();

        m_Driver = NetworkDriver.Create();
        m_Connection = default(NetworkConnection);
        var endpoint = NetworkEndPoint.LoopbackIpv4;
        endpoint.Port = 9000;
        driverReady = true;
        m_Connection = m_Driver.Connect(endpoint);
        //Debug.Log("after m_Driver.Connect, ConnectionState = " + m_Driver.GetConnectionState(m_Connection));
        //Debug.Log("Checking if server exists...");
        Invoke("CheckIfConnectedAfterTimeout", 2f);
    }

    private void CheckIfConnectedAfterTimeout() {

        Debug.Log(m_Connection.GetState(m_Driver));

        if ( m_Driver.GetConnectionState(m_Connection).Equals(NetworkConnection.State.Connecting) )
        {
            m_Connection.Disconnect(m_Driver);
            // assume server doesn't exist yet
            m_Connection = default(NetworkConnection);
            driverReady = false;
            m_Driver.Dispose();
            //Debug.Log("Server doesn't exist, starting host.");
            transportSpawner.StartHost();
        }
        else if ( m_Driver.GetConnectionState(m_Connection).Equals(NetworkConnection.State.Connected) )
        {
            m_Connection.Disconnect(m_Driver);
            // server already exists
            m_Connection = default(NetworkConnection);
            driverReady = false;
            m_Driver.Dispose();
            //Debug.Log("Server exists, starting client.");
            transportSpawner.StartClient();
        }


        //enabled = false;
    }

    DataStreamReader stream;
    NetworkEvent.Type cmd;
    private void Update()
    {
        if ( m_Driver.IsCreated && driverReady )
        {
            m_Driver.ScheduleUpdate().Complete();

            if (!m_Connection.IsCreated)
            {
                return;
            }
        }
    }

    public void OnDestroy()
    {
        if (m_Driver.IsCreated)
            m_Driver.Dispose();
    }        
}
