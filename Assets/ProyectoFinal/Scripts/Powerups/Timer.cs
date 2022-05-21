using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public float initialTime_;
    float actualTime_;

    public bool Repeat;
    bool activate = false;

    // Start is called before the first frame update
    void Start()
    {
        actualTime_ = initialTime_;
    }
    // Update is called once per frame
    void Update()
    {
        if (activate)
        {
            actualTime_ -= Time.deltaTime;

            if (itsOver() && Repeat)
                actualTime_ = initialTime_;
        }
    }

    public void setTime(int tiempo)
    {
        initialTime_ = tiempo;
        actualTime_ = initialTime_;
        activate = true;
    }

    public void restartTime()
    {
        actualTime_ = initialTime_;
    }

    public bool itsOver()
    {
        if (actualTime_ < 0) return true;
        else return false;
    }
}
