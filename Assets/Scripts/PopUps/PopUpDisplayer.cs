using UnityEngine;
using DG.Tweening;
using System;
using TMPro;
using AnotherMatch3.Hearts;

namespace AnotherMatch3.PopUps
{
    public class PopUpDisplayer : MonoBehaviour
    {
        [SerializeField] private CanvasReferences canvasReferences;
        [SerializeField] private TextReferences textReferences;
        [SerializeField] private PopUpSettings popUpSettings;

        private Sequence sequence = null;

        public void Show(CanvasGroup canvasGroup) => Show(canvasGroup, true, popUpSettings.grayBackgroundDuration, popUpSettings.popUpShowDuration, popUpSettings.popUpShowAnimation);

        public void Hide(CanvasGroup canvasGroup) => Show(canvasGroup, false, popUpSettings.grayBackgroundDuration, popUpSettings.popUpShowDuration, popUpSettings.popUpShowAnimation);

        public void ShowDaily() => Show(canvasReferences.dailyBonusCanvas, true);

        public void ShowPopUpBasedOnHearts(HeartCounter heartCounter) => ShowPopUpBasedOnHearts(heartCounter.GetCurrentHealths(), heartCounter.GetMaxHealths());

        public void ShowPopUpBasedOnHearts(int currentHealths, int maxHealths)
        {
            if (currentHealths <= 0)
            {
                Show(canvasReferences.noHealthsCanvas, true);
                return;
            }

            var canvasToShow = currentHealths >= maxHealths ? canvasReferences.maxHealthsCanvas : canvasReferences.regulardHealthsCanvas;
            Show(canvasToShow, true);
        }

        public void Show(CanvasGroup canvasGroup, bool state, 
        float grayBackgroundDuration = 1f, float canvasPopUpDuration = 1f, Ease canvasPopUpEasing = Ease.InOutSine)
        {
            sequence = DOTween.Sequence().OnComplete(() => sequence = null);
            sequence.Append(ShowBackground(state, grayBackgroundDuration));
            sequence.Append(ShowCanvas(canvasGroup, state, canvasPopUpDuration, canvasPopUpEasing));
            sequence.Play();
        }

        public Tweener ShowBackground(bool state, float duration)
        {
            var value = state ? 1 : 0;
            canvasReferences.grayBackgroundCanvas.interactable = state;
            canvasReferences.grayBackgroundCanvas.blocksRaycasts = state;
            return canvasReferences.grayBackgroundCanvas.DOFade(value, duration);
        }

        private Tweener ShowCanvas(CanvasGroup canvasGroup, bool state, float duration, Ease easing)
        {
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;

            var value = state ? Vector3.one : Vector3.zero;
            return canvasGroup.transform
            .DOScale(value, duration)
            .SetEase(easing);
        }

        public void UpdateCountdownInfo(string intervalCounter)
        {
            foreach (var item in textReferences.nextHeartCountdowns)
            {
                item.text = intervalCounter;
            }
        }

        public void UpdateDailyCoins(int coins) => textReferences.coinsText.text = coins.ToString();

        public void UpdateHealthsInfo(int healths)
        {
            foreach (var item in textReferences.heartsTexts)
            {
                item.text = healths.ToString();
            }
        }

        [Serializable]
        private class TextReferences
        {
            [SerializeField] internal TMP_Text[] heartsTexts;
            [SerializeField] internal TMP_Text[] nextHeartCountdowns;
            [SerializeField] internal TMP_Text coinsText;
        }

        [Serializable]
        private class PopUpSettings
        {
            [SerializeField] internal float popUpShowDuration = 0.65f;
            [SerializeField] internal float grayBackgroundDuration = 1f;
            [SerializeField] internal Ease popUpShowAnimation = Ease.InOutSine;
        }

        [Serializable]
        private class CanvasReferences
        {
            [SerializeField] internal CanvasGroup grayBackgroundCanvas;
            [SerializeField] internal CanvasGroup dailyBonusCanvas;
            [SerializeField] internal CanvasGroup noHealthsCanvas;
            [SerializeField] internal CanvasGroup maxHealthsCanvas;
            [SerializeField] internal CanvasGroup regulardHealthsCanvas;
        }
    } 
}