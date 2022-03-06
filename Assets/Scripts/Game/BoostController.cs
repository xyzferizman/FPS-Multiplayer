using UnityEngine;
using System;
using System.Collections.Generic;

public class BoostController : MonoBehaviour
{
    private Boost newBoost;

    internal PlayerMove playerMove;
    internal PlayerLook playerLook;
    internal PlayerShoot playerShoot;

    internal Dictionary<BoostType, uint> boostDict; 

    private void Start()
    {
        boostDict = new Dictionary<BoostType, uint>();

        playerMove = GetComponent<PlayerMove>();        
        playerLook = GetComponentInChildren<PlayerLook>();
        playerShoot = GetComponentInChildren<PlayerShoot>();

        // inicijaliziraj dictionary
        foreach ( BoostType bt in Enum.GetValues(typeof(BoostType)))
        {
            boostDict.Add(bt, 0);
        }
    }

    public void GetBoost(BoostType bt)
    {
        if ( bt.Equals(BoostType.Speed) )
        {
            //Debug.Log("Speed Boost picked up.");
            
            newBoost = gameObject.AddComponent<SpeedBoost>();
        }
        else if ( bt.Equals(BoostType.Damage) )
        {
            //Debug.Log("Damage Boost picked up.");

            newBoost = gameObject.AddComponent<DamageBoost>();
        }
        else if ( bt.Equals(BoostType.Walls) )
        {
            //Debug.Log("Walls Boost picked up.");
            
            newBoost = gameObject.AddComponent<WallsBoost>();
        }
        else
            Debug.LogError("Custom Error: unfamiliar boost type = " + bt.ToString());

        newBoost.SetUnityRelatedStuff();    // jer mi neda GetComponent u C# konstruktoru
        newBoost.ApplyBoost();
    }
}

