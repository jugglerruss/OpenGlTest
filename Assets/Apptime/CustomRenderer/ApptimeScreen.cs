
using UnityEngine;
using UnityEngine.UI;

public class ApptimeScreen : MonoBehaviour {
    private float _rectWidth;
    private float _rectHeight;
    private Texture2D _tex;
    private static ApptimeScreen _instance;
    private Vector2 _size;
    [SerializeField] private Texture2D _texture;

    // Start is called before the first frame update
    void Awake() {
        _instance = this;
        var img = GetComponent<Image>();
        var rect = img.rectTransform.rect;
        _rectWidth = rect.width;
        _rectHeight = rect.height;
        _size = new Vector2(_rectWidth, _rectHeight);
        _tex = new Texture2D((int) _rectWidth, (int) _rectHeight);
        var sprite = Sprite.Create(_tex, new Rect(0, 0, _rectWidth, _rectHeight), Vector2.one * .5f);
        img.sprite = sprite;
        Clear();
    }

    public static Vector2 GetScreenSize() {
        return _instance._size;
    }

    public static void SetPixel(int x, int y, Color color) {
        _instance._tex.SetPixel(x, y, color);
    }

    public static void ApplyPixelChanges() {
        _instance._tex.Apply();
    }

    public static void SetPixelFromTo(int x, int y, float ity) {
        var color = _instance._texture.GetPixel(x, y);
        color = new Color(color.r * ity, color.g * ity, color.b * ity);
        _instance._tex.SetPixel(x, y, color);
    }

    public static void Clear() {
        for (int x = 0; x < _instance._rectWidth; x++) {
            for (int y = 0; y < _instance._rectHeight; y++) {
                _instance._tex.SetPixel(x, y, new Color(0, 0, 0, 1));
            }
        }
    }
}