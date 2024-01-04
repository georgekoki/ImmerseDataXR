using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoteBehaviour : MonoBehaviour
{
    public string noteContent;

    void Start()
    {
        GetComponentInChildren<TextMeshPro>().text = noteContent;
    }

}
