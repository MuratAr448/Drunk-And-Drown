using TMPro;
using UnityEngine;

public class CoinSystem : MonoBehaviour
{
    [SerializeField] private int _coins;
    [SerializeField] private float _coinMultiplier = 1.0f;

    [SerializeField] private TextMeshProUGUI _coinTextLabel;

    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _visibleDuration = 3f;
    [SerializeField] private float _fadeDuration = 0.5f;

    public static CoinSystem Instance;

    private Coroutine _bounceRoutine;
    private float _visibleTimer = 0f;
    private ShopManager _shopManager;
    private int _lastCoins;

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
        _lastCoins = _coins;
        UpdateUI();

        // Try to automatically find or add a CanvasGroup on parent/text
        if (_canvasGroup == null)
        {
            if (_coinTextLabel != null)
            {
                Transform parent = _coinTextLabel.transform.parent;
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
                    _canvasGroup = _coinTextLabel.GetComponent<CanvasGroup>();
                    if (_canvasGroup == null)
                    {
                        _canvasGroup = _coinTextLabel.gameObject.AddComponent<CanvasGroup>();
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
        _visibleTimer = 0f;
    }

    private void Update()
    {
        // Keep UI updated in case of other changes
        UpdateUI();

        // Detect any change in coin amount to trigger visibility
        if (_coins != _lastCoins)
        {
            _lastCoins = _coins;
            _visibleTimer = _visibleDuration;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
            }
        }

        UpdateFading();
    }

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
            _canvasGroup.alpha = 1f;
            _visibleTimer = _visibleDuration; // Hold visible timer so it remains visible briefly after closing shop
        }
        else
        {
            if (_visibleTimer > 0f)
            {
                _visibleTimer -= Time.deltaTime;
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
    }

    public void AddCoins(int amount, Vector3? worldPosition = null)
    {
        int finalAmount = amount;
        if (amount > 0)
        {
            finalAmount = Mathf.RoundToInt(amount * _coinMultiplier);
        }

        if (finalAmount == 0) return;

        // Play flying animation if gaining coins and we have a world position
        if (finalAmount > 0 && _coinTextLabel != null && worldPosition.HasValue)
        {
            UIAnimationUtils.StartFlyingText(
                this,
                _coinTextLabel,
                finalAmount,
                worldPosition.Value,
                new Color(1f, 0.82f, 0f), // Gold color
                () => {
                    _coins += finalAmount;
                    UpdateUI();
                    _bounceRoutine = UIAnimationUtils.StartTextBounce(this, _coinTextLabel, _bounceRoutine);
                }
            );
        }
        else
        {
            _coins += finalAmount;
            UpdateUI();
            if (finalAmount != 0 && _coinTextLabel != null)
            {
                _bounceRoutine = UIAnimationUtils.StartTextBounce(this, _coinTextLabel, _bounceRoutine);
            }
        }
    }

    public float CoinMultiplier => _coinMultiplier;

    /// <summary>
    /// Base coin multiplier is 1x. To increase add to amount, to decrease make amount negative.
    /// </summary>
    /// <param name="amount"></param>
    public void AddCoinMultiplier(float amount)
    {
        _coinMultiplier += amount;
    }

    public int GetCoinAmount()
    {
        return _coins;
    }

    private void UpdateUI()
    {
        if (_coinTextLabel != null)
        {
            _coinTextLabel.text = _coins.ToString();
        }
    }
}
