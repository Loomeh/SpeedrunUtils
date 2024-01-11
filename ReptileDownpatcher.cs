using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Reptile;

namespace SpeedrunUtils
{
    internal class ReptileDownpatcher : MonoBehaviour
    {
        private Stage currentStage;
        private WorldHandler worldHandler;
        private BaseModule baseModule;
        private GameObject switchDoorWall;
        private MeshCollider switchDoorWallCollider;

        private void Update()
        {
            if(Core.Instance != null)
            {
                if(worldHandler == null)
                {
                    worldHandler = WorldHandler.instance;
                }

                if(baseModule == null)
                {
                    baseModule = Core.Instance.BaseModule;
                }
                else
                {
                    currentStage = baseModule.CurrentStage;
                }

                if(currentStage == Stage.Mall && switchDoorWall == null)
                {
                    switchDoorWall = GameObject.Find("M_WhiteTileWindow (36)");
                }

                if(switchDoorWall != null && switchDoorWallCollider == null)
                {
                    if(switchDoorWall.TryGetComponent<MeshCollider>(out switchDoorWallCollider))
                    {
                        switchDoorWallCollider.enabled = false;
                        Debug.Log("Switch door wall collision disabled!");
                    }
                    else
                    {
                        Debug.Log("Switch door wall collider could not be found! This is expected behaviour on the current public release.");
                    }
                }

            }
        }
    }
}
