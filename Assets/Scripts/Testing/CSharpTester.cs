using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.Testing.CSharpTesting;

public class CSharpTester : MonoBehaviour
{
    void Start()
    {
        Debug.Log("code executing...");
        SomeRunningCode.ExecuteCode();        
    }
}
