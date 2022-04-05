using UnityEngine;

class SpeedBoost : Boost
{    
    internal static float speedBoostFactor = 0.5f;
    private float speedBoostAddition;
    private float startTime;

    private BoostController myBoostController;

    public SpeedBoost() : base()
    {
        isStackable = false;
        duration = 3f;
        myBoostType = BoostType.Speed;
    }

    internal override void SetUnityRelatedStuff()
    {
        base.SetUnityRelatedStuff();

        //Debug.Log("SpeedBoost, SetUnityRelatedStuff");
        speedBoostAddition = playerMove.baseMovementSpeed * speedBoostFactor;
    }

    internal override void ApplyBoost()  
    {                
        startTime = Time.time;
        //Debug.Log("Apply Speed Boost invoked. Time = " + startTime);

        /// speedboost nije stackable, dakle ako postoji vec neki speed boost, samo "overwritaj" stari boost,
        /// tj zadrzi isti boost i pomakni vrijeme micanja boosta za 'duration'

        // ako nije aktivan nijedan speed boost, onda dodaj novi speedboost
        if ( boostDict[myBoostType] == 0 )
        {
            playerMove.movementSpeed += speedBoostAddition;
            //Debug.Log("Speed Boost applied. Time = " + startTime);            
        }
        // inace produzi trajanje boosta (RemoveBoost metoda se brine za to)
        else
        {
            
        }

        Invoke(nameof(RemoveBoost), duration);
        boostDict[myBoostType]++;
        //Debug.Log("Speed Boost Count = " + boostDict[myBoostType]);
        //Debug.Log("Movement Speed = " + playerMove.movementSpeed);
        //Debug.Log("----------------------");
    }
    
    protected override void RemoveBoost()
    {
        Debug.Log("Remove Speed Boost invoked. Time = " + Time.time);

        // ako stvarno treba ukloniti speedboost, prilagodi brzinu i unisti instancu SpeedBoost skripte
        if ( boostDict[myBoostType] == 1 )
        {
            Debug.Log("Speed Boost removed. Time = " + Time.time);
            playerMove.movementSpeed -= speedBoostAddition;
            Destroy(this);
        }

        boostDict[myBoostType]--;
        Debug.Log("Speed Boost Count = " + boostDict[myBoostType]);
        Debug.Log("Movement Speed = " + playerMove.movementSpeed);
        Debug.Log("----------------------");
    }
}