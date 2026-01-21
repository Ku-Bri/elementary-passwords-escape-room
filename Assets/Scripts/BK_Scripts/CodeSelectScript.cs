using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CodeSelectScript : MonoBehaviour
{
    public TextMeshProUGUI question;

    private void Start()
    {
        question.text = "What is the PASSCODE the PasswordManager created for you for Set" + Random.Range(1, 4) + "?";
    }
}
