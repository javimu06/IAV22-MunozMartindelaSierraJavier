using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Camera main, general;

    private void Start()
    {
        main.enabled = true;
        general.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown("1"))
        {
            main.enabled = true;
            general.enabled = false;
        }
        else if (Input.GetKeyDown("2"))
        {
            main.enabled = false;
            general.enabled = true;
        }
    }
}
