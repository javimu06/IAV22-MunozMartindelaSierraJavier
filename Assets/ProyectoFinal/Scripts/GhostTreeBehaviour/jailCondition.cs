using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner.Runtime.Tasks;

public class jailCondition : Conditional
{
    public override TaskStatus OnUpdate()
    {
        if (GameManager.Instance.estadoJaula)
            return TaskStatus.Success;
        else
            return TaskStatus.Failure;
    }
}
