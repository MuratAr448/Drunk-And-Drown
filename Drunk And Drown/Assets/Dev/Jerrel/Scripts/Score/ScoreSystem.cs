using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    [SerializeField] private int _totalScore = 0;
    [SerializeField] private TextMeshProUGUI _scoreTextLabel;

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

    public void AddScore(int baseScoreAmount)
    {
        float multiplier = 1f;

        if (ComboSystem.Instance != null)
        {
            multiplier = ComboSystem.Instance.CurrentMultiplier;
        }

        _totalScore += Mathf.RoundToInt(baseScoreAmount * multiplier);
        UpdateScoreUI();
    }

    public void UpdateScoreUI()
    {
        if (_scoreTextLabel != null)
        {
            _scoreTextLabel.text = _totalScore.ToString();
        }
    }
}