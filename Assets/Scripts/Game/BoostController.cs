using UnityEngine;
using System.Collections.Generic;

public class BoostController : MonoBehaviour
{
    private PlayerShoot playerShoot;
    private PlayerMove playerMove;
    private PlayerLook playerLook;

    private Dictionary<Boost, int> boostDict = new Dictionary<Boost, int>();

    // mozda ove varijable upakirati u nekakav "Boost" tip podatka, pa onda specijalizacija u "SpeedBoost", "WallBoost" itd.
    public float speedBoostFactor = 1.5f;
    public float speedBoostDuration = 3f;

    private void Start()
    {
        playerShoot = GetComponent<PlayerShoot>();
        playerMove = GetComponent<PlayerMove>();
        playerLook = GetComponent<PlayerLook>();
    }

    public void GetBoost(string boostName)
    {
        // TODO
        if ( boostName.Equals("speed") )
        {
            Boost newSpeedBoost = new SpeedBoost();
            //GetComponent<PlayerMove>().GetSpeedBoost();

            playerMove.movementSpeed *= speedBoostFactor;
            Debug.Log("Speed boost added, speed is now " + playerMove.movementSpeed);
            // RemoveBoost nakon x sekundi
        }
        else if (boostName.Equals("damage"))
        {
            //GetComponent<PlayerShoot>().GetDamageBoost();            
        }
        else if (boostName.Equals("walls"))
        {
            //GetComponent<PlayerMove>().GetWallBoost();
        }
    }
}

