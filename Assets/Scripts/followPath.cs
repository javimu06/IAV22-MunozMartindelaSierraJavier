using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UCM.IAV.Navegacion
{
    public class followPath : MonoBehaviour
    {
        List<Vertex> ourPath;
        Vertex currentNode;

        public GameObject hiloPrefab;
        GameObject HiloAriadna;
        float movementSpeed = 5f;

        private void OnDisable()
        {
            if (HiloAriadna != null)
            {
                HiloAriadna.GetComponent<lineController>().restart();
            }
        }

        public void updatePath(List<Vertex> newPath)
        {
            ourPath = newPath;
            if (ourPath.Count > 0)
                currentNode = ourPath[ourPath.Count - 1];
            if (GetComponent<PlayerManager>())
            {
                if (HiloAriadna == null)
                {
                    HiloAriadna = Instantiate(hiloPrefab, new Vector3(0, 0.5f, 0), Quaternion.identity) as GameObject;
                    HiloAriadna.GetComponent<lineController>().player = this.gameObject;
                }
                HiloAriadna.GetComponent<lineController>().updatePath(newPath);
            }

        }

        // Update is called once per frame
        void Update()
        {
            Vector3 nextPos = GraphGrid.instance_.IdToGrid(currentNode.id);
            nextPos.z = nextPos.y;
            nextPos.y = transform.position.y;
            transform.position = Vector3.MoveTowards(transform.position, nextPos, movementSpeed * Time.deltaTime);


            if (Vector3.Distance(transform.position, nextPos) <= 0.5f)
            {
                ourPath.Remove(currentNode);
                if (HiloAriadna != null)
                    HiloAriadna.GetComponent<lineController>().updateLine(currentNode.transform);

                if (ourPath.Count != 0)
                    currentNode = ourPath[ourPath.Count - 1];
                else
                    GetComponent<followPath>().enabled = false;
            }
        }
    }
}