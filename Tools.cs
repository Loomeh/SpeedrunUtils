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
    }
}