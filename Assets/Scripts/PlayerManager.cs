using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCM.IAV.Navegacion
{
    public class PlayerManager : MonoBehaviour
    {
        public GameObject salidaCamino;
        bool smoothPath = false;
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("s"))
                smoothPath = !smoothPath;



            if (Input.GetKeyDown("space"))
            {
                salidaCamino = GraphGrid.instance_.salida;
                GetComponent<playerController>().enabled = false;
                List<Vertex> path = GraphGrid.instance_.GetPathAstarCost(gameObject, salidaCamino);
                if (smoothPath)
                    path = GraphGrid.instance_.Smooth(path);
                GetComponent<followPath>().updatePath(path);
                GetComponent<followPath>().enabled = true;
            }
            if (Input.GetKeyUp("space"))
            {
                Vertex actual = GraphGrid.instance_.GetNearestVertex(transform.position);
                Vector3 actualPos = new Vector3(actual.transform.position.x, transform.position.y, actual.transform.position.z);
                GetComponent<playerController>().updateMovePoint(actualPos);
                GetComponent<playerController>().enabled = true;
                GetComponent<followPath>().enabled = false;
            }
        }
    }
}