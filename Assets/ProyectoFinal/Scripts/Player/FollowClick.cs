using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Mueve el transform siguiendo a su objetivo, con un offset
 */

public class FollowClick : MonoBehaviour
{

    public Transform objetivo;

    public Vector3 offset;
    private float camSpeed = 10;
    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, objetivo.position + offset, 10 * Time.deltaTime);
    }
}
