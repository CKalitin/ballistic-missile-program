using UnityEngine;

/// <summary>
/// Multi-mode camera controller.
///
///   • Free   – MMB drag pans pivot; RMB drag orbits camera about pivot; wheel zooms (dolly).
///   • Follow – Camera stays put, always looks at target; wheel zooms by FOV.
///   • Keys   – F toggles modes; R (while Free) snaps back to Follow/target.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    #region Variables

    // ───────────── Public tunables ─────────────
    [Header("General")]
    public VehicleInstance target;                // Rocket (or anything) to follow

    [Header("Free-camera")]
    public float freeZoomSpeed = 30f;    // Mouse-wheel → metres
    public float freePanSpeed = 0.1f;      // MMB drag → metres / pixel
    public float freeOrbitSpeed = 0.05f;   // RMB drag → ° per pixel
    public float minDistance = 5f;
    public float maxDistance = 5000f;
    public float minPitchDeg = -85f;    // Clamp so we never flip
    public float maxPitchDeg = 85f;
    public float defaultFoV = 60f;

    [Header("Follow-camera")]
    public float followFovSpeed = 30f;     // Wheel → °/unit
    public float minFollowFov = 1f;
    public float maxFollowFov = 90f;

    [Header("Vehicle-picking")]
    public string vehicleLayerName = "Vehicle";   // exact layer name in your project
    public float pickRayMaxDist = 10000f;       // how far the raycast can reach

    [Header("Keys")]
    public KeyCode toggleKey = KeyCode.F;   // Follow <--> Free
    public KeyCode resetKey = KeyCode.R;   // Free → Follow

    // ───────────── Internal state ─────────────
    private enum Mode { Free, Follow }
    private Mode _mode = Mode.Follow;

    private Camera _cam;
    private Vector3 _pivot;       // Point we’re looking at (Free)
    private float _distance;    // Camera-to-pivot distance
    private float _yaw;         // Yaw & pitch around pivot (Free)
    private float _pitch;

    private Vector3 _prevMousePos;

    private int _cachedLayerMask;

    #endregion

    #region Unity Lifecycle Functions

    // ───────────────────────────────────────────
    void Awake() {
        _cam = GetComponent<Camera>();

        _prevMousePos = Input.mousePosition;

        // Build layer mask once and cache it, no updates though
        _cachedLayerMask = LayerMask.GetMask(vehicleLayerName);

        EnterFreeMode();
    }

    void FixedUpdate() {
        HandleVehiclePicking();

        HandleModeSwitching();

        if (_mode == Mode.Free) UpdateFree();
        else UpdateFollow();
    }

    #endregion

    #region Camera Updates

    void UpdateFree() {
        if (target != null) _pivot = target.worldCoM;

        /* —— (1) re-seed when either button goes down —— */
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            _prevMousePos = Input.mousePosition;

        /* —— mouse deltas only while a button is held —— */
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2)) {
            Vector3 mouseDelta = Input.mousePosition - _prevMousePos;

            // (a) RMB → orbit
            if (Input.GetMouseButton(1)) {
                _yaw += mouseDelta.x * freeOrbitSpeed;
                _pitch -= mouseDelta.y * freeOrbitSpeed;
                _pitch = Mathf.Clamp(_pitch, minPitchDeg, maxPitchDeg);
            }

            // (b) MMB → pan
            if (Input.GetMouseButton(2)) {
                target = null;

                Vector3 right = transform.right;
                Vector3 up = transform.up;

                //  NEW: scale by current zoom distance  (_distance)
                _pivot += (-right * mouseDelta.x - up * mouseDelta.y)
                          * freePanSpeed * _distance * Time.deltaTime;
            }

            _prevMousePos = Input.mousePosition; // update only while dragging
        }

        // 3) Zoom (dolly) with wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 1e-3f) {
            _distance = Mathf.Clamp(_distance - scroll * freeZoomSpeed, minDistance, maxDistance);
        }

        // 4) Re-position camera from yaw/pitch/radius
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        transform.position = _pivot + rot * Vector3.forward * -_distance;
        transform.LookAt(_pivot);
    }

    void UpdateFollow() {
        if (target == null) return;

        transform.LookAt(target.worldCoM);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 1e-3f) {
            _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView - scroll * followFovSpeed, minFollowFov, maxFollowFov);
        }
    }

    #endregion

    #region Mode Switching

    void HandleModeSwitching() {
        if (Input.GetKeyDown(toggleKey)) {
            if (_mode == Mode.Follow) EnterFreeMode();
            else EnterFollowMode();
        }

        if (_mode == Mode.Follow && Input.GetKeyUp(resetKey)) {
            EnterFollowMode();
        }

        if (_mode == Mode.Free && Input.GetKeyDown(resetKey)) {
            EnterFreeMode();
        }
    }

    void EnterFreeMode() {
        _mode = Mode.Free;

        // Use current view as starting point for yaw/pitch
        _pivot = target ? target.worldCoM : transform.position + transform.forward * _distance;
        Vector3 dir = (transform.position - _pivot).normalized;
        _distance = Vector3.Distance(transform.position, _pivot);
        _yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        _pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;

        _cam.fieldOfView = defaultFoV;
    }

    void EnterFollowMode() {
        if (target == null) return;
        _mode = Mode.Follow;
        _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView, minFollowFov, maxFollowFov);
    }

    /// <summary>
    /// If the player left-clicks on a collider that sits on the “vehicle” layer,
    /// grab the collider’s root transform and begin following it.
    /// </summary>
    void HandleVehiclePicking() {
        if (!Input.GetMouseButtonDown(0)) return; // only on click

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, pickRayMaxDist, _cachedLayerMask)) {
            Transform root = hit.transform.root;

            target = root.GetComponent<VehicleInstance>(); // new follow target
            if (_mode == Mode.Free) EnterFreeMode();
            if (_mode == Mode.Follow) EnterFollowMode();
        }
    }

    #endregion
}
