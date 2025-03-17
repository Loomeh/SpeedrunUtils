using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpeedrunUtils
{
    public class SRUStopwatch : MonoBehaviour
    {
        private bool _timerActive;
        private float _currentTime;
        public string stopwatchString = "0:00.000";

        public void Start()
        {
            _currentTime = 0;
        }

        public void StartStopwatch()
        {
            _timerActive = true;
        }

        public void StopStopwatch()
        {
            _timerActive = false;
        }

        public void ResetStopwatch()
        {
            _currentTime = 0;
        }

        public string GetStopwatchTimeString()
        {
            return stopwatchString;
        }

        public float GetStopwatchTimeNum()
        {
            return _currentTime;
        }

        public bool GetStopwatchState()
        {
            return _timerActive;
        }

        public void Update()
        {
            if(_timerActive)
            {
                _currentTime = _currentTime + Time.deltaTime;
            }
            TimeSpan _time = TimeSpan.FromSeconds(_currentTime);

            stopwatchString = _time.Minutes.ToString() + ":" + _time.Seconds.ToString() + "." + _time.Milliseconds.ToString();
        }
    }
}
