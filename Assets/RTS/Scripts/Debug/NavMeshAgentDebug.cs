/*    
   Copyright (C) 2020 Federico Peinado
   http://www.federicopeinado.com

   Este fichero forma parte del material de la asignatura Inteligencia Artificial para Videojuegos.
   Esta asignatura se imparte en la Facultad de Informática de la Universidad Complutense de Madrid (España).

   Autores originales: Opsive (Behavior Designer Samples)
   Revisión: Federico Peinado 
   Contacto: email@federicopeinado.com
*/
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace es.ucm.fdi.iav.rts
{
    /* 
     * Depurador para ver las rutas de los agentes de mallas de navegación.
     */
    public class NavMeshAgentDebug : MonoBehaviour
    {
        public LineRenderer line; //to hold the line Renderer
        public Transform target; //to hold the transform of the target
        public NavMeshAgent agent; //to hold the agent of this gameObject

        private void Start()
        {
            line = GetComponent<LineRenderer>(); //get the line renderer
            agent = GetComponent<NavMeshAgent>(); //get the agent
            StartCoroutine("getPath"); // En vez de llamar directamente a getPath();
        }

        // Añadí el IEnumerator
        IEnumerator getPath()
        {
            line.SetPosition(0, transform.position); //set the line's origin

            // Se me ha ocurrido meter este bucle para que se repita todo
            for (; ; )
            {

                //agent.SetDestination(target.position); //create the path
                //yield WaitForEndOfFrame(); //wait for the path to generate
                yield return new WaitForEndOfFrame();


                /* IDEAS DE OTRA GENTE
                 * I would use CalculatePath instead of setting the destination and then, stopping the agent.

    https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.CalculatePath.html



    I would also check whether the computed path is computed before trying to draw it.

    https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-pathPending.html
                */
                if (!agent.pathPending)
                    DrawPath(agent.path);

                //agent.Stop();//add this if you don't want to move the agent
                //agent.isStopped = true;

            }
        }

        /* Otra manera de hacerlo
         public static void DrawPath(NavMeshPath path, Vector3 start)
    {
        Vector3 current = start;
        Vector3 next;
        Color green = new Color(0f,1f,0f);
        for(int i = 0; i < (path.corners.Length-1);i++)
        {
           next = path.corners[i];
           Debug.DrawLine(current, next, green, 5.0f);
           current = next;
        }
    }
        */
            public void DrawPath(NavMeshPath path)
        {
            if (path.corners.Length < 2) //if the path has 1 or no corners, there is no need
                return;

            //line.SetVertexCount(path.corners.Length); //set the array of positions to the amount of corners
            line.positionCount = path.corners.Length;

            // Creo que esto puede abreviarse como line.SetPositions(path.corners)
            /* for (var i = 1; i < path.corners.Length; i++)
            {
                line.SetPosition(i, path.corners[i]); //go through each corner and set that to the line renderer's position
            } */
            line.SetPositions(path.corners);
        }
    }
}
 