using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MZ.UI
{
    public class ControlObject : MonoBehaviour
    {
        [SerializeField] private Slider controlSlider;
        [SerializeField] private TextMeshProUGUI controlText;
        [SerializeField] private Vector2Int minMaxField;
        [SerializeField] private ControlObject dependantFromControl;
        [SerializeField] private int defaultValue;

        public event Action<int> OnValueChanged;

        private int _currentValue;
        public int CurrentValue
        {
            get => _currentValue;
            private set
            {
                _currentValue = value;
                UpdateText();
            }
        }

        private void Awake()
        {
            if (dependantFromControl != null)
            {
                dependantFromControl.OnValueChanged += OnOtherSliderValueChanged;
            }
            
            CurrentValue = defaultValue;
            
            controlSlider.minValue = minMaxField.x;
            controlSlider.maxValue = minMaxField.y;
            
            controlSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnSliderValueChanged(float val)
        {
            int intVal = (int)val;
            CurrentValue = intVal;
            
            OnValueChanged?.Invoke(intVal);
        }
        
        private void OnOtherSliderValueChanged(int val)
        {
            int needValue = val * val / 2;
            
            if (needValue < CurrentValue)
            {
                CurrentValue = needValue;
            }
            
            controlSlider.maxValue = needValue;
        }

        private void UpdateText()
        {
            controlText.text = $"{CurrentValue}";
        }
    }
}