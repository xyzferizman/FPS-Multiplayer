using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Dropdown dropdown;
    public Text label;
    public GameObject errorImage;

    float errorTextDuration = 5f;

    private void Start()
    {
        #region stari_GUI
        /*
        dropdown.options.Clear();
        List<string> items = new List<string>();
        items.Add("Unity - MLAPI");
        items.Add("Unity - Transport");
        items.Add("Photon Unity Networking 2");
        
        foreach ( var item in items )
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = item });
        }

        //DropdownItemSelected(dropdown);
       
        dropdown.onValueChanged.AddListener(delegate { DropdownItemSelected(dropdown); });
        */
        #endregion
    
        if ( PlayerPrefs.GetInt("Server_DCClient_or_Shutdown") == 1)    {
            errorImage.GetComponentInChildren<Text>().text = "You got disconnected by the server or server shutdown happened.";
            errorImage.SetActive(true);
            Invoke("RemoveErrorImage",errorTextDuration);
            PlayerPrefs.SetInt("Server_DCClient_or_Shutdown",0);
        }
    }

    private void RemoveErrorImage() {
        errorImage.GetComponentInChildren<Text>().text = string.Empty;
        errorImage.SetActive(false);
    }

    private void DropdownItemSelected(Dropdown dropdown)
    {        
        int index = dropdown.value;
        label.text = dropdown.options[index].text;
        Debug.Log("dropdown item selected, index = " + index + ", text = " + label.text);
    }

    public void StartGameButton_Click()
    {
        string netType = string.Empty;

        switch(dropdown.value)
        {
            case 0:
                netType = "mlapi";
                break;
            case 1:
                netType = "transport";
                break;
            case 2:
                netType = "pun_v2";
                break;
            default:
                Debug.LogError("wrong dropdown index in NewMenuScript");
                break;
        }

        // hardcoded prilagodba (nakon diplomskog)
        netType = "mlapi";

        PlayerPrefs.SetString("networkType",netType);

        SceneManager.LoadScene(1);
    }

    public void ConfigurationButton_Click()
    {
        // TODO
    }

    public void QuitGameButton_Click()
    {
        Application.Quit();
    }
}
