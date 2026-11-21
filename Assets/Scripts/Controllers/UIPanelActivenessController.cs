using System.Collections.Generic;
using Controllers.UI;
using Enums;
using UnityEngine;

namespace Controllers
{
    public class UIPanelActivenessController : MonoBehaviour
    {
        #region Self Variables

        #region Serialized Variables

        [SerializeField] private List<CanvasGroup> panels;

        #endregion

        #endregion

        public void RegisterPanel(UIPanels panelType, CanvasGroup canvasGroup)
        {
            if (canvasGroup == null)
            {
                return;
            }

            if (panels == null)
            {
                panels = new List<CanvasGroup>();
            }

            int index = (int)panelType;
            while (panels.Count <= index)
            {
                panels.Add(null);
            }

            panels[index] = canvasGroup;
        }

        public void OpenMenu(UIPanels storeMenu)
        {
            int index = (int)storeMenu;
            panels[index].GetComponent<UIPanelAnimator>().AnimateOpen();
        }

        public void CloseMenu(UIPanels storeMenu)
        {
            int index = (int)storeMenu;
            panels[index].GetComponent<UIPanelAnimator>().AnimateClose();
        }
    }
}
