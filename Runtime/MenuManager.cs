using Rehcub.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace Rehcub.UI
{
    public class MenuManager : Singleton<MenuManager>
    {
        public MenuScreen CurrentScreen { get => _currentScreen; }
        private MenuScreen _currentScreen;
        [SerializeField] private MenuScreen _startScreen;

        [Tooltip("If the MenuManager should use the cancle Action from the EventSystem to go back.")]
        [SerializeField] private bool _backWithCancleAction = true;
        [SerializeField] private InputSystemUIInputModule _module;

        private Stack<MenuScreen> _previewsScreens = new Stack<MenuScreen>();
        private List<MenuScreen> _availableScreens = new List<MenuScreen>();

        public event Func<bool> InterceptBack;

        private void Start()
        {
            _availableScreens.AddRange(FindObjectsOfType<MenuScreen>(true));

            if(_availableScreens.Count == 0)
            {
                Debug.LogError("No Screens in the Scene, destroying MenuManager!");
                Destroy(gameObject);
                return;
            }

            foreach (MenuScreen menuScreen in _availableScreens)
            {
                menuScreen.Initialize();
            }

            /*if (TryGetComponent(out MenuAudioHandler audioHandler))
                audioHandler.SetupAudio(_availableScreens);*/

            if (_startScreen == null)
            {
                Debug.LogError("No Start Screen is defined!");
                _startScreen = _availableScreens[0];
            }

            if(_startScreen.gameObject.activeSelf == false)
                Debug.LogWarning("The Start Screen is not activ!");

            SetCurrentScreen(_startScreen);
        }

        private void OnEnable()
        {

            if (_backWithCancleAction == false)
                return;

            if (_module == null)
            {
                BaseInputModule baseModule = EventSystem.current.currentInputModule;
                if (baseModule is not InputSystemUIInputModule inputModule)
                    return;

                _module = inputModule;
            }

            _module.cancel.action.performed += Trigger;
        }

        private void OnDisable()
        {
            if (_backWithCancleAction == false)
                return;

            if (_module == null)
                return;

            _module.cancel.action.performed -= Trigger;
        }

        private void OnDestroy()
        {
            foreach (MenuScreen menuScreen in _availableScreens)
                menuScreen.Destroy();
        }

        private void Trigger(InputAction.CallbackContext context)
        {
            bool? intercepted = InterceptBack?.Invoke();

            if (intercepted.HasValue && intercepted.Value)
                return;

            if(context.started)
                Back();
        }

        public void SetCurrentScreen(MenuScreen menuScreen)
        {
            if (_currentScreen != null)
            {
                if (_currentScreen.IsEndScreen)
                    return;
                _previewsScreens.Push(_currentScreen);
                _currentScreen.Deselect();
            }
            _currentScreen = menuScreen;
            _currentScreen.Select();
        }

        public void SwitchTo(MenuScreen menuScreen)
        {
            if (_currentScreen != null)
                _currentScreen.Deselect();

            _currentScreen = menuScreen;
            _currentScreen.Select();
        }

        public void Back()
        {
            if (_previewsScreens.Count == 0)
                return;

            MenuScreen lastScreen = _currentScreen;
            _currentScreen = _previewsScreens.Pop();

            if (lastScreen != null)
                lastScreen.Deselect();

            _currentScreen.Select();
        }

        public void BackAll()
        {
            while (_previewsScreens.Count > 0)
                Back();
        }

        public T GetScreenByType<T>() where T : MenuScreen
        {
            foreach (MenuScreen screen in _availableScreens)
            {
                if (screen is T t)
                    return t;
            }
            Debug.LogError($"No Screen of type {typeof(T)} found!");
            return null;
        }

        public bool IsPreviewsScreen(MenuScreen menuScreen) => _previewsScreens.Contains(menuScreen);

        public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
        public void LoadScene(int index) => SceneManager.LoadScene(index);
        public void ReloadCurrentScene() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        public void QuitGame() => Application.Quit();
    }
}