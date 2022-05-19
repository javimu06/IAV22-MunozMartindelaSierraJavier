using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Sirve para ocultar la reticula de direccion cuando el jugador llega a su destino
 */

public class WayMark : MonoBehaviour
{
    public GameObject player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            this.gameObject.SetActive(false);
        }
    }
}
