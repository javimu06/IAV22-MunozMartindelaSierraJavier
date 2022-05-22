using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class StuffBehaviour : MonoBehaviour
{
    public string effect;
    public int timeEffect;

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

        if (effect == "speedUp" || effect == "speedDown")
        {
            playerSpeed = GameManager.Instance.Player.GetComponent<NavMeshAgent>().speed;
            highSpeed = playerSpeed + playerSpeed * exponentSpeed;
            reducedSpeed = playerSpeed - playerSpeed / exponentSpeed;
        }
        else if (effect == "zoomIn" || effect == "zoomOut")
        {
            cameraZoom = GameManager.Instance.Player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset;
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
        if (other.GetComponent<Player>())
        {
            //Activar timer
            timer.setTime(timeEffect);
            //Activar efecto
            activateEffect();
            //Añadir a la lista de efectos del player
            GameManager.Instance.Player.GetComponent<BehaviourList>().addElement(this);
            gameObject.SetActive(false);
        }
    }

    void activateEffect()
    {
        switch (effect)
        {
            case "speedUp":
                GameManager.Instance.Player.GetComponent<NavMeshAgent>().speed = highSpeed;
                break;
            case "speedDown":
                GameManager.Instance.Player.GetComponent<NavMeshAgent>().speed = reducedSpeed;
                break;
            case "zoomOut":
                GameManager.Instance.Player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset = highZoom;
                break;
            case "zoomIn":
                GameManager.Instance.Player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset = reducedZoom;
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
                GameManager.Instance.Player.GetComponent<NavMeshAgent>().speed = playerSpeed;
                break;
            case "speedDown":
                GameManager.Instance.Player.GetComponent<NavMeshAgent>().speed = playerSpeed;
                break;
            case "zoomIn":
                GameManager.Instance.Player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset = cameraZoom;
                break;
            case "zoomOut":
                GameManager.Instance.Player.GetComponent<Player>().camera.GetComponent<FollowClick>().offset = cameraZoom;
                break;
            default:
                break;
        }
    }
}