using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    public int _totalScore = 0;
    [SerializeField] private TextMeshProUGUI _scoreTextLabel;
    [SerializeField] private float _scoreMultiplier = 1.0f;

    public float ScoreMultiplier
    {
        get => _scoreMultiplier;
        set => _scoreMultiplier = value;
    }

    public void AddScoreMultiplier(float amount)
    {
        _scoreMultiplier += amount;
    }

    private Coroutine _bounceRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        Enemy.GlobalHealthMultiplier = 1.0f;
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int baseScoreAmount, Vector3? worldPosition = null)
    {
        float multiplier = _scoreMultiplier;

        if (ComboSystem.Instance != null)
        {
            multiplier *= ComboSystem.Instance.CurrentMultiplier;
        }

        int finalScore = Mathf.RoundToInt(baseScoreAmount * multiplier);

        if (finalScore <= 0) return;

        _totalScore += finalScore;
        UpdateScoreUI();

        if (_scoreTextLabel != null)
        {
            float intensityMultiplier = Mathf.Clamp(1f + (finalScore - 10) * 0.02f, 1f, 2.5f);
            _bounceRoutine = UIAnimationUtils.StartTextBounce(this, _scoreTextLabel, _bounceRoutine, intensityMultiplier);
        }
    }

    public void UpdateScoreUI()
    {
        if (_scoreTextLabel != null)
        {
            _scoreTextLabel.text = UIUtils.FormatNumber(_totalScore);
        }
    }
}