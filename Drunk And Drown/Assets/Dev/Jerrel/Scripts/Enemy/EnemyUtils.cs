using UnityEngine;
using TMPro;

public class EnemyUtils : MonoBehaviour
{
    [SerializeField] private int _enemyCount = 0;
    public static EnemyUtils Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

    }
    public void AddEnemy()
    {
        _enemyCount++;
    }

    public void RemoveEnemy()
    {
        _enemyCount--;
    }

    public int GetEnemyCount()
    {
        return _enemyCount;
    }

    public void ResetEnemyCount()
    {
        _enemyCount = 0;
    }
}

public class FloatingDamageText : MonoBehaviour
{
    private TextMeshPro textMesh;
    private float lifetime = 0.8f;
    private float timer = 0f;
    private Vector3 moveDirection;

    public static void Create(Vector3 position, float damageAmount)
    {
        GameObject go = new GameObject("DamageText");
        // Spawn slightly above the enemy's center
        go.transform.position = position + Vector3.up * 1.8f + Random.insideUnitSphere * 0.15f;

        TextMeshPro tm = go.AddComponent<TextMeshPro>();
        tm.text = UIUtils.FormatNumber(damageAmount);
        tm.alignment = TextAlignmentOptions.Center;
        
        // Visual hierarchy based on damage size with high-vibrancy HDR White
        Color faceColor = new Color(2f, 2f, 2f, 1f); // Bright HDR White for every damage number
        if (damageAmount >= 20f)
        {
            tm.fontSize = 18;
        }
        else if (damageAmount >= 10f)
        {
            tm.fontSize = 15;
        }
        else
        {
            tm.fontSize = 13;
        }

        tm.color = Color.white; // Base vertex color (alpha fading works relative to this)

        // Enable black outline for contrast and apply face color
        if (tm.fontMaterial != null)
        {
            Shader overlayShader = Shader.Find("TextMeshPro/Mobile/Distance Field Overlay");
            if (overlayShader == null)
            {
                overlayShader = Shader.Find("TextMeshPro/Distance Field Overlay");
            }
            if (overlayShader != null)
            {
                tm.fontMaterial.shader = overlayShader;
            }
            
            tm.fontMaterial.EnableKeyword("OUTLINE_ON");
            tm.fontMaterial.SetFloat("_OutlineWidth", 0.25f);
            tm.fontMaterial.SetColor("_OutlineColor", Color.black);
            tm.fontMaterial.SetColor("_FaceColor", faceColor);
        }

        FloatingDamageText fdt = go.AddComponent<FloatingDamageText>();
        fdt.textMesh = tm;
        // Float upwards and slightly sideways in a random direction
        fdt.moveDirection = new Vector3(Random.Range(-0.3f, 0.3f), 1f, Random.Range(-0.3f, 0.3f)).normalized;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // Move text
        transform.position += moveDirection * (2f * Time.deltaTime);

        // Face the main camera (perfect billboard plane alignment) and scale with distance for constant screen size
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;

            float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
            // Reference distance of 15 units: at this distance, scale is 1.0f.
            float scale = distance / 15f;
            transform.localScale = Vector3.one * Mathf.Max(scale, 0.1f);
        }

        // Fade color alpha
        if (textMesh != null)
        {
            float alpha = Mathf.Clamp01(1f - (timer / lifetime));
            Color c = textMesh.color;
            c.a = alpha;
            textMesh.color = c;
        }

        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
