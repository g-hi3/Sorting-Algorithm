using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMovement3D : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _mouseSensitivityX;
    [SerializeField] private float _mouseSensitivityY;
    [SerializeField] private Transform _escapeMenu;
    private CursorLockMode _cursorLock;
    private FullScreenMode _screenMode;
    private Quaternion _originalRotation;
    private bool _paused;

    protected virtual float MoveSpeed => _moveSpeed;

    protected virtual float MouseSensitivityX => _mouseSensitivityX;

    protected virtual float MouseSensitivityY => _mouseSensitivityY;

    protected virtual Transform EscapeMenu => _escapeMenu;

    protected virtual CursorLockMode CursorLock
    {
        get => _cursorLock;
        set => _cursorLock = value;
    }

    protected virtual FullScreenMode ScreenMode
    {
        get => _screenMode;
        set => _screenMode = value;
    }

    protected virtual Quaternion OriginalRotation
    {
        get => _originalRotation;
        set => _originalRotation = value;
    }

    protected virtual bool Paused
    {
        get => _paused;
        set => _paused = value;
    }

    // Start is called before the first frame update
    void Start()
    {
        CursorLock = CursorLockMode.Locked;
        ScreenMode = FullScreenMode.FullScreenWindow;
        OriginalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Paused = !Paused;

        if (Paused)
            return;

        var horizontalInput = Input.GetAxis("Horizontal");
        var verticalInput = Input.GetAxis("Vertical");
        var flightInput = Input.GetAxis("Flight");
        var sprint = Input.GetAxis("Sprint") + 1;
        var movement = new Vector3(horizontalInput, flightInput, verticalInput) * MoveSpeed * Time.deltaTime * sprint;

        transform.Translate(movement);

        var rotationX = Input.GetAxis("Mouse X") * MouseSensitivityX;
        var rotationY = Input.GetAxis("Mouse Y") * MouseSensitivityY;
        var xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
        var yQuaternion = Quaternion.AngleAxis(rotationY, -Vector3.right);
        var rotation = OriginalRotation * xQuaternion * yQuaternion;

        transform.localEulerAngles += rotation.eulerAngles;
    }

    void OnGUI()
    {
        Cursor.lockState = (Paused ? CursorLockMode.Confined : CursorLock);
        Cursor.visible = Paused;
        Screen.fullScreenMode = ScreenMode;
        EscapeMenu.gameObject.SetActive(Paused);
    }
}
