using UnityEngine;
using AnotherMatch3.SaveSystem;
using AnotherMatch3.PopUps;
using AnotherMatch3.Hearts;

namespace AnotherMatch3
{
    public class RelationshipHandler : MonoBehaviour
    {
        [SerializeField] private LoginDataHolder dataHolder;
        [SerializeField] private PopUpDisplayer popUpDisplayer;
        [SerializeField] private HeartCounter heartCounter;

        private void Awake() => DontDestroyOnLoad(this);

        private void OnEnable()
        {
            dataHolder.OnFirstLoginToday += popUpDisplayer.ShowDaily;
            dataHolder.OnDailyCoinsUpdate += popUpDisplayer.UpdateDailyCoins;
            heartCounter.OnCounterUpdated += popUpDisplayer.UpdateCountdownInfo;
            heartCounter.OnHealthsUpdated += popUpDisplayer.UpdateHealthsInfo;
        }

        private void OnDisable()
        {
            dataHolder.OnFirstLoginToday -= popUpDisplayer.ShowDaily;
            dataHolder.OnDailyCoinsUpdate -= popUpDisplayer.UpdateDailyCoins;
            heartCounter.OnCounterUpdated -= popUpDisplayer.UpdateCountdownInfo;
            heartCounter.OnHealthsUpdated -= popUpDisplayer.UpdateHealthsInfo;
        }
    }   
}
