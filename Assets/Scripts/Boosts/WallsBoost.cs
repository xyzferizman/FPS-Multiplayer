using UnityEngine;

class WallsBoost : Boost
{
    private float startTime;

    public WallsBoost()
    {
        isStackable = false;
        duration = 4f;
        myBoostType = BoostType.Walls;
    }

    internal override void ApplyBoost()  {
        
        startTime = Time.time;

        if (boostDict[myBoostType] == 0)
        {
            playerMove.canGoThroughWalls = true;
            Debug.Log("Walls Boost applied. Time = " + startTime);
            boostDict[myBoostType]++;

            Invoke(nameof(RemoveBoost), duration);

            Debug.Log("Walls Boost Count = " + boostDict[myBoostType]);
            Debug.Log("----------------------");
        }
        // inace produzi trajanje boosta (RemoveBoost metoda se brine za to)
        else
        {

        }

        
    }

    protected override void RemoveBoost() {

        // TODO

        Debug.Log("Walls Boost removed. Time = " + Time.time);
        playerMove.canGoThroughWalls = false;
        Destroy(this);

        boostDict[myBoostType]--;
        Debug.Log("Walls Boost Count = " + boostDict[myBoostType]);
        Debug.Log("----------------------");
    }
}

