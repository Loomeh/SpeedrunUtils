using System.Reflection;
using Reptile;
using UnityEngine;

namespace SpeedrunUtils
{
    public class Tools : MonoBehaviour
    {
        public static Tools Instance;

        private Core core;
        private WorldHandler world;
        public bool coreHasBeenSetup;

        public Tools()
        {
            Instance = this;
        }

        private bool delegateHasBeenSetup = false;

        public void Update()
        {
            if (!coreHasBeenSetup)
            {
                core = Core.Instance;
                if (core != null)
                {
                    world = WorldHandler.instance;
                    coreHasBeenSetup = world != null;

                    if (!delegateHasBeenSetup)
                    {
                        StageManager.OnStageInitialized += () =>
                        {
                            Debug.Log("Swapped to new stage!");
                            coreHasBeenSetup = false;
                        };

                        delegateHasBeenSetup = true;
                    }
                }
            }
        }
    }
}