using TMPro;
using UnityEngine;

public class CoinSystem : MonoBehaviour
{
    [SerializeField] private int _coins;
    [SerializeField] private float _coinMultiplier = 1.0f;

    [SerializeField] private TextMeshProUGUI _coinTextLabel;

    public static CoinSystem Instance;

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
        UpdateUI();
    }

    private void Update()
    {
        // Keep UI updated in case of other changes
        UpdateUI();
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
