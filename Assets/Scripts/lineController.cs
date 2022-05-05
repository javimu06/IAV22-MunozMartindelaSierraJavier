using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UCM.IAV.Navegacion
{
    public class lineController : MonoBehaviour
    {
        public GameObject player;

        public Queue<GameObject> circles = new Queue<GameObject>();
        public GameObject circle;
        public void updateLine(Transform aux)
        {
            if (circles.Count > 0)
            {
                GameObject toDelete;
                toDelete = circles.Dequeue();
                Destroy(toDelete);
            }

            GetComponent<LineRenderer>().positionCount--;
        }

        public void updatePath(List<Vertex> newPath)
        {
            GetComponent<LineRenderer>().positionCount = newPath.Count;
            for (int i = 0; i < newPath.Count - 1; ++i)
            {
                GetComponent<LineRenderer>().SetPosition(i, newPath[i].transform.position + new Vector3(0, 0.5f, 0));
                circles.Enqueue(Instantiate(circle, newPath[newPath.Count - 1 - i].transform.position + new Vector3(0, 0.25f, 0), Quaternion.identity) as GameObject);
            }
            GetComponent<LineRenderer>().SetPosition(newPath.Count - 1, player.transform.position);
        }

        public void restart()
        {
            GetComponent<LineRenderer>().positionCount = 0;
            int aux = circles.Count;
            for (int i = 0; i < aux; ++i)
            {
                GameObject toDelete = circles.Dequeue();
                Destroy(toDelete);
            }
        }
    }
}
