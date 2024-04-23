using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PlayerUI : View
{
    public Image playerProfile;
    public Button mainMenuButton;

    public Text nickName;

    public Image levelBackGround;
    private Text level;
    
    public GameObject expBar;
    private Image expSlider;
    private Text expNumber;
    
    public GameObject healthBar;
    private Image healthSlider;
    private Text healthNumber;
    
    [HideInInspector]
    public CanvasGroup canvasGrounp;

    private bool startIsCalled;
    private CharacterStats characterStats;
    private Color customGreen = new Color(0.23137f , 0.8f , 0.1333f , 1f);
    private Color customDarkGreen = new Color(0.14901f , 0.22156f , 0.14901f , 1f);
    private Color customRed = new Color(0.8f , 0.1333f , 0.184313f , 1f);
    private Color customDarkRed = new Color(0.294117f , 0.15294f , 0.1568f , 1f);
    
    public enum LevelColor{Red , Orange , Yellow , Green , Cyan , Blue , Purple , Magenta , Black , Pink , Navy}
    public enum Profile{White , Cyan , Green , Yellow , Red , Pink , Black , Purple , Blue}

    public Sprite whiteDog;
    public Sprite cyanDog;
    public Sprite greenDog;
    public Sprite yellowDog;
    public Sprite redDog;
    public Sprite pinkDog;
    public Sprite blackDog;
    public Sprite purpleDog;
    public Sprite blueDog;
    
    [HideInInspector]
    public LevelColor levelColor;

    [HideInInspector]
    public Profile profile;


    private void Awake()
    {
        //set up reference - level
        level = levelBackGround.transform.GetChild(0).GetComponent<Text>();
        
        //set up reference - expBar
        expSlider = expBar.transform.GetChild(0).GetComponent<Image>();
        expNumber = expBar.transform.GetChild(1).GetComponent<Text>();
        
        //set up reference - healthBar
        healthSlider = healthBar.transform.GetChild(0).GetComponent<Image>();
        healthNumber = healthBar.transform.GetChild(1).GetComponent<Text>();

        canvasGrounp = gameObject.GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        
        characterStats = GameManager.Instance.playerStats;
        GameManager.Instance.PlayerInstantiated += SubscripDelegate;
        characterStats.UpdateHealthUI += UpdateHealthUI;
        characterStats.UpdateExpLevelUI += UpdateExpLevelUI;

        //get player custom settings from saved data
        String key = GameManager.Instance.player.name + GameManager.Instance.playerController.playerGameStatsData.name;
        if (PlayerPrefs.HasKey(key))
        {
            var data = GameManager.Instance.playerController.playerGameStatsData;
            
            //update UI
            levelColor = data.levelColor;
            UpdateOtherUI(data.nickName, data.profile);
        }
        else
        {
            var data = GameManager.Instance.playerController.playerGameStatsData;
            
            //set data value as default
            data.levelColor = LevelColor.Green;
            data.profile = Profile.Yellow;
            data.nickName = "Dog Knight";
            
            //update UI
            levelColor = LevelColor.Green;
            UpdateOtherUI("Dog Knight" , Profile.Yellow);
        }

        UpdatePlayerUI();
        startIsCalled = true;
    }

    private void OnEnable()
    {
        if (startIsCalled)
        {
            GameManager.Instance.PlayerInstantiated += SubscripDelegate;
            GameManager.Instance.playerStats.UpdateHealthUI += UpdateHealthUI;
            GameManager.Instance.playerStats.UpdateExpLevelUI += UpdateExpLevelUI;
            UpdatePlayerUI();
        }
    }

    private void OnDisable()
    {
        if (GameManager.IsInitialized)
        {
            GameManager.Instance.PlayerInstantiated -= SubscripDelegate;
            GameManager.Instance.playerStats.UpdateHealthUI -= UpdateHealthUI;
            GameManager.Instance.playerStats.UpdateExpLevelUI -= UpdateExpLevelUI;
        }
    }

    private void SubscripDelegate()
    {
        characterStats = GameManager.Instance.playerStats;
        characterStats.UpdateHealthUI += UpdateHealthUI;
        characterStats.UpdateExpLevelUI += UpdateExpLevelUI;
    }
    
    public void UpdateOtherUI(String nickName , Profile profile)
    {
        this.nickName.text = nickName;
        switch (profile)
        {
            case Profile.White:
                playerProfile.sprite = whiteDog;
                break;
            case Profile.Cyan:
                playerProfile.sprite = cyanDog;
                break;
            case Profile.Green:
                playerProfile.sprite = greenDog;
                break;
            case Profile.Yellow:
                playerProfile.sprite = yellowDog;
                break;
            case Profile.Red:
                playerProfile.sprite = redDog;
                break;
            case Profile.Pink:
                playerProfile.sprite = pinkDog;
                break;
            case Profile.Black:
                playerProfile.sprite = blackDog;
                break;
            case Profile.Purple:
                playerProfile.sprite = purpleDog;
                break;
            case Profile.Blue:
                playerProfile.sprite = blueDog;
                break;
        }
    }
    
    public void UpdateHealthUI()
    {
        float sliderPercent = (float)characterStats.CurrentHealth / characterStats.MaxHealth;
        healthSlider.fillAmount = sliderPercent;
        healthNumber.text = characterStats.CurrentHealth + " / " + characterStats.MaxHealth;
        if (sliderPercent > 0.3f)
        {
            healthBar.GetComponent<Image>().color = customDarkGreen;
            healthSlider.color = customGreen;
        }
        else
        {
            healthBar.GetComponent<Image>().color = customDarkRed;
            healthSlider.color = customRed;
        }
    }

    public void UpdateExpUI()
    {
        float expSliderPercent = (float)characterStats.CurrentExp / (float)characterStats.MaxExp;
        expSlider.fillAmount = expSliderPercent;
        expNumber.text = characterStats.CurrentExp + " / " + characterStats.MaxExp;
    }

    public void UpdateLevelUI()
    {
        level.text = "LEVEL " + characterStats.CurrentLevel;
        float percent_cur_max = (float)(characterStats.MaxLevel - characterStats.CurrentLevel) / (float)characterStats.MaxLevel;
        float percent_0_cur = (float)characterStats.CurrentLevel / (float)characterStats.MaxLevel;
        float half_0_cur = percent_0_cur * 0.5f;
        switch (levelColor)
        {
            case LevelColor.Red:
                levelBackGround.color = new Color(1f, percent_cur_max, percent_cur_max, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Orange:
                levelBackGround.color = new Color(1f, (1f - percent_0_cur), percent_cur_max, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Yellow:
                levelBackGround.color = new Color(1f, 1f, percent_cur_max, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Green:
                levelBackGround.color = new Color(percent_cur_max, 1f, percent_cur_max, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Cyan:
                levelBackGround.color = new Color(percent_cur_max, 1f, 1f, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Navy:
                levelBackGround.color = new Color(percent_cur_max, percent_cur_max, 1f, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Pink:
                levelBackGround.color = new Color(1f, percent_cur_max, 1f, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Magenta:
                levelBackGround.color = new Color(1f, percent_cur_max, (1f - percent_0_cur), 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Purple:
                levelBackGround.color = new Color((1f - percent_0_cur), percent_cur_max, 1f, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Blue:
                levelBackGround.color = new Color(percent_cur_max, (1f - percent_0_cur), 1f, 1f);
                level.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                break;
            case LevelColor.Black:
                levelBackGround.color = new Color(percent_0_cur, percent_0_cur, percent_0_cur, 1f);
                level.color = new Color(percent_cur_max, percent_cur_max, percent_cur_max, 1f);
                break;
        }
    }
    
    public void UpdatePlayerUI()
    {
        UpdateHealthUI();
        UpdateExpUI();
        UpdateLevelUI();
    }

    public void UpdateExpLevelUI()
    {
        UpdateExpUI();
        UpdateLevelUI();
    }
}
