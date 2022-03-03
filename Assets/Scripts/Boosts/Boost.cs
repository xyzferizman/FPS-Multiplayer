using UnityEngine;
using System.Collections.Generic;

public abstract class Boost : MonoBehaviour
{
    internal bool isStackable;
    internal bool isRefreshable;    // dali pickup istog boosta, dok je isti aktivan, "overwrite-a" stari boost i efektivno ga produzuje ili ne?
    internal float duration;
    
    private protected PlayerMove playerMove;
    private protected PlayerLook playerLook;
    private protected PlayerShoot playerShoot;
    private protected BoostController boostController;

    private protected Dictionary<BoostType, uint> boostDict;
    private protected BoostType myBoostType;

    public Boost()
    {

    }

    internal virtual void SetUnityRelatedStuff()
    {
        Debug.Log("Boost, SetUnityRelatedStuff");
                
        boostController = GetComponent<BoostController>();

        playerMove = boostController.playerMove;
        playerShoot = boostController.playerShoot;
        playerLook = boostController.playerLook;

        boostDict = boostController.boostDict;
    }

    internal abstract void ApplyBoost();
    protected abstract void RemoveBoost();
}


