using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UCM.IAV.Navegacion
{
    public class minotauroManager : MonoBehaviour
    {
        public float maxRadius = 10;
        public GameObject childPos;
        public GameObject player;
        bool findPlayer = false;

        //raycast
        public float rayRange = 5.0f;

        // Start is called before the first frame update
        void Start()
        {
            GetComponent<followPath>().enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            //Buscar un punto aleatorio al que ir
            if (!findPlayer && !GetComponent<followPath>().enabled)
            {
                childPos.GetComponent<Transform>().position = newPoint();
                GetComponent<followPath>().updatePath(GraphGrid.instance_.GetPathAstar(gameObject, childPos));
                GetComponent<followPath>().enabled = true;
            }
            //Seguir al jugador
            else if (findPlayer)
            {
                GetComponent<followPath>().updatePath(GraphGrid.instance_.GetPathAstar(gameObject, player));
                GetComponent<followPath>().enabled = true;
            }

            findPlayerTrayectory();
        }

        Vector3 newPoint()
        {
            Vector3 newPos = new Vector3(-1, 0, -1);
            while (newPos.x > GraphGrid.instance_.mapVertices.GetLength(0) * GraphGrid.instance_.cellSize ||
                   newPos.x < 0 ||
                   newPos.z > GraphGrid.instance_.mapVertices.GetLength(1) * GraphGrid.instance_.cellSize ||
                   newPos.z < 0)
            {
                float randomAngle = Random.Range(0, 360);
                newPos = new Vector3(Mathf.Sin(randomAngle), 0.5f, Mathf.Cos(randomAngle));
                newPos *= Random.Range(0, maxRadius);
                newPos.y = transform.position.y;
            }

            return newPos;
        }

        bool findPlayerTrayectory()
        {
            Ray theRay = new Ray(transform.position, (player.transform.position - transform.position).normalized * rayRange);
            Debug.DrawRay(transform.position, (player.transform.position - transform.position).normalized * rayRange);
            if (Physics.Raycast(theRay, out RaycastHit hit, rayRange))
            {
                if (hit.collider.tag == "Player")
                {
                    findPlayer = true;
                }
                else
                    findPlayer = false;
            }
            return findPlayer;
        }
    }
}