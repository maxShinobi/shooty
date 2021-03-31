using UnityEngine;
using UnityEngine.UI;
using VHS;

namespace ExamineSystem
{
    public class ExamineUIManager : MonoBehaviour
    {
        [HideInInspector] public ExamineItemController examineController;

        public static ExamineUIManager instance;

        private void Awake()
        {
            if (instance == null) { instance = this; }
        }

        public void CloseButton()
        {
            examineController.StopInteractingObject();
        }
        }
    }
