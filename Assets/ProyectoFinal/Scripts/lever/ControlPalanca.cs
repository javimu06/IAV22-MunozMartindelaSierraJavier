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
        if (other.gameObject.name == "Ghost" && caido) return;
        caido = !caido;
        Interact();
    }

    public void Interact()
    {
        //publico.GetComponent<Collider>().enabled = !caido && !otroControl.caido;



        if (!caido)
        {
            GameManager.Instance.jail.SetActive(true);
            NavMeshSurface nm = GameObject.FindObjectOfType<NavMeshSurface>();
            nm.UpdateNavMesh(nm.navMeshData);
            
            //jail.GetComponent<Rigidbody>().useGravity = true;
            //for (int i = 0; i < publico.transform.childCount; ++i)
            //{
            //    //publico.transform.GetChild(i).GetComponent<Publico>().apagaLuz(lado_);

            //}
            //float step = speed * Time.deltaTime;
            //transform.position = Vector3.MoveTowards(transform.position, target.position, step);
        }
        else
        {
            GameManager.Instance.jail.SetActive(false);
            NavMeshSurface nm = GameObject.FindObjectOfType<NavMeshSurface>();
            nm.UpdateNavMesh(nm.navMeshData);

            //jail.GetComponent<Rigidbody>().useGravity = false;
            //for (int i = 0; i < publico.transform.childCount; ++i)
            //{
            //    //publico.transform.GetChild(i).GetComponent<Publico>().enciendeLuz(lado_);
            //}
        }
    }
}
