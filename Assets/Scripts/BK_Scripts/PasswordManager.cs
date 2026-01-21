using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordManager : MonoBehaviour
{
    private List<string> pm = new List<string>();
    private int slotIndex = 0;

    public TextMeshProUGUI textList;
    public GameObject passcodesObject;
    public TextMeshProUGUI passcodes;

    private int code1;
    private int code2;
    private int code3;

    private bool scenePreviouslyLoaded;

    private void Start()
    {
        textList.text = "<size=50><align=\"center\">_SECURED PASSWORDS_</align>\n\n";
        passcodes.text = "<size=45><align=\"center\">\n\nPASSCODES</align>\n";
        passcodesObject.SetActive(false);

       if(!scenePreviouslyLoaded)
        {
            scenePreviouslyLoaded = true;
            createRandomNums();
        }
        
    }

    private void createRandomNums()
    {
        code1 = Random.Range(1000, 9999);
        code2 = Random.Range(1000, 9999);
        code3 = Random.Range(1000, 9999);
    }
    public void Populate(string website, string username,  string password)
    {
        pm.Add(website);
        pm.Add(username);
        pm.Add(password);

        //textList.text = textList.text + "\nSlot " + (slotIndex + 1) + "\n" + website + "\n" + username + "\n" + password + "\n";
        textList.text = textList.text + "<size=45>Set " + (slotIndex + 1) + "</size>\n"
                            + "<size=35>" + website + "</size>\n"
                            + "<size=35>" + username + "</size>\n"
                            + "<size=35>" + password + "</size>\n";

        slotIndex++;
        textList.text = textList.text + "\n";
        if (slotIndex == 3)
        {
            passcodesObject.SetActive(true);
            passcodes.text += "<size=45><align=\"center\">Set1</align>\n" + "<size=35><align=\"center\">" + code1 + "</align>\n" +
                              "<size=45><align=\"center\">\n\nSet2</align>\n" + "<size=35><align=\"center\">" + code2 + "</align>\n" +
                              "<size=45><align=\"center\">\n\nSet3</align>\n" + "<size=35><align=\"center\">" + code3 + "</align>\n";
        }

    }

    void OnMouseDown()
    {
        GUIUtility.systemCopyBuffer = textList.text;
    }
}
