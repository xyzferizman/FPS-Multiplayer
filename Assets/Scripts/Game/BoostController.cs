using UnityEngine;
using System.Collections.Generic;

public class BoostController : MonoBehaviour
{
    private PlayerShoot playerShoot;
    private PlayerMove playerMove;
    private PlayerLook playerLook;

    private Dictionary<BoostType, int> boostDict = new Dictionary<BoostType, int>();

    // mozda ove varijable upakirati u nekakav "Boost" tip podatka, pa onda specijalizacija u "SpeedBoost", "WallBoost" itd.
    public float speedBoostFactor = 1.5f;
    public float speedBoostDuration = 3f;

    private void Start()
    {
        playerShoot = GetComponent<PlayerShoot>();
        playerMove = GetComponent<PlayerMove>();
        playerLook = GetComponent<PlayerLook>();

        boostDict.Add(BoostType.Speed, 0);
        boostDict.Add(BoostType.Damage, 0);
        boostDict.Add(BoostType.Walls, 0);
    }

    public void GetBoost(BoostType bt)
    {
        // TODO
        if ( bt.Equals(BoostType.Speed) )
        {
            Boost newSpeedBoost = new SpeedBoost();
            //GetComponent<PlayerMove>().GetSpeedBoost();

            // ako nije stackable i vec je aktivan, izadji
            if (!newSpeedBoost.isStackable && boostDict[BoostType.Speed] > 0)
                return;

            //GiveSpeedBoost();
            boostDict[BoostType.Speed]++;

            //playerMove.movementSpeed *= speedBoostFactor;
            //Debug.Log("Speed boost added, speed is now " + playerMove.movementSpeed);
            // RemoveBoost nakon x sekundi
        }
        else if ( bt.Equals(BoostType.Damage) )
        {
            //GetComponent<PlayerShoot>().GetDamageBoost();            
        }
        else if ( bt.Equals(BoostType.Walls) )
        {
            //GetComponent<PlayerMove>().GetWallBoost();
        }
    }

    // SPEED BOOST
    private void GetSpeedBoost()
    {

    }
}

