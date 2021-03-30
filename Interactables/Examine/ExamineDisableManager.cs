using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.ImageEffects;

namespace ExamineSystem
{
    public class ExamineDisableManager : MonoBehaviour
    {
        public static ExamineDisableManager instance;

        [SerializeField] private Image crosshair = null;
        [SerializeField] private FirstPersonController player;
        [SerializeField] private ExamineRaycast raycastManager = null;
        [SerializeField] private BlurOptimized blur = null;

        void Awake()
        {
            if (instance != null) { Destroy(gameObject); }
            else { instance = this; DontDestroyOnLoad(gameObject); }
        }

        public void DisablePlayer(bool disable)
        {
            if (disable)
            {
                raycastManager.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                blur.enabled = true;
                crosshair.enabled = false;
                player.enabled = false;
            }
            else
            {
                raycastManager.enabled = true;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                blur.enabled = false;
                crosshair.enabled = true;
                player.enabled = true;
            }
        }
    }
}
