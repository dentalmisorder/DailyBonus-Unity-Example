using System;
using System.Collections;
using UnityEngine;

namespace AnotherMatch3.Hearts
{
    public class HeartCounter : MonoBehaviour
    {
        [SerializeField] private int maxHealths = 5;
        [SerializeField] private int currentHealths = 3;
        [SerializeField] private int secondsToRefillLife = 20;
        [SerializeField] private int minutesToRefillLife = 0;

        [SerializeField] private string maxHealthsTimerMessage = "Full";
        [SerializeField] private string timerFormat = @"mm\:ss";

        private DateTime futureMark;
        private TimeSpan currentInterval;

        public event Action<string> OnCounterUpdated;
        public event Action<int> OnHealthsUpdated;

        private void Start()
        {
            SetEndPointTime(minutesToRefillLife, secondsToRefillLife);
            OnHealthsUpdated?.Invoke(currentHealths);

            StartCoroutine(Tick());
        }

        /// <summary>
        /// Updating current healths to their max level by default
        /// or a custom value if u pass in a count
        /// </summary>
        /// <param name="count">Leave it as 0 to refill to max health or pass an int if u want to get something like a health pack or smth</param>
        public void UpdateHearts(int count = 0)
        {
            var value = count == 0 ? maxHealths : count;
            currentHealths = count;
            SetEndPointTime(minutesToRefillLife, secondsToRefillLife); //updating the ticks in case if it was full and we spent some so it starts from new cycle
            OnHealthsUpdated?.Invoke(currentHealths);
        }

        public void UpdateHeartsToMax() => UpdateHearts(maxHealths);

        public void DecreaseHeart(int count = 1) => UpdateHearts(--currentHealths);

        public void IncreaseHeart(int count = 1) => UpdateHearts(++currentHealths);

        public int GetCurrentHealths() => currentHealths;

        public int GetMaxHealths() => maxHealths;

        public void SetEndPointTime(int minutes, int seconds)
        {
            futureMark = DateTime.Now;

            futureMark = futureMark.AddMinutes(minutes);
            futureMark = futureMark.AddSeconds(seconds);
        }

        private IEnumerator Tick()
        {
            while(true)
            {
                UpdateInterval();
                OnCounterUpdated?.Invoke(currentHealths >= maxHealths ? maxHealthsTimerMessage : currentInterval.ToString(timerFormat));
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        private void UpdateInterval()
        {
            if(DateTime.Now >= futureMark && currentHealths >= maxHealths) return;

            if (DateTime.Now >= futureMark && currentHealths < maxHealths)
            {
                SetEndPointTime(minutesToRefillLife, secondsToRefillLife);
                currentHealths++;
                OnHealthsUpdated?.Invoke(currentHealths);
            }

            currentInterval = futureMark.ToUniversalTime() - DateTime.Now.ToUniversalTime();
        }
    }   
}