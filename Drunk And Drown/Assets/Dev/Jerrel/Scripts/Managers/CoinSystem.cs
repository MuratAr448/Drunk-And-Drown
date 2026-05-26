using TMPro;
using UnityEngine;

public class CoinSystem : MonoBehaviour
{
    [SerializeField] private int _coins;
    [SerializeField] private float _coinMultiplier = 1.0f;

    [SerializeField] private TextMeshProUGUI _coinTextLabel;

    public static CoinSystem Instance;


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
        UpdateUI();
    }
    public void AddCoins(int amount)
    {
        _coins += Mathf.RoundToInt(amount * _coinMultiplier);
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
        _coinTextLabel.text = _coins.ToString();
    }
}
