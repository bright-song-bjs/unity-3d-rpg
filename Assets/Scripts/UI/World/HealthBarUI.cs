using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{

    #region Editor Values
    [Tooltip("Whether this enemy is BOSS")]
    public bool IsBOSS;
    
    [Space]
    
    [Header("Non-BOSS HealthBar Settings")]
    public GameObject HealthBarPrefab;
    [Tooltip("choose one among 1,2,3 --- 1:small, 2:medium, 3:large")]
    [Range(1, 3)]
    public int HealthBarSize;
    [Tooltip("The height the healthBar hovers over the enemy")]
    public float Height;
    [Tooltip("The radius in which the healthBar will show (Green)")]
    public float ShowingRadius;
    [Tooltip("The HealthBarShowingRadius will be set as SightRadius, and the HealthBarShowingRadius value will be ignored")]
    public bool RadiusSetAsDefalut;

    [Space]

    [Header("BOSS HealthBar Settings")]
    public GameObject HealthBarPrefab_BOSS;
    public string BOSSName;
    [TextArea]
    public string BOSSDescription;
    
    #endregion
    
    private Transform cameraTransform;
    private CharacterStats characterStats;
    [HideInInspector]
    public GameObject healthBar;
    private Image transition;
    private Image healthSlider;
    private TMP_Text level;
    private Text bossName;
    private Text description;
    private Color customWhite = new Color(0.8627f, 0.8627f, 0.8627f, 1f);
    private Color customRed = new Color(0.8627f, 0.12941f, 0.12941f, 1f);
    private Coroutine transitionCoroutine;

    private void Awake()
    {
        characterStats = GetComponent<CharacterStats>();
        if (RadiusSetAsDefalut)
            ShowingRadius = GetComponent<EnemyController>().SightRadius;
    }

    private void OnEnable()
    {
        characterStats.UpdateHealthUI += UpdateHealthUI;
        characterStats.UpdateExpLevelUI += UpdateLevelUI;
    }

    private void OnDisable()
    {
        characterStats.UpdateHealthUI -= UpdateHealthUI;
        characterStats.UpdateExpLevelUI -= UpdateLevelUI;
    }

    private void Start()
    {
        if (!IsBOSS)
        {
            cameraTransform = CameraController.Instance.camera.transform;
            
            healthBar = Instantiate(HealthBarPrefab , UIManager.Instance.worldCanvas.transform);
            transition = healthBar.transform.GetChild(0).GetComponent<Image>();
            healthSlider = healthBar.transform.GetChild(1).GetComponent<Image>();
            level = healthBar.transform.GetChild(2).GetComponent<TMP_Text>();
            SetHealthBarRectTransform();
            UpdateEnemyUI();
            healthBar.SetActive(false);
        }
        else
        {
            UIManager.Instance.InstantiateOnScreenCanvas(HealthBarPrefab_BOSS, out healthBar);
            transition = healthBar.transform.GetChild(0).GetComponent<Image>();
            healthSlider = healthBar.transform.GetChild(1).GetComponent<Image>();
            level = healthBar.transform.GetChild(2).GetComponent<TMP_Text>();
            SetHealthBarRectTransform();
            UpdateEnemyUI();
            healthBar.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (!IsBOSS)
        {
            if (healthBar != null)
            {
                healthBar.transform.position = transform.position + new Vector3(0f, Height, 0f);
                healthBar.transform.forward = cameraTransform.forward;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position , ShowingRadius);
    }

    public void UpdateHealthUI()
    {
        float sliderPercent = (float)characterStats.CurrentHealth / characterStats.MaxHealth;
        healthSlider.fillAmount = sliderPercent;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = StartCoroutine(HealthBarSliderTransition(sliderPercent));
        }
        transitionCoroutine = StartCoroutine(HealthBarSliderTransition(sliderPercent));
    }

    public void UpdateLevelUI()
    {
        level.text = "Lv." + characterStats.CurrentLevel;
        if (GameManager.Instance.playerStats.CurrentLevel < characterStats.CurrentLevel)
            level.color = customRed;
        else level.color = customWhite;
    }
    
    public void UpdateEnemyUI()
    {
        UpdateHealthUI();
        UpdateLevelUI();
    }

    private IEnumerator HealthBarSliderTransition(float finalPercent)
    {
        if (transition.fillAmount > finalPercent)
        {
            float percent = transition.fillAmount - finalPercent;
            while (transition.fillAmount > finalPercent)
            {
                transition.fillAmount -= percent * Time.deltaTime / 1.5f;
                yield return null;
            }
        }
        else transition.fillAmount = finalPercent;
    }

    private void SetHealthBarRectTransform()
    {
        if(healthBar == null) return;
        if (!IsBOSS)
        {
            switch (HealthBarSize)
            {
                case 1:
                    healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(1.2f , 0.08f);
                    level.GetComponent<RectTransform>().sizeDelta = new Vector2(0.6f, 0.25f);
                    level.fontSize = 0.17f;
                    break;
                case 2:
                    healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(2f , 0.1f);
                    level.GetComponent<RectTransform>().sizeDelta = new Vector2(1f, 0.4f);
                    level.fontSize = 0.3f;
                    break;
                case 3:
                    healthBar.GetComponent<RectTransform>().sizeDelta = new Vector2(4f , 0.15f);
                    level.GetComponent<RectTransform>().sizeDelta = new Vector2(2f, 0.8f);
                    level.fontSize = 0.5f;
                    break;
            }
        }
        else
        {
            bossName = healthBar.transform.GetChild(3).GetComponent<Text>();
            description = healthBar.transform.GetChild(4).GetComponent<Text>();

            bossName.text = BOSSName;
            description.text = BOSSDescription;
        }
    }

    private void OnDestroy()
    {
        Destroy(healthBar);
    }
}
