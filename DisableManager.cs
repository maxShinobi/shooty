using UnityEngine;
//using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.UI;

namespace interactSystem
{
    public class DisableManager : MonoBehaviour
    {
        public static DisableManager instance;

        [SerializeField] private FirstPersonController player = null;
        [SerializeField] private Image crosshair = null; 

        void Awake()
        {
            if (instance != null) { Destroy(gameObject); }
            else { instance = this; DontDestroyOnLoad(gameObject); }
        }

        public void DisablePlayer(bool disable)
        {
            if (disable)
            {
                player.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                crosshair.enabled = false;
            }

            if (!disable)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                player.enabled = true;
                crosshair.enabled = true;
            }
        }
    }
}
