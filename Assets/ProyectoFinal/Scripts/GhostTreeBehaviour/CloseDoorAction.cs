using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine.AI;
using BehaviorDesigner.Runtime.Tasks.Movement;
/*
 * Accion de cerrar la puerta de la celda, yendo hacia la palanca, cuando la alcanza devuelve Success
 */

public class CloseDoorAction : NavMeshMovement
{
    NavMeshAgent agent;
    GameObject puerta;


    public override TaskStatus OnUpdate()
    {
        puerta = GameManager.Instance.mazeInstance.lever;
        if (HasArrived())
            SetDestination(puerta.transform.position);
        if (Vector3.SqrMagnitude(transform.position - puerta.transform.position) < 8f)
        {
            SetDestination(transform.position);
            return TaskStatus.Success;
        }
        if (!GameManager.Instance.estadoJaula) return TaskStatus.Success;
        return TaskStatus.Running;
    }
}