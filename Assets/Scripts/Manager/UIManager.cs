using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class UIManager : Singleton<UIManager>
{
    public enum FadeInAndOutColor{White , Red , Black}
    
    public Canvas worldCanvas;
    public Canvas screenCanvas;
    public EventSystem eventSystem;
    [SerializeField]
    private View startingView;
    private List<View> views = new List<View>();
    private View currentView;
    private readonly Stack<View> histroy = new Stack<View>();

    public GameObject loadSceneUIPrefab;
    public GameObject fadeInAndOutPrefab;
    public GameObject playerDeadUIPrefab;

    private StartUI startUI;
    private PlayerUI playerUI;

    private GameObject loadSceneUI;
    private GameObject playerDeadUI;
    private GameObject fadeInAndOutUI;
    private Image fadeInAndOutImage;
    private CanvasGroup fadeInAndOutCanvasGroup;

    private bool isfadingIn;
    private bool isFadingOut;
    
    
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);

        var temp1 = screenCanvas.GetComponentsInChildren<View>();
        foreach (var view in temp1)
        {
            views.Add(view);
            view.Hide();
        }
        
        startUI = worldCanvas.GetComponentInChildren<StartUI>();
        playerUI = GetView<PlayerUI>();

        InstantiateOnScreenCanvas(loadSceneUIPrefab , out loadSceneUI);
        loadSceneUI.SetActive(false);
        InstantiateOnScreenCanvas(playerDeadUIPrefab , out playerDeadUI);
        playerDeadUI.SetActive(false);
        InstantiateOnScreenCanvas(fadeInAndOutPrefab , out fadeInAndOutUI);
        fadeInAndOutImage = fadeInAndOutUI.GetComponent<Image>();
        fadeInAndOutCanvasGroup = fadeInAndOutUI.GetComponent<CanvasGroup>();
        fadeInAndOutUI.SetActive(false);
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    private void SceneLoaded(Scene scene , LoadSceneMode loadSceneMode)
    {
        if (scene.name != "StartScene")
        {
            startUI.Hide();
            playerUI.Show();
            StartCoroutine(PlayerUIFadeIn(1.5f));
        }
        else
        {
            startUI.Show();
            playerUI.Hide();
            StartCoroutine(UIManager.Instance.FadeOut(FadeInAndOutColor.White, 1.5f));
            StartCoroutine(StartUIFadeIn(1.5f));
        }
    }

    public void InstantiateOnScreenCanvas(GameObject prefab , out GameObject uiReference)
    {
        uiReference = Instantiate(prefab, screenCanvas.transform);
        uiReference.GetComponent<RectTransform>().sizeDelta = prefab.GetComponent<RectTransform>().sizeDelta;
        uiReference.GetComponent<RectTransform>().anchoredPosition =
            prefab.GetComponent<RectTransform>().anchoredPosition;
    }

    private IEnumerator PlayerUIFadeIn(float fadeInDuration)
    {
        playerUI.canvasGrounp.alpha = 0f;
        while (fadeInAndOutUI.activeSelf)
            yield return null;
        while (true)
        {
            if(!playerUI.gameObject.activeSelf)
                break;
            if (playerUI.canvasGrounp.alpha < 1f)
            {
                playerUI.canvasGrounp.alpha += Time.deltaTime / fadeInDuration;
                yield return null;
            }
            else break;
        }
    }

    private IEnumerator StartUIFadeIn(float fadeInDuration)
    {
        startUI.canvasGroup.alpha = 0f;
        while (fadeInAndOutUI.activeSelf)
            yield return null;
        while (true)
        {
            if(!startUI.gameObject.activeSelf)
                break;
            if (startUI.canvasGroup.alpha < 1f)
            {
                startUI.canvasGroup.alpha += Time.deltaTime / fadeInDuration;
                yield return null;
            }
            else break;
        }
    }
    
    public IEnumerator FadeIn(FadeInAndOutColor fadeInColor, float fadeInDuration)
    {
        isfadingIn = true;
        fadeInAndOutUI.SetActive(true);
        fadeInAndOutCanvasGroup.alpha = 0f;
        switch (fadeInColor)
        {
            case FadeInAndOutColor.White:
                fadeInAndOutImage.color= new Color(1f, 1f, 1f, 1f);
                while (true)
                {
                    if(isFadingOut)
                        break;
                    if (fadeInAndOutCanvasGroup.alpha < 1f)
                    {
                        fadeInAndOutCanvasGroup.alpha += Time.deltaTime / fadeInDuration;
                        yield return null;
                    }
                    else break;
                }
                break;
            case FadeInAndOutColor.Red:
                fadeInAndOutImage.color = new Color(0.490566f, 0.06247775f, 0.06247775f, 1f);
                while (true)
                {
                    if(isFadingOut)
                        break;
                    if (fadeInAndOutCanvasGroup.alpha < 1f)
                    {
                        fadeInAndOutCanvasGroup.alpha += Time.deltaTime / fadeInDuration;
                        yield return null;
                    }
                    else break;
                }
                break;
            case FadeInAndOutColor.Black:
                fadeInAndOutImage.color = new Color(0f, 0f, 0f, 1f);
                while (true)
                {
                    if(isFadingOut)
                        break;
                    if (fadeInAndOutCanvasGroup.alpha < 1f)
                    {
                        fadeInAndOutCanvasGroup.alpha += Time.deltaTime / fadeInDuration;
                        yield return null;
                    }
                    else break;
                }
                break;
        }
        isfadingIn = false;
    }

    public IEnumerator FadeOut(FadeInAndOutColor fadeOutColor, float fadeOutDuration)
    {
        isFadingOut = true;
        fadeInAndOutUI.SetActive(true);
        fadeInAndOutCanvasGroup.alpha = 1f;
        switch (fadeOutColor)
        {
            case FadeInAndOutColor.White:
                fadeInAndOutImage.color = new Color(1f, 1f, 1f, 1f);
                while (true)
                {
                    if(isfadingIn)
                        break;
                    if (fadeInAndOutCanvasGroup.alpha > 0f)
                    {
                        fadeInAndOutCanvasGroup.alpha -= Time.deltaTime / fadeOutDuration;
                        yield return null;
                    }
                    else break;
                }
                break;
            case FadeInAndOutColor.Red:
                fadeInAndOutImage.color = new Color(0.490566f, 0.06247775f, 0.06247775f, 1f);
                while (true)
                {
                    if(isfadingIn)
                        break;
                    if (fadeInAndOutCanvasGroup.alpha > 0f)
                    {
                        fadeInAndOutCanvasGroup.alpha -= Time.deltaTime / fadeOutDuration;
                        yield return null;
                    }
                    else break;
                }
                break;
            case FadeInAndOutColor.Black:
                fadeInAndOutImage.color = new Color(0f, 0f, 0f, 1f);
                while (true)
                {
                    if(isfadingIn)
                        break;
                    if (fadeInAndOutCanvasGroup.alpha > 0f)
                    {
                        fadeInAndOutCanvasGroup.alpha -= Time.deltaTime / fadeOutDuration;
                        yield return null;
                    }
                    else break;
                }
                break;
        }
        if(!isfadingIn)
            fadeInAndOutUI.SetActive(false);
        isFadingOut = false;
    }

    public static T GetView<T> () where T : View
    {
        foreach (var view in Instance.views)
        {
            if (view is T tView)
                return tView;
        }
        return null;
    }
    
    public static List<T> GetViews<T> () where T : View
    {
        List<T> matchViews = new List<T>();
        foreach (var view in Instance.views)
        {
            if(view is T tView)
                matchViews.Add(tView);
        }
        return matchViews;
    }

    public static void Show<T>(bool remember = false) where T : View
    {
        foreach (var view in Instance.views)
        {
            if (view is T)
            {
                if (Instance.currentView != null)
                {
                    if(remember)
                        Instance.histroy.Push(Instance.currentView);
                    Instance.currentView.Hide();
                }
                view.Show();
                Instance.currentView = view;
            }
        }
    }

    public static void Show(View view, bool remember = false)
    {
        if (Instance.currentView != null)
        {
            if(remember)
                Instance.histroy.Push(Instance.currentView);
            Instance.currentView.Hide();
        }
        view.Show();
        Instance.currentView = view;
    }

    public static void ShoeLast()
    {
        if(Instance.histroy.Count != 0)
            Show(Instance.histroy.Pop(), false);
    }

    public static void HideAllView()
    {
        foreach (var view in Instance.views)
            view.Hide();
    }

    public void GetReference_LoadSceneUI(out GameObject uiReference)
    {
        HideAllView();
        loadSceneUI.SetActive(true);
        uiReference = loadSceneUI;
    }

    public void BreakReference_LoadSceneUI(ref GameObject uiReference)
    {
        loadSceneUI.SetActive(false);
        uiReference = null;
    }
    
    public void GetReference_PlayerDeadUI(out GameObject uiReference)
    {
        HideAllView();
        loadSceneUI.SetActive(true);
        uiReference = loadSceneUI;
    }

    public void BreakReference_PlayerDeadUI(ref GameObject uiReference)
    {
        playerDeadUI.SetActive(false);
        uiReference = null;
    }
}
