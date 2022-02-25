
using UnityEngine;

public class ScreenUsageExample : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {
        FillScreenWithGradient();
    }

    private static void FillScreenWithGradient() {
        var screenSize = ApptimeScreen.GetScreenSize();
        for (int x = 0; x < screenSize.x; x++) {
            for (int y = 0; y < screenSize.y; y++) {
                var color = new Color(x / screenSize.x, y / screenSize.y, 0, 1);
                ApptimeScreen.SetPixel(x, y, color);
            }
        }
        ApptimeScreen.ApplyPixelChanges();
    }
}
