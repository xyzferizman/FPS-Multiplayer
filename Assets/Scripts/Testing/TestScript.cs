using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Debug.Log("Start Debug Log");

        // Boost boost1 = new SpeedBoost();
        // Debug.Log("boost1: " + boost1.testVar);
        // Debug.Log("boost1 spec: " + SpeedBoost.testVar);
        // Debug.Log("boost1 type: " + boost1.GetType());

        // Boost boost2 = new DamageBoost();
        // Debug.Log("boost2: " + boost2.testVar);
        // Debug.Log("boost2 spec: " + DamageBoost.testVar);
        // Debug.Log("boost2 type: " + boost2.GetType());

        // Boost boost3 = new WallsBoost();
        // Debug.Log("boost3: " + boost3.testVar);
        // Debug.Log("boost3 spec: " + WallsBoost.testVar);
        // Debug.Log("boost3 type: " + boost3.GetType());

        Debug.Log("Script on test object, Invoking in 3 sec");
        Invoke("SomeFunction",3f);
    }

    void SomeFunction()
    {
        Debug.Log("This is a Debug.Log in invoked function");
    }

    
}
