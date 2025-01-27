using UnityEngine;
using UnityEngine.UI;

namespace Src
{
    public abstract class SimulationBase : MonoBehaviour
    {
        [Header("Time parameters")]
        [SerializeField] protected float simulationTime;
        [SerializeField] protected int samplesCount;
        [Header("Controls")]
        [SerializeField] private Slider timeSlider;
        [SerializeField] private Button playAsCoroutineButton;

        protected float TimeStep => simulationTime / samplesCount;
        
        protected virtual void Start()
        {
            timeSlider.minValue = 0;
            timeSlider.maxValue = simulationTime;
            timeSlider.value = 0;
            
            timeSlider.onValueChanged.AddListener(OnTimeSliderValueChanged);
            playAsCoroutineButton.onClick.AddListener(PlayAsCoroutine);
        }

        private void PlayAsCoroutine()
        {
            StartCoroutine(PlaySimulation());
        }
        
        private System.Collections.IEnumerator PlaySimulation()
        {
            timeSlider.interactable = false;
            playAsCoroutineButton.interactable = false;
            timeSlider.value = 0;
            while (timeSlider.value < simulationTime)
            {
                timeSlider.value += Time.deltaTime;
                yield return null;
            }
            timeSlider.interactable = true;
            playAsCoroutineButton.interactable = true;
        }

        private void OnTimeSliderValueChanged(float value)
        {
            var stateIndex = Mathf.RoundToInt(value / TimeStep);
            stateIndex = Mathf.Clamp(stateIndex, 0, samplesCount - 1);
            SetSimulationState(stateIndex);
        }

        protected abstract void SetSimulationState(int stateIndex);
    }
}