using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboSystem : MonoBehaviour
{
    public static ComboSystem Instance { get; private set; }

    [Header("Combo Settings")]
    [SerializeField] private float _comboDuration = 3.5f;
    [SerializeField] private int _maxCombo = 10;

    [Header("Gradient Colors")]
    [SerializeField] private Gradient _comboGradient;

    [Header("References")]
    [SerializeField] private Image _comboBar;
    [SerializeField] private TextMeshProUGUI _multiplierText;

    private int _currentCombo = 0;
    private float _comboTimer = 0f;
    private float _currentMultiplier = 1f;

    public float CurrentMultiplier => _currentMultiplier;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (_comboTimer > 0)
        {
            _comboTimer -= Time.deltaTime;

            if (_comboBar != null)
            {
                _comboBar.fillAmount = _comboTimer / _comboDuration;
            }

            if (_comboTimer <= 0)
            {
                ResetCombo();
            }
        }
    }

    public void OnEnemyKilled()
    {
        if (_currentCombo < _maxCombo)
        {
            _currentCombo++;
            _currentMultiplier = 1f + ((_currentCombo - 1) * 0.5f);
        }

        _comboTimer = _comboDuration;

        if (_comboBar != null)
        {
            _comboBar.fillAmount = 1f;
        }

        UpdateUI();
    }

    private void ResetCombo()
    {
        _currentCombo = 0;
        _comboTimer = 0f;
        _currentMultiplier = 1f;

        if (_comboBar != null)
        {
            _comboBar.fillAmount = 0f;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        float progress = (float)_currentCombo / _maxCombo;
        Color currentColor = _comboGradient.Evaluate(progress);

        if (_multiplierText != null)
        {
            _multiplierText.text = _currentMultiplier.ToString("F1") + "x";
            _multiplierText.color = currentColor;
        }

        if (_comboBar != null)
        {
            _comboBar.color = currentColor;
        }
    }
}