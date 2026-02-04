using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;
using Debug = UnityEngine.Debug;
using Unity.VisualScripting;

public class PanelOperatorMulti : MonoBehaviour
{
    public GameObject[] panelArray;
    public bool[] isPanelActiveArray;
    public GameObject sceneObjects;
    public GameObject notebook;
    public GameObject textFile;
    public GameObject stickyNote;
    public GameObject scrollObjects;

    Draggable nb;
    Draggable tf;
    Draggable sn;

    private void Awake()
    {
        if (isPanelActiveArray[0])
        {
            panelArray[0].SetActive(isPanelActiveArray[0]);
        }
        //sceneObjects.SetActive(false);
        //Debug.Log("SceneObjects should not be active AWAKE");
        scrollObjects.SetActive(false);
    }

    private void Start()
    {
        sceneObjects.SetActive(false);
        Debug.Log("SceneObjects should not be active START");
        /*notebook.SetActive(false);
        textFile.SetActive(false);
        stickyNote.SetActive(false);
        */

        nb = notebook.GetComponent<Draggable>();
        tf = notebook.GetComponent<Draggable>();
        sn = notebook.GetComponent<Draggable>();

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
                //Debug.Log(j);
                //Debug.Log(panelArray[j] + " sent to inactive from for loop");
                SetPanelInactive(j);
            }
            SetPanelActive(i);
        }


    }

    public void SetPanelActive(int i)
    {
        isPanelActiveArray[i] = true;
        panelArray[i].SetActive(true);
        //Debug.Log(panelArray[i] + " set active");
        HideSceneObjects();
    }
    public void SetPanelInactive(int i)
    {
        isPanelActiveArray[i] = false;
        panelArray[i].SetActive(false);
        //Debug.Log(panelArray[i] + " set inactive");
        UnhideSceneObjects();
    }

    public void HideSceneObjects()
    {
        sceneObjects.SetActive(false);
        /*notebook.SetActive(false);
        textFile.SetActive(false);
        stickyNote.SetActive(false);
        */
    }

    public void UnhideSceneObjects()
    {
        //Debug.Log("UnhideSceneObjects has been accessed");
        sceneObjects.SetActive(true);
        /*if (!nb.hasBeenAdded)
        {
            notebook.SetActive(true);
        }
        if (!tf.hasBeenAdded)
        {
            textFile.SetActive(true);
        }
        if (!sn.hasBeenAdded)
        {
            stickyNote.SetActive(true);
        }
        */
    }
}
