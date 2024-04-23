using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CameraController : Singleton<CameraController>
{
    #region Editor Values
    
    [Tooltip("Default camera height")]
    public float CameraHeight;
    
    [Tooltip("Default camera radius of rotation")]
    public float CameraRadius;
    
    [Tooltip("Camera rotate speed")]
    public float CameraRotateSpeed;
    
    [Tooltip("Camera hieght-changing speed")]
    public float CameraHeightChangingSpeed;
    
    [Tooltip("0: No delay")]
    [Range(0f , 1f)]
    public float CameraFollowDelay;

    [Tooltip("Maximum Camera Height")]
    public float MaxCameraHeight;

    [Tooltip("Minimum Camera Height")]
    public float MinCameraHeight;

    [Tooltip("Maximum Camera Field of View")]
    public float MaxFieldOfView;
    
    [Tooltip("Minimum Camera Field of View")]
    public float MinFieldOfView;
    
    #endregion

    #region Values
    private Transform playerTransform;
    [HideInInspector]
    public  float actualCameraHeight;
    private bool startIsCalled;
    [HideInInspector]
    public Camera camera;

    #endregion
    
    #region Unity Functions

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
        camera = transform.GetChild(0).GetChild(0).GetComponent<Camera>();
    }

    private void Start()
    {
        playerTransform = GameManager.Instance.player.transform;
        GameManager.Instance.PlayerInstantiated += ResetCameraTarget;
        camera.transform.localPosition = new Vector3(0f, 0f, 0f);
        transform.GetChild(0).localPosition = new Vector3(0f, 0f, -CameraRadius);
        actualCameraHeight = CameraHeight;
        camera.fieldOfView = 60f;
        transform.position = new Vector3(0f, actualCameraHeight, 0f) + playerTransform.position;
        startIsCalled = true;
    }

    private void OnEnable()
    {
        if (startIsCalled)
        {
            GameManager.Instance.PlayerInstantiated += ResetCameraTarget;
        }
    }

    private void OnDisable()
    {
        if (GameManager.IsInitialized)
        {
            GameManager.Instance.PlayerInstantiated -= ResetCameraTarget;
        }
    }

    private void ResetCameraTarget()
    {
        playerTransform = GameManager.Instance.player.transform;
        transform.position = new Vector3(0f, actualCameraHeight, 0f) + playerTransform.position;
    }

    private void LateUpdate()
    {
        if (playerTransform != null)
        {
            Vector3 desiredPosition = playerTransform.position + new Vector3(0f, actualCameraHeight, 0f);
            Vector3 actualPosition = Vector3.Lerp(desiredPosition , transform.position , CameraFollowDelay);
            transform.position = actualPosition;
            camera.transform.LookAt(playerTransform.position);
        }
    }

    #endregion

    #region Custom Functions
    public void CameraRotation(float mouseInput_x)
    {
        float y = mouseInput_x * Time.deltaTime * CameraRotateSpeed;
        Quaternion rotation = Quaternion.Euler(0f , y , 0f);
        transform.rotation *= rotation;
    }

    public void CameraHeightChanging(float mouseInput_y)
    {
        float y = mouseInput_y * Time.deltaTime * CameraHeightChangingSpeed;
        if (mouseInput_y > 0)
        {
            if (actualCameraHeight <= MaxCameraHeight)
            {
                actualCameraHeight += y;
                transform.position += new Vector3(0f, y , 0f);
            }
        }
        else
        {
            if (actualCameraHeight >= MinCameraHeight)
            {
                actualCameraHeight += y;
                transform.position += new Vector3(0f, y , 0f);
            }
        }
    }
    
    #endregion
}
