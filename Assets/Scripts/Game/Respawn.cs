using UnityEngine;
using UnityEngine.UI;
using MLAPI;

public class Respawn : MonoBehaviour
{
    private Text countdownText;
    private Button respawnButton;
    private GameObject UIPanel;
    private readonly int maxCountDownValue = 5;
    private int currentCountDownValue;
    private string netType;
    private PUN_Starter punStarter;

    private void Start()
    {
        UIPanel = FindObjectOfType<RespawnUI>().gameObject;
        countdownText = UIPanel.transform.GetChild(0).gameObject.GetComponent<Text>();
        respawnButton = UIPanel.transform.GetChild(1).gameObject.GetComponent<Button>();
        netType = PlayerPrefs.GetString("networkType");
        punStarter = FindObjectOfType<PUN_Starter>();
    }

    internal void InitializeRespawnCountdown()
    {
        // do countdown
        countdownText.gameObject.SetActive(true);
        countdownText.text = maxCountDownValue.ToString();
        currentCountDownValue = maxCountDownValue;
        Invoke("CountDown", 1f);
    }

    private void CountDown()
    {
        currentCountDownValue--;
        if (currentCountDownValue > 0)
        {
            countdownText.text = currentCountDownValue.ToString();
            Invoke("CountDown", 1f);
        }
        else
        {
            countdownText.gameObject.SetActive(false);
            respawnButton.gameObject.SetActive(true);
        }
    }

    public void RespawnButton_OnClick()
    {
        respawnButton.gameObject.SetActive(false);

        if ( netType == "mlapi")
        {
            FindObjectOfType<NetworkGameManager>().RespawnClientPlayer_ServerRpc(NetworkManager.Singleton.LocalClientId);  // no matter, client or host, should run properly
        }
        else if ( netType == "pun_v2" )
        {
            punStarter.RespawnPlayer();
        }
        else if ( netType == "transport" )
        {
            // respawn player
            Debug.Log("Transport respawn should happen");
        }
        
        
    }
}
