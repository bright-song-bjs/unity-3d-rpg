using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Portal : MonoBehaviour
{
    public enum TransitionID
    {
        A01 , A02 , A03 , A04 , A05 , A06 , A07 , A08 , A09 , A10 , A11 , A12 , A13 , A14 , A15 ,
        B01 , B02 , B03 , B04 , B05 , B06 , B07 , B08 , B09 , B10 , B11 , B12 , B13 , B14 , B15 ,
        C01 , C02 , C03 , C04 , C05 , C06 , C07 , C08 , C09 , C10 , C11 , C12 , C13 , C14 , C15 
    }
    public enum TransitionType{ SameScene , DifferentScene }
    public enum PortalType{ To , From , ToAndFrom }

    public TransitionType transitionType;
    public string destinationScene;
    public PortalType portalType; 
    public TransitionID transitionID;
    public GameObject teleportButtonPrefab;
    public float buttonHeight;

    [HideInInspector]
    public Transform portalPos;
    private Transform cameraTransfom;
    private GameObject teleportButtonBackground;
    private Button teleportButton;
    private TMP_Text buttonText;
    private bool isPlayerEnter;

    
    private void Start()
    {
        cameraTransfom = CameraController.Instance.camera.transform;
        portalPos = transform.GetChild(0);

        teleportButtonBackground = Instantiate(teleportButtonPrefab, UIManager.Instance.worldCanvas.transform);
        teleportButton = teleportButtonBackground.transform.GetChild(0).GetComponent<Button>();
        buttonText = teleportButton.transform.GetChild(0).GetComponent<TMP_Text>();
        teleportButtonBackground.transform.position = transform.position + new Vector3(0f, buttonHeight, 0f);
        teleportButton.onClick.AddListener(OnButtonPressed);
        teleportButtonBackground.SetActive(false);
    }

    private void Update()
    {
        if ((isPlayerEnter) && (teleportButtonBackground != null))
            teleportButtonBackground.transform.forward = cameraTransfom.forward;
    }

    //button event
    public void OnButtonPressed()
    {
        SceneController.Instance.Transfer(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player")) && (teleportButtonBackground != null))
        {
            if ((portalType == PortalType.From) || (portalType == PortalType.ToAndFrom))
            {
                teleportButtonBackground.SetActive(true);
                isPlayerEnter = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.CompareTag("Player")) && (teleportButtonBackground != null))
        {
            teleportButtonBackground.SetActive(false);
            isPlayerEnter = false;
        }
    }

    private void OnDestroy()
    {
        Destroy(teleportButtonBackground);
    }
}
