using System;
using MZ.Field;
using MZ.UI;
using UnityEngine;

namespace MZ.Sim
{
    public class SimManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private UIManager uiManager;
        [SerializeField] private FieldManager fieldManager;

        [Header("Controls")]
        [SerializeField] private ControlObject fieldLengthControl;
        [SerializeField] private ControlObject animalsCountControl;
        [SerializeField] private ControlObject animalSpeedControl;
        [SerializeField] private ControlObject simSpeedControl;

        private string SavedSimString
        {
            get => PlayerPrefs.GetString("SavedSimString", string.Empty);
            set => PlayerPrefs.SetString("SavedSimString", value);
        }

        private bool _simInProcess = false;

        private SimParams _currentParams;

        private void Awake()
        {
            uiManager.StartButtonPressed += UiManagerOnStartButtonPressed;
            uiManager.LoadButtonPressed += UiManagerOnLoadButtonPressed;
            uiManager.SaveButtonPressed += UiManagerOnSaveButtonPressed;

            simSpeedControl.OnValueChanged += SimSpeedControlOnValueChanged;
        }

        private void OnDestroy()
        {
            uiManager.StartButtonPressed -= UiManagerOnStartButtonPressed;
            uiManager.LoadButtonPressed -= UiManagerOnLoadButtonPressed;
            uiManager.SaveButtonPressed -= UiManagerOnSaveButtonPressed;

            simSpeedControl.OnValueChanged -= SimSpeedControlOnValueChanged;
        }

        private void SimSpeedControlOnValueChanged(int speedValue)
        {
            fieldManager.SetSimSpeed(speedValue);
        }

        #region ButtonsRegion

        private void UiManagerOnSaveButtonPressed()
        {
            SaveSim();
        }

        private void UiManagerOnLoadButtonPressed()
        {
            if (_simInProcess) return;
            if (string.IsNullOrEmpty(SavedSimString)) return;

            _simInProcess = true;

            uiManager.ShowSimState();
            LoadSim();
        }

        private void UiManagerOnStartButtonPressed()
        {
            if (_simInProcess) return;
            _simInProcess = true;

            uiManager.ShowSimState();
            NewSim();
        }

        #endregion

        private void NewSim()
        {
            _currentParams = new SimParams(fieldLengthControl.CurrentValue, animalsCountControl.CurrentValue,
                animalSpeedControl.CurrentValue);
            fieldManager.SetSimSpeed(simSpeedControl.CurrentValue);
            fieldManager.InitField(_currentParams);
        }

        private void SaveSim()
        {
            fieldManager.GetState(out var animals, out var feeds);
            _currentParams.Animals = animals;
            _currentParams.Feeds = feeds;
            SavedSimString = JsonUtility.ToJson(_currentParams);
        }

        private void LoadSim()
        {
            _currentParams = JsonUtility.FromJson<SimParams>(SavedSimString);
            fieldManager.SetSimSpeed(simSpeedControl.CurrentValue);
            fieldManager.InitField(_currentParams);
        }
    }
}
