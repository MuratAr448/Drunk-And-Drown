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

        if (_scoreTextLabel != null && worldPosition.HasValue)
        {
            UIAnimationUtils.StartFlyingText(
                this,
                _scoreTextLabel,
                finalScore,
                worldPosition.Value,
                Color.white,
                () => {
                    _totalScore += finalScore;
                    UpdateScoreUI();
                    _bounceRoutine = UIAnimationUtils.StartTextBounce(this, _scoreTextLabel, _bounceRoutine);
                }
            );
        }
        else
        {
            _totalScore += finalScore;
            UpdateScoreUI();
            if (_scoreTextLabel != null)
            {
                _bounceRoutine = UIAnimationUtils.StartTextBounce(this, _scoreTextLabel, _bounceRoutine);
            }
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