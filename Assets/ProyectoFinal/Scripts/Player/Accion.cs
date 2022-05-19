using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Accion : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Piano"))
        {
            //other.gameObject.GetComponent<ControlPiano>().Interact();
            Debug.Log("Han golpeado al piano");
        }
        else if (other.gameObject.CompareTag("Palanca"))
        {
            //other.gameObject.GetComponent<ControlPalanca>().Interact();
            Debug.Log("Ha tocado la palanca");
        }
        else if (other.gameObject.CompareTag("Puerta"))
        {
            Debug.Log("Ha tocado la puerta");
        }
    }
}
