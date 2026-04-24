using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Maneja la visualización de vida mediante sprites de corazones,
/// al estilo de Enter The Gungeon.
///
/// Configuración esperada:
/// - 3 corazones en el inspector (izquierdo, central, derecho)
/// - El corazón central tiene "flipped = true" para invertirlo
/// - Cada corazón vale 2 HP: lleno=2, mitad=1, vacío=0
/// - maxHP default: 6 (3 corazones x 2 HP)
/// </summary>
public class HeartHealthUI : MonoBehaviour
{
    [System.Serializable]
    public struct HeartSlot
    {
        public Image heartImage;
        [Tooltip("El corazón del centro va invertido horizontalmente (como en ETG).")]
        public bool flipped;
    }

    [Header("Sprites")]
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartHalf;
    [SerializeField] private Sprite heartEmpty;

    [Header("Corazones (izq → centro → der)")]
    [SerializeField] private HeartSlot[] hearts;

    private void Awake()
    {
        ApplyFlips();
    }

    /// <summary>
    /// Actualiza los sprites según el HP actual.
    /// Llamar desde PlayerHealth cada vez que cambie la vida.
    /// </summary>
    public void UpdateHearts(int currentHP)
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i].heartImage == null) continue;

            // HP que "pertenece" a este corazón
            int hpForSlot = currentHP - (i * 2);

            if (hpForSlot >= 2)
                hearts[i].heartImage.sprite = heartFull;
            else if (hpForSlot == 1)
                hearts[i].heartImage.sprite = heartHalf;
            else
                hearts[i].heartImage.sprite = heartEmpty;
        }
    }

    // Aplica el flip horizontal al corazón central via RectTransform
    private void ApplyFlips()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i].heartImage == null) continue;

            Vector3 scale = hearts[i].heartImage.rectTransform.localScale;
            scale.x = hearts[i].flipped ? -1f : 1f;
            hearts[i].heartImage.rectTransform.localScale = scale;
        }
    }
}