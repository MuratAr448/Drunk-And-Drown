using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonUtils : MonoBehaviour
{
    public void Navigate(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("You succefully quitted the game");
    }

    public void ToggleUI(GameObject UI)
    {
        UI.SetActive(!UI.activeSelf);
    }

    [Header("Optional Panel List for SwitchToPanel")]
    [SerializeField] private GameObject[] allPanels;

    /// <summary>
    /// Method 1: Disables all panels configured in the 'allPanels' array (except those containing a Button component) and enables only the target UI.
    /// If 'allPanels' is empty, it automatically falls back to disabling siblings under the same parent.
    /// </summary>
    public void SwitchToPanel(GameObject targetUI)
    {
        if (targetUI == null) return;

        if (allPanels != null && allPanels.Length > 0)
        {
            foreach (GameObject panel in allPanels)
            {
                if (panel != null)
                {
                    if (panel == targetUI)
                    {
                        panel.SetActive(true);
                    }
                    else if (panel.GetComponent<Button>() == null)
                    {
                        panel.SetActive(false);
                    }
                }
            }
        }
        else
        {
            SwitchToUIAndDisableSiblings(targetUI);
        }
    }

    /// <summary>
    /// Method 2: Disables all sibling GameObjects under the target UI's parent (except those containing a Button component), and enables only the target UI.
    /// Great for menus where all screens (Main, Settings, Credits) share a parent container.
    /// </summary>
    public void SwitchToUIAndDisableSiblings(GameObject targetUI)
    {
        if (targetUI == null) return;

        Transform parent = targetUI.transform.parent;
        if (parent == null)
        {
            targetUI.SetActive(true);
            return;
        }

        foreach (Transform child in parent)
        {
            GameObject childObj = child.gameObject;
            if (childObj == targetUI)
            {
                childObj.SetActive(true);
            }
            else if (childObj.GetComponent<Button>() == null)
            {
                childObj.SetActive(false);
            }
        }
    }
}
