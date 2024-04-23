using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class StartUI : View
{
    [SerializeField]
    private GameObject newGameHolder;
    
    [SerializeField]
    private GameObject continueHolder;
    
    [SerializeField]
    private GameObject quitGameHolder;
    
    [SerializeField]
    private GameObject newGameReassure;
    
    private Button yesButton;
    
    private Button noButton;

    private Button newGameButton;

    private Button continueButton;

    private Button quitGameButton;
    
    [SerializeField]
    private Text title;
    
    [SerializeField]
    private Text title_Animation;
    
    [SerializeField]
    private Text copyRight;
    
    [SerializeField]
    private Text copyRight_Animation;
    
    [SerializeField]
    private Text welcome_Animation;
    
    [SerializeField]
    private PlayableDirector newGameDirector;

    [HideInInspector]
    public CanvasGroup canvasGroup;


    private void Awake()
    {
        //set up reference - newGameReassure
        yesButton = newGameReassure.transform.GetChild(0).GetComponent<Button>();
        noButton = newGameReassure.transform.GetChild(1).GetComponent<Button>();
        
        //set up reference - buttons
        newGameButton = newGameHolder.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        continueButton = continueHolder.transform.GetChild(0).GetChild(0).GetComponent<Button>();
        quitGameButton = quitGameHolder.transform.GetChild(0).GetChild(0).GetComponent<Button>();

        canvasGroup = gameObject.GetComponent<CanvasGroup>();

        //add button event
        newGameButton.onClick.AddListener(NewGame);
        continueButton.onClick.AddListener(Continue);
        quitGameButton.onClick.AddListener(QuitGame);
        yesButton.onClick.AddListener(Yes);
        noButton.onClick.AddListener(No);
    }

    private void Start()
    {
        Cursor.SetCursor(MouseManager.Instance.Arrow , new Vector2(MouseManager.Instance.Arrow.width/2f ,
            MouseManager.Instance.Arrow.height/2f) , CursorMode.Auto);
    }
    
    private void OnEnable()
    {
        if ((PlayerPrefs.GetInt("Started" , 0)) == 0)
        {
            //entirely new - show two buttons
            newGameHolder.gameObject.SetActive(true);
            continueHolder.gameObject.SetActive(false);
            quitGameHolder.gameObject.SetActive(true);
            newGameHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(-114f , -22f);
            quitGameHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(114f , -22f);
        }
        else
        {
            //already played - show three buttons
            title.gameObject.SetActive(true);
            newGameHolder.gameObject.SetActive(true);
            continueHolder.gameObject.SetActive(true);
            quitGameHolder.gameObject.SetActive(true);
            copyRight.gameObject.SetActive(true);
            newGameHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(-175f , -22f);
            continueHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f , -22f);
            quitGameHolder.GetComponent<RectTransform>().anchoredPosition = new Vector2(175f , -22f);
        }

        copyRight.gameObject.SetActive(true);
        title.gameObject.SetActive(true);
        newGameReassure.SetActive(false);
        
        //these are animation related gameObjects
        title_Animation.gameObject.SetActive(false);
        copyRight_Animation.gameObject.SetActive(false);
        welcome_Animation.gameObject.SetActive(false);
    }

    //button event
    public void NewGame()
    {
        if ((PlayerPrefs.GetInt("Started" , 0)) == 0)
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.SetInt("Started" , 1);
            //hide all UI elements
            newGameHolder.gameObject.SetActive(false);
            continueHolder.gameObject.SetActive(false);
            quitGameHolder.gameObject.SetActive(false);
            title.gameObject.SetActive(false);
            copyRight.gameObject.SetActive(false);
            
            //enable some of the animation related gameObjects, others will get active in activation track
            title_Animation.gameObject.SetActive(true);

            newGameDirector.Play();
        }
        else
        {
            //pop up the reassure menu, and hide everything except title
            newGameReassure.SetActive(true);
            newGameHolder.gameObject.SetActive(false);
            continueHolder.gameObject.SetActive(false);
            quitGameHolder.gameObject.SetActive(false);
            copyRight.gameObject.SetActive(false);
        }
    }

    //button event
    public void Continue()
    {
        //hide all UI elements
        newGameHolder.gameObject.SetActive(false);
        continueHolder.gameObject.SetActive(false);
        quitGameHolder.gameObject.SetActive(false);
        title.gameObject.SetActive(false);
        copyRight.gameObject.SetActive(false);
        
        //load the previous scene in saved data
        String key = "Player(Clone)" + GameManager.Instance.playerController.playerGameStatsData.name;
        if (GameManager.Instance.LoadData(key, GameManager.Instance.playerController.playerGameStatsData))
            StartCoroutine(SceneController.Instance.LoadScene_FadeInAndOut(
                GameManager.Instance.playerController.playerGameStatsData.previousSceneName , 
                UIManager.FadeInAndOutColor.White , UIManager.FadeInAndOutColor.White , 
                1.5f , 1.5f));
        else 
            StartCoroutine(SceneController.Instance.LoadScene_FadeInAndOut("MainScene" , 
                UIManager.FadeInAndOutColor.White , UIManager.FadeInAndOutColor.White , 
                1.5f , 1.5f));
    }

    
    private IEnumerator QuitGameCoroutine()
    {
        yield return StartCoroutine(UIManager.Instance.FadeIn(UIManager.FadeInAndOutColor.Black, 3f));
        Application.Quit();
    }
    
    //button event
    public void QuitGame()
    {
        //hide all UI elements
        newGameHolder.gameObject.SetActive(false);
        continueHolder.gameObject.SetActive(false);
        quitGameHolder.gameObject.SetActive(false);
        title.gameObject.SetActive(false);
        copyRight.gameObject.SetActive(false);
        StartCoroutine(QuitGameCoroutine());
    }

    //button event
    private void Yes()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("Started" , 1);
        
        //hide reassure menu and title (others are disabled when reassure menu pops up)
        newGameReassure.SetActive(false);
        title.gameObject.SetActive(false);
        
        //enable some of the animation related gameObjects, others will get active in activation track
        title_Animation.gameObject.SetActive(true);

        newGameDirector.Play();
    }

    //button event
    private void No()
    {
        //hide the reassure menu, and show everything else
        newGameReassure.SetActive(false);
        newGameHolder.gameObject.SetActive(true);
        continueHolder.gameObject.SetActive(true);
        quitGameHolder.gameObject.SetActive(true);
        title.gameObject.SetActive(true);
        copyRight.gameObject.SetActive(true);
    }
    
    //signal event
    public void StartGame()
    {
        StartCoroutine(SceneController.Instance.LoadScene_FadeInAndOut("MainScene" , 
            UIManager.FadeInAndOutColor.White , UIManager.FadeInAndOutColor.White , 
            2.5f , 1.5f));
    }
}
