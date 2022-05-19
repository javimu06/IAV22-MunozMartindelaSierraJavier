using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourList : MonoBehaviour
{
    public static List<StuffBehaviour> effectList;


    private void Start()
    {
        effectList = new List<StuffBehaviour>();
    }
    public void addElement(StuffBehaviour behav)
    {
        effectList.Add(behav);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (StuffBehaviour a in effectList)
        {
            if (a.itsOver())
            {
                a.deActivateEffect();
                effectList.Remove(a);
            }
        }
    }
}
