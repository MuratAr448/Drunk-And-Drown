using UnityEngine;
using UnityEngine.UI;

public class LoadingScreen : MonoBehaviour
{
    private float _progress;
    [SerializeField] private Image _progressBar;

    public float Progress
    {
        get => _progress;
        set
        {
            _progress = value;
            DisplayProgress();
        }
    }

    private void DisplayProgress()
    {
        if (_progressBar != null)
        {
            _progressBar.fillAmount = Mathf.Clamp(_progress, 0f, 100f) / 100f;
        }
    }
}