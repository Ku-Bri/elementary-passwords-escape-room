using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using Debug = UnityEngine.Debug;

public class PanelOperatorBasic : MonoBehaviour
{
    public GameObject[] panelArray;
    public bool[] isPanelActiveArray;
    //public GameObject sceneObjects;

    private bool[] isPanelActive;

    private void Start()
    {
        StartCoroutine(PanelDelay());
    }

    private IEnumerator PanelDelay()
    {
        yield return null;

        if (isPanelActiveArray[0])
        {
            panelArray[0].SetActive(isPanelActiveArray[0]);
        }
        //sceneObjects.SetActive(false);
    }
    public void ShowPanel(int i)
    {
        if (isPanelActiveArray[i])
        {
            //Debug.Log(panelArray[i] + " sent to inactive from if");
            SetPanelInactive(i);
        }
        else
        {
            for (int j = 0; j < panelArray.Length; j++)
            {
                Debug.Log(j);
                Debug.Log(panelArray[j] + " sent to inactive from for loop");
                SetPanelInactive(j);
            }
            SetPanelActive(i);
        }


    }

    public void SetPanelActive(int i)
    {
        isPanelActiveArray[i] = true;
        panelArray[i].SetActive(true);
        Debug.Log(panelArray[i] + " set active");
        //HideSceneObjects();
    }
    public void SetPanelInactive(int i)
    {
        isPanelActiveArray[i] = false;
        panelArray[i].SetActive(false);
        Debug.Log(panelArray[i] + " set inactive");
        //UnhideSceneObjects();
    }
/*
    public void HideSceneObjects()
    {
        sceneObjects.SetActive(false);
    }

    public void UnhideSceneObjects()
    {
        sceneObjects.SetActive(true);
    }
*/
}
