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

    // Update is called once per frame
    void Update()
    {
        transform.position = objetivo.position + offset;
    }
}
