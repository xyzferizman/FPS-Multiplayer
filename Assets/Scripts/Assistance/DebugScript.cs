using UnityEngine;
using MLAPI;
using UnityEngine.SceneManagement;

namespace DebugStuff
{
    public class DebugScript : MonoBehaviour
    {
        //#if !UNITY_EDITOR
        static string myLog = "";
        private string output;
        private string stack;

        void OnEnable()
        {
            Application.logMessageReceived += Log;
        }

        void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }

        public void Log(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Warning) return;
            output = logString;
            stack = stackTrace;
            if (myLog != "")
                myLog = myLog + "\n" + output;
            else
                myLog = output;
            if (myLog.Length > 5000)
            {
                myLog = myLog.Substring(0, 4000);
            }
        }

        void OnGUI()
        {
            if (/*!Application.isEditor*/true /*&& SceneManager.GetActiveScene().buildIndex == 1*/) //Do not display in editor ( or you can use the UNITY_EDITOR macro to also disable the rest)
            {
                myLog = GUI.TextArea(new Rect(300, 10, 300, 150), myLog);
                if (GUILayout.Button("Reset output")) myLog = "";
                if (GUILayout.Button("Disconnect client"))
                {
                    NetworkManager.Singleton.StopClient();
                    //SceneManager.LoadScene(0);
                }
                
                //if (GUILayout.Button("Move +X")) FindPlayerObj(2).transform.position += new Vector3(0.5f, 0f, 0f);
                //if (GUILayout.Button("Move -X")) FindPlayerObj(2).transform.position += new Vector3(-0.5f, 0f, 0f);
                //if (GUILayout.Button("Move +Z")) FindPlayerObj(2).transform.position += new Vector3(0f, 0f, 0.5f);
                //if (GUILayout.Button("Move -Z")) FindPlayerObj(2).transform.position += new Vector3(0f, 0f, -0.5f);
            }

        }
        //#endif
        Transport_Player FindPlayerObj(int id)
        {
            Transport_Player[] trans_players = FindObjectsOfType<Transport_Player>();
            foreach (var p in trans_players)
            {
                if (p.uniqueId == id)
                    return p;
            }
            Debug.Log("PROBLEM: on debug script, didnt find proper player object");
            return null;
        }
    }
}
