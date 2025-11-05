using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenHintsEtc : MonoBehaviour
{
    public GameObject panel1;
    public bool isPanelActive;

    private void Awake()
    {
        if (isPanelActive)
        {
            panel1.SetActive(isPanelActive);
        }
    }
    public void ShowPanel()
    {
        panel1.SetActive(!isPanelActive);
        isPanelActive = !isPanelActive;
    }
}
