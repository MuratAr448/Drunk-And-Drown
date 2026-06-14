using TMPro;
using UnityEngine;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    [SerializeField] private int _totalScore = 0;
    [SerializeField] private TextMeshProUGUI _scoreTextLabel;

    private Coroutine _bounceRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        UpdateScoreUI();
    }

    public void AddScore(int baseScoreAmount, Vector3? worldPosition = null)
    {
        float multiplier = 1f;

        if (ComboSystem.Instance != null)
        {
            multiplier = ComboSystem.Instance.CurrentMultiplier;
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
            _scoreTextLabel.text = _totalScore.ToString();
        }
    }
}