using Enums;
using Signals;
using UnityEngine;

public class SourcesPanelController : MonoBehaviour
{
    public void CloseSourcesPanel()
    {
        UISignals.Instance.onClosePanel?.Invoke(UIPanels.SourcesPanel);
    }

    public void CloseOptionsPanel()
    {
        CloseSourcesPanel();
    }
}
