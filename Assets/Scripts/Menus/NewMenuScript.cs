using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewMenuScript : MonoBehaviour
{
    public Dropdown dropdown;
    public Text label;

    private void Start()
    {
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
        PlayerPrefs.SetString("networkType",netType);

        SceneManager.LoadScene(1);
    }
}
