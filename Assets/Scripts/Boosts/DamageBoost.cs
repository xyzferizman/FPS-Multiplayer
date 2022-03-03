using UnityEngine;

class DamageBoost : Boost
{
    private float damageMultiplier = 0.5f;
    private float damageBoostAddition;
    private float startTime;

    public DamageBoost()
    {
        isStackable = true;
        duration = 5f;
        myBoostType = BoostType.Damage;
    }

    internal override void SetUnityRelatedStuff()
    {
        base.SetUnityRelatedStuff();

        //Debug.Log("DamageBoost, SetUnityRelatedStuff");
        damageBoostAddition = playerShoot.baseDamage * damageMultiplier;
    }

    internal override void ApplyBoost()  {

        // nema uvjeta kao kod Speed Boost jer je Damage Boost stackable
        playerShoot.damage += damageBoostAddition;

        Debug.Log("Damage boost added, Time = " + Time.time);
        startTime = Time.time;

        Invoke(nameof(RemoveBoost), duration);
        boostDict[myBoostType]++;
        Debug.Log("Damage Boost Count = " + boostDict[myBoostType]);
        Debug.Log("Damage = " + playerShoot.damage);
        Debug.Log("----------------------");
    }

    protected override void RemoveBoost() {

        // nema uvjeta kao kod Speed Boost jer je Damage Boost stackable

        Debug.Log("Damage Boost removed. Time = " + Time.time);
        playerShoot.damage -= damageBoostAddition;
        Destroy(this);

        boostDict[myBoostType]--;
        Debug.Log("Damage Boost Count = " + boostDict[myBoostType]);
        Debug.Log("Damage = " + playerShoot.damage);
        Debug.Log("----------------------");
    }
}


