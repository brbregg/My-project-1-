using UnityEngine;

public class Camera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform CollegeStudent;
    [SerializeField] private Vector3 offset = new Vector3(0, 3, 0); // 相机相对于玩家的偏移

    [Header("Smooth Follow Settings")]
    [SerializeField] private float smoothSpeed = 5f; // 平滑跟随速度，值越大跟随越快
    [SerializeField] private bool useSmoothDamp = true; // 使用SmoothDamp还是Lerp
    private Vector3 velocity = Vector3.zero; // 用于SmoothDamp
    [SerializeField] private float smoothTime = 0.3f; // SmoothDamp的平滑时间

    [Header("Camera Boundary Settings (场景边界)")]
    [SerializeField] private bool useBoundary = true; // 是否启用边界限制
    [SerializeField] private float minX = -10f; // 场景X轴最小边界
    [SerializeField] private float maxX = 100f; // 场景X轴最大边界
    [SerializeField] private float minY = -5f;  // 场景Y轴最小边界
    [SerializeField] private float maxY = 20f;  // 场景Y轴最大边界

    [Header("Debug Settings")]
    [SerializeField] private bool showBoundaryGizmos = true; // 在编辑器中显示边界线
    [SerializeField] private Color boundaryColor = Color.green; // 场景边界线颜色
    [SerializeField] private Color cameraBoundsColor = Color.yellow; // 相机可视区域颜色

    // 缓存相机组件
    private UnityEngine.Camera cam;

    void Start()
    {
        cam = GetComponent<UnityEngine.Camera>();
        if (cam == null)
        {
            Debug.LogError("Camera component not found!");
        }
    }

    void LateUpdate()
    {
        if (CollegeStudent == null || cam == null) return;

        // 计算目标位置
        Vector3 targetPosition = CollegeStudent.position + offset;
        targetPosition.z = transform.position.z; // 保持相机的Z轴位置不变

        // 平滑跟随
        Vector3 smoothedPosition;
        if (useSmoothDamp)
        {
            // 使用SmoothDamp实现更自然的平滑效果
            smoothedPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        }
        else
        {
            // 使用Lerp实现平滑跟随
            smoothedPosition = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }

        // 应用边界限制（考虑相机可视区域）
        if (useBoundary)
        {
            smoothedPosition = ClampCameraPosition(smoothedPosition);
        }

        // 应用最终位置
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// 根据相机可视区域限制相机位置，确保不会显示场景边界外的内容
    /// </summary>
    private Vector3 ClampCameraPosition(Vector3 position)
    {
        // 计算相机可视区域的半宽和半高
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        // 计算相机中心点的有效范围
        // 相机中心点必须在 [minX + halfWidth, maxX - halfWidth] 范围内
        // 这样相机可视区域的左边界就不会小于 minX，右边界不会大于 maxX
        float clampedMinX = minX + halfWidth;
        float clampedMaxX = maxX - halfWidth;
        float clampedMinY = minY + halfHeight;
        float clampedMaxY = maxY - halfHeight;

        // 处理场景过小的情况（场景宽度/高度小于相机可视范围）
        if (clampedMinX > clampedMaxX)
        {
            // 场景宽度小于相机可视宽度，将相机居中
            position.x = (minX + maxX) / 2f;
        }
        else
        {
            position.x = Mathf.Clamp(position.x, clampedMinX, clampedMaxX);
        }

        if (clampedMinY > clampedMaxY)
        {
            // 场景高度小于相机可视高度，将相机居中
            position.y = (minY + maxY) / 2f;
        }
        else
        {
            position.y = Mathf.Clamp(position.y, clampedMinY, clampedMaxY);
        }

        return position;
    }

    /// <summary>
    /// 在Unity编辑器中绘制边界线（仅在编辑器中可见）
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!showBoundaryGizmos || !useBoundary) return;

        // 绘制场景边界（绿色）
        Gizmos.color = boundaryColor;
        Vector3 topLeft = new Vector3(minX, maxY, 0);
        Vector3 topRight = new Vector3(maxX, maxY, 0);
        Vector3 bottomLeft = new Vector3(minX, minY, 0);
        Vector3 bottomRight = new Vector3(maxX, minY, 0);

        Gizmos.DrawLine(topLeft, topRight);     // 上边
        Gizmos.DrawLine(bottomLeft, bottomRight); // 下边
        Gizmos.DrawLine(topLeft, bottomLeft);   // 左边
        Gizmos.DrawLine(topRight, bottomRight); // 右边

        // 绘制相机可移动范围（黄色）- 相机中心点的有效范围
        UnityEngine.Camera cam = GetComponent<UnityEngine.Camera>();
        if (cam != null && cam.orthographic)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float clampedMinX = minX + halfWidth;
            float clampedMaxX = maxX - halfWidth;
            float clampedMinY = minY + halfHeight;
            float clampedMaxY = maxY - halfHeight;

            // 只有当场景大于相机可视范围时才绘制
            if (clampedMinX <= clampedMaxX && clampedMinY <= clampedMaxY)
            {
                Gizmos.color = cameraBoundsColor;
                Vector3 camTopLeft = new Vector3(clampedMinX, clampedMaxY, 0);
                Vector3 camTopRight = new Vector3(clampedMaxX, clampedMaxY, 0);
                Vector3 camBottomLeft = new Vector3(clampedMinX, clampedMinY, 0);
                Vector3 camBottomRight = new Vector3(clampedMaxX, clampedMinY, 0);

                Gizmos.DrawLine(camTopLeft, camTopRight);
                Gizmos.DrawLine(camBottomLeft, camBottomRight);
                Gizmos.DrawLine(camTopLeft, camBottomLeft);
                Gizmos.DrawLine(camTopRight, camBottomRight);
            }
        }
    }

    /// <summary>
    /// 在运行时动态设置边界
    /// </summary>
    public void SetBoundary(float newMinX, float newMaxX, float newMinY, float newMaxY)
    {
        minX = newMinX;
        maxX = newMaxX;
        minY = newMinY;
        maxY = newMaxY;
    }

    /// <summary>
    /// 启用或禁用边界限制
    /// </summary>
    public void SetBoundaryEnabled(bool enabled)
    {
        useBoundary = enabled;
    }
}
