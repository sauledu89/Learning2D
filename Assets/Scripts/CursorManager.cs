using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Oculta el cursor del SO y dibuja un cursor personalizado
/// que sigue la posición del mouse.
/// Agregar a un GameObject persistente en la escena de juego.
/// </summary>
public class CursorManager : MonoBehaviour
{
    [Header("Cursor personalizado")]
    [SerializeField] private RectTransform cursorImage;
    [SerializeField] private Canvas cursorCanvas;

    [Header("Offset (ajuste visual)")]
    [Tooltip("Desplazamiento en píxeles para centrar el sprite sobre el puntero real.")]
    [SerializeField] private Vector2 hotspotOffset = Vector2.zero;

    private void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    private void Update()
    {
        if (Mouse.current == null || cursorImage == null || cursorCanvas == null) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            cursorCanvas.GetComponent<RectTransform>(),
            mouseScreen,
            cursorCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : cursorCanvas.worldCamera,
            out Vector2 localPos
        );

        cursorImage.anchoredPosition = localPos + hotspotOffset;
    }

    private void OnDisable()
    {
        // Restaurar cursor del SO si el script se desactiva (ej: menú)
        Cursor.visible = true;
    }
}