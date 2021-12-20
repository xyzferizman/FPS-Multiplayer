using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PUN_ProjectileOwnershipTransfer : MonoBehaviourPun, IPunOwnershipCallbacks//, IPunObservable
{
    private void Awake()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if ( stream.IsWriting) {
    //        stream.SendNext(transform.position);
    //    }
    //    else if ( stream.IsReading )
    //    {
    //        transform.position = (Vector3)stream.ReceiveNext();
    //    }
    //}

    public void OnOwnershipRequest(PhotonView targetView, Player requestingPlayer)
    {
        Debug.Log("OnOwnershipRequest entered, targetView.owner = " + targetView.OwnerActorNr + ", requestingPlayer = " + requestingPlayer.ActorNumber);
        if (targetView != photonView) return;
        
        Debug.Log("projectile: on ownership request.");
        targetView.TransferOwnership(requestingPlayer);
    }

    public void OnOwnershipTransfered(PhotonView targetView, Player previousOwner)
    {
        Debug.Log("OnOwnershipTransfered entered, targetView.owner = " + targetView.OwnerActorNr + ", previousOwner = " + previousOwner.ActorNumber);
        if (targetView != photonView) return;

        if ( PhotonNetwork.IsMasterClient )
        {
            Debug.Log("OnOwnershipTransfered, destroying photonView");
            // destroy this network object here
            PhotonNetwork.Destroy(photonView);
        }        
    }

    public void OnOwnershipTransferFailed(PhotonView targetView, Player senderOfFailedRequest)
    {
        Debug.Log("projectile: on ownership transfer failed.");
    }

    private void OnDestroy()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    
}
