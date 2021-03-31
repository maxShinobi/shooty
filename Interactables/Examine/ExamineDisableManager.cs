using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.ImageEffects;
using VHS;

namespace ExamineSystem
{
    public class ExamineDisableManager : MonoBehaviour
    {
        public static ExamineDisableManager instance;

        [SerializeField] private Image crosshair = null;
        [SerializeField] private ExamineRaycast raycastManager = null;
        [SerializeField] private BlurOptimized blur = null;

        void Awake()
        {
            if (instance != null) { Destroy(gameObject); }
            else { instance = this; DontDestroyOnLoad(gameObject); }
        }

        public void DisablePlayer(bool disable)
        {
                raycastManager.enabled = !disable;
                Cursor.lockState = disable? CursorLockMode.None :CursorLockMode.Locked;
                Cursor.visible = disable;
                blur.enabled = disable;
                crosshair.enabled = !disable;
                FirstPersonController.instance.DisableFPC(!disable);
        }
    }
}
