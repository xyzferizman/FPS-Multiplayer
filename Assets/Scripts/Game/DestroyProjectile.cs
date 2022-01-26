using UnityEngine;
using MLAPI;
using Photon.Pun;

public class DestroyProjectile : NetworkBehaviour
{
    string netType;
    PhotonView myPhotonView;

    // TRENUTNO POSTAVLJENO ZA PUN2: lokalni klijent inicira destrukciju svog projektila, ne Master Client

    private void Awake()
    {
        netType = PlayerPrefs.GetString("networkType");
        if (netType == "pun_v2")
            myPhotonView = GetComponent<PhotonView>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if ( collision.collider.tag == "Gun" )
        //    Debug.Log("collided with gun");

        if ( netType.Equals("mlapi") )
        {
            if (IsServer)
            {
                Destroy(gameObject);
            }
        }
        // preselio unistavanje projektila u Photonu na player objecta koji je pogodjen zbog problema sinkronizacije 
        // (nije se registrirala kolizija na playeru, jer je owner projektila vec unistio remote projektil sa PhotonNetwork.Destroy, pa se lokalno na hittanom playeru nije registrirala kolizija
        // vlasnik projektila i dalje inicira unistenje kad projektil izadje van arene/kroz "plafon"

        else if (netType.Equals("pun_v2"))
        {
            if (collision.collider.tag != "Player" && myPhotonView.IsMine)
                PhotonNetwork.Destroy(myPhotonView);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ( netType.Equals("mlapi") )
        {
            if (IsServer)
            {
                Invoke("DestroyDelayed", 2f);
            }
        }
        else if ( netType.Equals("pun_v2") )
        {
            if ( myPhotonView.IsMine )
                Invoke("DestroyDelayed", 2f);
        }
    }

    private void DestroyDelayed()
    {
        if (netType == "mlapi")
            Destroy(gameObject);

        else if (netType == "pun_v2" && myPhotonView.IsMine)
            PhotonNetwork.Destroy(myPhotonView);
    }


}
