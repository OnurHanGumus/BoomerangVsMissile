using System.Collections.Generic;
using DG.Tweening;
using Enums;
using Managers;
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
            if (index >= 0 && index < panels.Count && panels[index] != null)
            {
                panels[index].DOKill();
                panels[index].DOFade(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
                panels[index].blocksRaycasts = true;
                panels[index].interactable = true;
            }
        }
        public void CloseMenu(UIPanels storeMenu)
        {
            int index = (int)storeMenu;
            if (index >= 0 && index < panels.Count && panels[index] != null)
            {
                panels[index].DOKill();
                panels[index].DOFade(0f, 0.5f).SetUpdate(true);
                panels[index].blocksRaycasts = false;
                panels[index].interactable = false;
            }
        }
    }
}
