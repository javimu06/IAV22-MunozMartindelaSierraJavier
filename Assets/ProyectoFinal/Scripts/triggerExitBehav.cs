using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class triggerExitBehav : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>())
        {
            if (other.GetComponent<BehaviourList>().keyObtained())
                GameManager.Instance.endGame();
        }
    }
}
