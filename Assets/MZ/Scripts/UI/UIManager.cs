using System;
using UnityEngine;
using UnityEngine.UI;

namespace MZ.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject initPanel;
        [SerializeField] private GameObject controlPanel;
        [SerializeField] private GameObject simPanel;
        
        [Header("Buttons")]
        [SerializeField] private Button loadSimButton;
        [SerializeField] private Button newSimButton;
        [SerializeField] private Button startSimButton;
        [SerializeField] private Button saveSimButton;

        public event Action StartButtonPressed;
        public event Action LoadButtonPressed;
        public event Action SaveButtonPressed;
        
        private enum GameState
        {
            Init = 0,
            Control = 1,
            Sim = 2
        }

        private GameState _currentState;
        private GameState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                UpdateState();
            }
        }

        private void Awake()
        {
            InitButtons();
            
            CurrentState = GameState.Init;
        }

        private void InitButtons()
        {
            loadSimButton.onClick.AddListener(OnLoadSimButton);
            newSimButton.onClick.AddListener(OnNewSimButton);
            startSimButton.onClick.AddListener(OnStartSimButton);
            saveSimButton.onClick.AddListener(OnSaveSimButton);
        }

        #region ButtonsRegion

        private void OnSaveSimButton()
        {
            SaveButtonPressed?.Invoke();
        }
        
        private void OnStartSimButton()
        {
            StartButtonPressed?.Invoke();
        }
        
        private void OnNewSimButton()
        {
            CurrentState = GameState.Control;
        }

        private void OnLoadSimButton()
        {
            LoadButtonPressed?.Invoke();
        }

        #endregion

        private void UpdateState()
        {
            initPanel.SetActive(CurrentState == GameState.Init);
            controlPanel.SetActive(CurrentState == GameState.Control);
            simPanel.SetActive(CurrentState == GameState.Sim);
        }

        public void ShowSimState()
        {
            CurrentState = GameState.Sim;
        }
    }
}
