using UnityEngine;
using UnityEngine.UI;

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