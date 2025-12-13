using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    // 弹窗引用
    public GameObject popupWindow;
    public Text popupText;

    // 单例
    public static PopupManager Instance;

    [Header("随机检测间隔范围（秒）")]
    public float minCheckInterval = 2f;
    public float maxCheckInterval = 10f;

    [Header("随机数范围")]
    public int randomMin = 1;
    public int randomMax = 100;

    // 新增：自动关闭配置
    [Header("自动关闭设置")]
    public float autoCloseDelay = 3f; // 弹窗显示后自动关闭的延迟（秒），可在Inspector调整
    private Coroutine autoCloseCoroutine; // 存储自动关闭协程的引用，用于终止

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        if (popupWindow != null)
        {
            Button closeBtn = popupWindow.transform.Find("CloseBtn")?.GetComponent<Button>();
            if (closeBtn != null)
            {
                closeBtn.onClick.AddListener(HidePopup);
            }
            else
            {
                Debug.LogError("未找到CloseBtn或其没有Button组件！");
            }
        }
        else
        {
            Debug.LogError("popupWindow未赋值！");
        }

        StartCoroutine(RandomCheckAndShowPopup());
    }

    // 改造显示弹窗方法：启动自动关闭协程
    public void ShowPopup(string text = "默认弹窗内容")
    {
        if (popupWindow != null && popupText != null)
        {
            popupWindow.SetActive(true);
            popupText.text = text;

            // 先终止之前的自动关闭协程（避免重复触发）
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
            }
            // 启动新的自动关闭协程
            autoCloseCoroutine = StartCoroutine(AutoClosePopup());
        }
    }

    // 改造隐藏弹窗方法：终止自动关闭协程
    public void HidePopup()
    {
        if (popupWindow != null)
        {
            popupWindow.SetActive(false);

            // 手动关闭时，终止自动关闭协程（防止协程还在等待后重复关闭）
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }
    }

    // 新增：自动关闭弹窗的协程
    private IEnumerator AutoClosePopup()
    {
        // 等待指定延迟时间
        yield return new WaitForSeconds(autoCloseDelay);
        // 延迟结束后关闭弹窗
        HidePopup();
        // 清空协程引用
        autoCloseCoroutine = null;
    }

    private IEnumerator RandomCheckAndShowPopup()
    {
        while (true)
        {
            float waitTime = Random.Range(minCheckInterval, maxCheckInterval);
            yield return new WaitForSeconds(waitTime);

            int randomNum = Random.Range(randomMin, randomMax + 1);
            Debug.Log($"生成随机数：{randomNum}，等待时间：{waitTime:F2}秒");

            if (randomNum % 5 == 0)
            {
                ShowPopup($"随机数{randomNum}能整除5！弹窗触发~ {autoCloseDelay}秒后自动关闭");
            }
        }
    }
}