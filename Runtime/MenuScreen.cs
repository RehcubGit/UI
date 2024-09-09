using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.EventSystems;
#endif

namespace Rehcub.UI
{
    public class MenuScreen : MonoBehaviour
    {

        #region Editor Functions
#if UNITY_EDITOR

        [MenuItem("GameObject/UI/Menu/Empty")]
        public static void AddMenuScreen()
        {
            GameObject menu = AddMenu();

            menu.AddComponent<MenuScreen>();
        }

        private static GameObject CreateMenuScreen()
        {
            GameObject obj = new GameObject
            {
                name = "Menu"
            };

            if (FindObjectOfType(typeof(MenuManager)) == null)
            {
                GameObject managerObj = new GameObject
                {
                    name = "MenuManager"
                };
                managerObj.AddComponent<MenuManager>();
            }

            return obj;
        }

        public static GameObject AddMenu()
        {
            GameObject menu;
            GameObject parent = Selection.activeGameObject;
            bool addWithCanvas = parent == null || (parent != null && parent.GetComponentInParent<Canvas>() == null);
            if (addWithCanvas)
            {
                menu = AddOutsideCanvas();
            }
            else
            {
                if (parent.GetComponentInParent<MenuScreen>() != null)
                {
                    Debug.LogError("A MenuScreen can not be inside another MenuScreen!");
                    return null;
                }

                menu = AddInsideCanvas();
            }

            if (parent != null)
                menu.transform.SetParent(Selection.activeGameObject.transform, false);
            Selection.activeGameObject = menu;

            return menu;
        }

        private static GameObject AddInsideCanvas()
        {
            GameObject menu = CreateMenuScreen();
            RectTransform rectTransform = menu.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return menu;
        }

        private static GameObject AddOutsideCanvas()
        {
            GameObject menu = CreateMenuScreen();
            menu.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            menu.AddComponent<CanvasScaler>();
            menu.AddComponent<GraphicRaycaster>();

            if (FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject
                {
                    name = "EventSystem"
                };
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }
            return menu;
        }
#endif
        #endregion

        public enum TransionMode
        {
            None, Activate, Fade, UIToolkit
        }

        [SerializeField] private TransionMode _transionMode;
        [SerializeField] private Selectable _firstSelected;
        public bool IsEndScreen { get => _isEndScreen; }
        [Tooltip("When set you can not go further in the menu tree, you can only go back or switch to another")]
        [SerializeField] private bool _isEndScreen;

        public UnityEvent OnInitialize;
        public UnityEvent OnSelect;
        public UnityEvent OnDeselect;

        private bool _isInitialized;

        public bool IsSelected { get => _isSelected; }
        private bool _isSelected;

        [HideInInspector][SerializeField] private VisualElement _root;
        
        protected virtual void OnValidate()
        {
            if(TryGetComponent(out UIDocument document))
            {
                _root = document.rootVisualElement;
                _transionMode = TransionMode.UIToolkit;
            }
            else if(_transionMode == TransionMode.UIToolkit)
            {
                _root = null;
                _transionMode = TransionMode.None;
            }
        }

        protected virtual void Awake()
        {
            if (_isSelected)
                return;

            switch (_transionMode)
            {
                case TransionMode.None:
                    break;
                case TransionMode.Activate:
                    gameObject.SetActive(false);
                    break;
                case TransionMode.Fade:
                    break;
                case TransionMode.UIToolkit:
                    _root.style.display = DisplayStyle.None;
                    break;
                default:
                    break;
            }
        }

        public virtual void Initialize()
        {
            _isInitialized = true;
            OnInitialize?.Invoke();
            if (_firstSelected == null)
                Debug.LogWarning($"The menuscreen {gameObject.name} has not defined a first selected.");
        }

        public virtual void Select()
        {
            if (_isInitialized == false)
                Initialize();

            _isSelected = true;
            switch (_transionMode)
            {
                case TransionMode.None:
                    break;
                case TransionMode.Activate:
                    gameObject.SetActive(true);
                    break;
                case TransionMode.Fade:
                    break;
                case TransionMode.UIToolkit:
                    _root.style.display = DisplayStyle.Flex;
                    break;
                default:
                    break;
            }
            OnSelect?.Invoke();

            if(_firstSelected != null)
            {
                _firstSelected.Select();
            }
        }

        public virtual void Deselect()
        {
            _isSelected = false;
            switch (_transionMode)
            {
                case TransionMode.None:
                    break;
                case TransionMode.Activate:
                    gameObject.SetActive(false);
                    break;
                case TransionMode.Fade:
                    break;
                case TransionMode.UIToolkit:
                    _root.style.display = DisplayStyle.None;
                    break;
                default:
                    break;
            }
            OnDeselect?.Invoke();
        }

        public virtual void Destroy() { }
    }
}