using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UCM.IAV.Navegacion
{
    public class playerController : MonoBehaviour
    {
        float movementSpeed = 5f;
        public Transform movePoint;


        // Start is called before the first frame update
        void Start()
        {
            movePoint.parent = null;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position, movePoint.position, movementSpeed * Time.deltaTime);
            if (Input.anyKey)
            {
                if (Vector3.Distance(transform.position, movePoint.position) <= 0.5f)
                {
                    if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f && Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
                    {
                        if (GraphGrid.instance_.mapVertices[Mathf.RoundToInt(transform.position.z + Input.GetAxisRaw("Vertical")), Mathf.RoundToInt(transform.position.x + Input.GetAxisRaw("Horizontal"))])
                            movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
                    }
                    else if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1f)
                    {
                        if (GraphGrid.instance_.mapVertices[Mathf.RoundToInt(transform.position.z), Mathf.RoundToInt(transform.position.x + Input.GetAxisRaw("Horizontal"))])
                            movePoint.position += new Vector3(Input.GetAxisRaw("Horizontal"), 0f, 0f);
                    }
                    else if (Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1f)
                        if (GraphGrid.instance_.mapVertices[Mathf.RoundToInt(transform.position.z + Input.GetAxisRaw("Vertical")), Mathf.RoundToInt(transform.position.x)])
                            movePoint.position += new Vector3(0f, 0f, Input.GetAxisRaw("Vertical"));

                }
            }
        }

        public void updateMovePoint(Vector3 newPos)
        {
            movePoint.position = newPos;
        }
    }
}