using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseManager : Singleton<MouseManager>
{

    #region Editor Values
    public Texture2D Point, Doorway, Attack, Target, Arrow;
    
    #endregion

    #region Value
    public event Action<Vector3> OnMouseClicked_Move;
    public event Action<GameObject> OnMouseClicked_Attack;
    public event Action<Vector3> OnMouseClicked_Sprint; 
    private RaycastHit hitInfo;
    private float clickIntervalRemain;
    private float sprintIntervalRemain;
    private float clickInterval;
    private Camera camera;

    #endregion

    #region Unity Functions
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(this.gameObject);
    }

    private void Start()
    {
        clickInterval = GameManager.Instance.playerController.clickInterval;
        camera = CameraController.Instance.camera;
    }

    private void Update()
    {
        SetCursorTexture();
        MouseControl();
    }

    #endregion

    #region Custom Functions
    void SetCursorTexture()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hitInfo))
        {
            //Set cursor texture
            switch (hitInfo.collider.tag)
            {
                case "Ground":
                    Cursor.SetCursor(Target , new Vector2(Target.width/2f , Target.height/2f) , CursorMode.Auto);
                    break;
                case "Enemy":
                    Cursor.SetCursor(Attack , new Vector2(Attack.width/2f , Attack.height/2f) , CursorMode.Auto);
                    break;
                case "Attackable":
                    Cursor.SetCursor(Attack , new Vector2(Attack.width/2f , Attack.height/2f) , CursorMode.Auto);
                    break;
                case "Portal":
                    Cursor.SetCursor(Doorway , new Vector2(Doorway.width/2f , Doorway.height/2f) , CursorMode.Auto);
                    break;
                default:
                    Cursor.SetCursor(Arrow , new Vector2(Arrow.width/2f , Arrow.height/2f) , CursorMode.Auto);
                    break;
            }
        }
    }

    void MouseControl()
    {
        //interaction
        if (Input.GetMouseButtonDown(0) && hitInfo.collider != null)
        {
            if (hitInfo.collider.CompareTag("Ground"))
                OnMouseClicked_Move?.Invoke(hitInfo.point);
            if(hitInfo.collider.CompareTag("Portal"))
                OnMouseClicked_Move?.Invoke(hitInfo.point);
            if (hitInfo.collider.CompareTag("Enemy"))
                OnMouseClicked_Attack?.Invoke(hitInfo.collider.gameObject);
            if (hitInfo.collider.CompareTag("Attackable"))
                OnMouseClicked_Attack?.Invoke(hitInfo.collider.gameObject);

            if (sprintIntervalRemain <= 0)
            {
                if (clickIntervalRemain > 0)
                {
                    OnMouseClicked_Sprint?.Invoke(hitInfo.point);
                    sprintIntervalRemain = GameManager.Instance.playerStats.SprintInterval;
                }
            }
            clickIntervalRemain = clickInterval;
        }
        sprintIntervalRemain -= Time.deltaTime;
        clickIntervalRemain -= Time.deltaTime;

        //rotate the camera and change the height
        if (Input.GetMouseButton(1))
        {
            Cursor.visible = false;
            float mouseInput_x = Input.GetAxis("Mouse X");
            CameraController.Instance.CameraRotation(mouseInput_x);
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
                CameraController.Instance.CameraHeightChanging(0.35f);
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
                CameraController.Instance.CameraHeightChanging(-0.35f);
        }
        else
        {
            Cursor.visible = true;
            //zoom in
            if (Input.GetAxis("Mouse ScrollWheel") > 0)
            {
                if (camera.fieldOfView >= CameraController.Instance.MinFieldOfView)
                    camera.fieldOfView -= 1.5f;
            }
            //zoom out
            if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (camera.fieldOfView <= CameraController.Instance.MaxFieldOfView)
                    camera.fieldOfView += 1.5f;
            }
        }
    }
    #endregion
}
