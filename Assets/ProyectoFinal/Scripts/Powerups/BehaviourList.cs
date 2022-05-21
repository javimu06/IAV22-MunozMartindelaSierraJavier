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
        if (effectList.Count > 0)
        {
            List<StuffBehaviour> removeBehav = new List<StuffBehaviour>();
            foreach (StuffBehaviour a in effectList)
            {
                if (a.itsOver())
                {
                    a.deActivateEffect();
                    removeBehav.Add(a);
                    //effectList.Remove(a);
                }
            }
            foreach (StuffBehaviour a in removeBehav)
            {
                effectList.Remove(a);
            }
            removeBehav.Clear();
        }
    }
}
