using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
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

    [Header("Audio Settings")]
    [SerializeField] private List<AudioEvent> _comboSound;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _fadeDuration = 0.5f;

    private int _currentCombo = 0;
    private float _comboTimer = 0f;
    private float _currentMultiplier = 1f;
    private AudioSource _audioSource;

    public float CurrentMultiplier => _currentMultiplier;

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
        _audioSource = GetComponent<AudioSource>();

        // Try to automatically find or add a CanvasGroup on parent/text
        if (_canvasGroup == null)
        {
            if (_multiplierText != null)
            {
                Transform parent = _multiplierText.transform.parent;
                if (parent != null)
                {
                    _canvasGroup = parent.GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = parent.gameObject.AddComponent<CanvasGroup>();
                    }
                }
                else
                {
                    _canvasGroup = _multiplierText.GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = _multiplierText.gameObject.AddComponent<CanvasGroup>();
                    }
                }
            }
            else
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        if (_canvasGroup != null)
        {
            _canvasGroup.alpha = 0f;
        }

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

        UpdateFading();
    }

    private ShopManager _shopManager;

    private void UpdateFading()
    {
        if (_canvasGroup == null) return;

        bool isShopActive = false;
        if (_shopManager == null)
        {
            _shopManager = FindFirstObjectByType<ShopManager>();
        }
        if (_shopManager != null)
        {
            isShopActive = _shopManager.IsShopActive;
        }

        if (isShopActive)
        {
            _canvasGroup.alpha = 0f;
            return;
        }

        if (_currentCombo > 0)
        {
            _canvasGroup.alpha = 1f;
        }
        else
        {
            if (_canvasGroup.alpha > 0f)
            {
                _canvasGroup.alpha -= Time.deltaTime / _fadeDuration;
                if (_canvasGroup.alpha < 0f)
                {
                    _canvasGroup.alpha = 0f;
                }
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
        TriggerTextBounce();

        if (_comboSound != null && _audioSource != null && _currentCombo > 0)
        {
            float pitchMultiplier = Mathf.Pow(1.059463f, _currentCombo - 1);
            _comboSound[(int)_currentMultiplier-1].Play(_audioSource, pitchMultiplier);
        }
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
            _multiplierText.text = UIUtils.FormatNumber(_currentMultiplier) + "x";
            _multiplierText.color = currentColor;
        }

        if (_comboBar != null)
        {
            _comboBar.color = currentColor;
        }
    }

    private Coroutine _bounceRoutine;

    private void TriggerTextBounce()
    {
        if (_multiplierText == null) return;
        if (_bounceRoutine != null)
        {
            StopCoroutine(_bounceRoutine);
        }
        _bounceRoutine = StartCoroutine(BounceTextRoutine());
    }

    private System.Collections.IEnumerator BounceTextRoutine()
    {
        RectTransform rectTransform = _multiplierText.rectTransform;
        rectTransform.localScale = Vector3.one;

        float duration = 0.12f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rectTransform.localScale = Vector3.one * Mathf.Lerp(1f, 1.4f, t);
            yield return null;
        }

        elapsed = 0f;
        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rectTransform.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, t);
            yield return null;
        }

        rectTransform.localScale = Vector3.one;
        _bounceRoutine = null;
    }
}