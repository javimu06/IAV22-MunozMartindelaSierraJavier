using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StuffBehaviour : MonoBehaviour
{
    public string effect;
    public int timeEffect;

    public GameObject player;
    Timer timer;

    float playerSpeed;
    public float exponentSpeed;
    float reducedSpeed;
    float highSpeed;

    Vector3 cameraZoom;
    public float exponentCamera;
    Vector3 reducedZoom;
    Vector3 highZoom;
    private void Start()
    {
        timer = GetComponent<Timer>();

        if (effect == "speedUp")
        {
            playerSpeed = player.GetComponent<NavMeshAgent>().speed;
            highSpeed = playerSpeed + playerSpeed * exponentSpeed;
            reducedSpeed = playerSpeed - playerSpeed / exponentSpeed;
        }
        else if (effect == "zoomIn")
        {
            cameraZoom = player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset;
            highZoom = cameraZoom + cameraZoom * exponentSpeed;
            reducedZoom = cameraZoom - cameraZoom / exponentSpeed;
        }
    }

    public bool itsOver()
    {
        return timer.itsOver();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Activar timer
        timer.setTime(timeEffect);
        //Activar efecto
        activateEffect();
        //Añadir a la lista de efectos del player
        player.GetComponent<BehaviourList>().addElement(this);
    }

    void activateEffect()
    {
        switch (effect)
        {
            case "speedUp":
                player.GetComponent<NavMeshAgent>().speed = highSpeed;
                break;
            case "speedDown":
                player.GetComponent<NavMeshAgent>().speed = reducedSpeed;
                break;
            case "zoomIn":
                player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset = highZoom;
                break;
            case "zoomOut":
                player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset = reducedZoom;
                break;
            default:
                break;
        }
    }

    public void deActivateEffect()
    {
        switch (effect)
        {
            case "speedUp":
                player.GetComponent<NavMeshAgent>().speed = playerSpeed;
                break;
            case "speedDown":
                player.GetComponent<NavMeshAgent>().speed = playerSpeed;
                break;
            case "zoomIn":
                player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset = cameraZoom;
                break;
            case "zoomOut":
                player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset = cameraZoom;
                break;
            default:
                break;
        }
    }
}