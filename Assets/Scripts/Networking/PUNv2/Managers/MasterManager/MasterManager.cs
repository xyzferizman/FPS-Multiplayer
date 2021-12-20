using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Photon.Pun;

[CreateAssetMenu(menuName = "Singletons/MasterManager")]
public class MasterManager : SingletonScriptableObject<MasterManager>
{
    [SerializeField]
    private GameSettings _gameSettings;

    public static GameSettings GameSettings { get { return instance._gameSettings; } }

    [SerializeField]
    private List<NetworkedPrefab> _networkedPrefabs = new List<NetworkedPrefab>();

    public static GameObject NetworkInstantiate(GameObject obj, Vector3 position, Quaternion rotation)
    {
        Debug.Log("network instantiate method, _networkedPrefabs count = " + instance._networkedPrefabs.Count);
        foreach (NetworkedPrefab networkedPrefab in instance._networkedPrefabs)
        {
            if ( networkedPrefab.Prefab == obj)
            {
                if (networkedPrefab.Path != string.Empty)
                {
                    GameObject result = PhotonNetwork.Instantiate(networkedPrefab.Path, position, rotation);
                    return result;
                }
                else
                {
                    Debug.LogError("Path is empty for gameobject name " +  networkedPrefab.Prefab);
                    return null;
                }                
            }
        }
        return null;
    }

    public GameObject GetProjectilePrefab()
    {
        return _networkedPrefabs.Find(netPrefab => netPrefab.Prefab.name.Equals("PUN_Projectile")).Prefab;
    }

    
      // ONLY going to run if you're inside the editor
    // in order to populate your NetworkedPrefabs you have to at least hit play ONCE after you added new prefab OR moved them around
    // IF you want it to populate NetworkedPrefabs on build you'll have to write another editor script
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void PopulateNetworkedPrefabs()
    {
#if UNITY_EDITOR
        instance._networkedPrefabs.Clear();

        GameObject[] results = Resources.LoadAll<GameObject>("");   // loads entire Resources folder
        for (int i = 0; i < results.Length; ++i)
        {
            if (results[i].GetComponent<PhotonView>() != null)
            {
                // for assembly-csharp.player, says "Not Available"
                string path = AssetDatabase.GetAssetPath(results[i]);
                instance._networkedPrefabs.Add(new NetworkedPrefab(results[i], path));

                //Debug.Log("Resource loaded: " + results[i].name);
            }
        }

#endif
    }
    
}
