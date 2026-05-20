using TMPro;
using UnityEngine;

public class SpeedrunTimer : MonoBehaviour
{
    public static SpeedrunTimer Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TextMeshProUGUI _timerText;

    private float _elapsedTime = 0f;
    private bool _isTimerRunning = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (_isTimerRunning)
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        if (_timerText == null) return;

        int minutes = Mathf.FloorToInt(_elapsedTime / 60F);
        int seconds = Mathf.FloorToInt(_elapsedTime % 60F);

        _timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void StopTimer()
    {
        _isTimerRunning = false;
    }

    public void StartTimer()
    {
        _isTimerRunning = true;
    }

    public void ResetTimer()
    {
        _elapsedTime = 0f;
        UpdateTimerUI();
    }
}