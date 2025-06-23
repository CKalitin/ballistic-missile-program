using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThrottleDisplayUI : MonoBehaviour {
    public Slider slider;
    public TMP_Text percentLabel;

    UserVehicleControl _controller;
    void Awake() {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.interactable = false;         // read-only HUD
    }

    void Update() {
        if (!_controller) {
            _controller = FindObjectOfType<UserVehicleControl>();
            return;
        }

        float value = _controller.throttle;    // already 0‒1
        slider.value = value;

        if (percentLabel)
            percentLabel.text = $"{value * 100f:0}%";
    }
}
