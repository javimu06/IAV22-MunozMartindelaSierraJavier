using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
 * Sube o baja los candelabros cuando un objeto se colisiona con este, además avisando al público de este evento
 */

public class ControlPalanca : MonoBehaviour
{
    //public GameObject publico;
    public float step;
    public bool caido { get; set; }
    public bool lado_ = false;

    public ControlPalanca otroControl;

    private void Start()
    {
        caido = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponent<Player>()) return;
        if (other.gameObject.name == "bot" && !caido) return;
        caido = !caido;
        Interact();
    }

    public void Interact()
    {
        //publico.GetComponent<Collider>().enabled = !caido && !otroControl.caido;
        GameManager.Instance.estadoJaula = !GameManager.Instance.estadoJaula;


        if (!caido)
        {
            GameManager.Instance.jail.SetActive(true);
            GameManager.Instance.mazeInstance.surfaces.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
        else
        {
            GameManager.Instance.jail.SetActive(false);
            GameManager.Instance.mazeInstance.surfaces.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }
}
