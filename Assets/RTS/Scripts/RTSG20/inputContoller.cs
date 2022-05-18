using es.ucm.fdi.iav.rts.G20;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace es.ucm.fdi.iav.rts
{
    public class inputContoller : MonoBehaviour
    {
        public RTSAIControllerG20 controller;
        public RTSAIControllerG20 controller2;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown("f"))//F,G,H
            {
                if (controller != null)
                    controller.influencemap_.changeTeamColor(0);
                if (controller2 != null)
                    controller2.influencemap_.changeTeamColor(1);
            }
            else if (Input.GetKeyDown("g"))//F,G,H
            {
                if (controller != null)
                    controller.influencemap_.changeTeamColor(2);
                if (controller2 != null)
                    controller2.influencemap_.changeTeamColor(2);
            }
            else if (Input.GetKeyDown("h"))//F,G,H
            {
                if (controller != null)
                    controller.influencemap_.changeTeamColor(1);
                if (controller2 != null)
                    controller2.influencemap_.changeTeamColor(0);
            }
            else if (Input.GetKeyDown("s"))//F,G,H
            {
                if (controller != null)
                    controller.influencemap_.debugPrefabParent.active = !controller.influencemap_.debugPrefabParent.active;
                if (controller2 != null)
                    controller2.influencemap_.debugPrefabParent.active = !controller2.influencemap_.debugPrefabParent.active;
            }
        }
    }
}