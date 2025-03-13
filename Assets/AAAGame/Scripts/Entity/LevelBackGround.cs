using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelBackGround : MonoBehaviour
{
    [SerializeField]
    private Image backGroundImage;
    private Sprite oldSprite = null;
    [SerializeField]
    private Canvas canvas;
    public Sprite OldSprite { get { return oldSprite; } }
    public RectTransform Ground;

    void Start()
    {
        if (backGroundImage == null)
        {
            backGroundImage = transform.GetChild(0).GetComponent<Image>();
        }
        if (canvas == null)
        {
            canvas = GetComponent<Canvas>();
        }
        if (Ground == null)
        {
            Ground = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        }
        canvas.worldCamera = CameraController.Instance.BackGroundCamera;
    }
    public void ChangeBackGroundSprite(Sprite sprite = null)
    {
        if (sprite != null)
        {
            if (oldSprite == null)
            {
                oldSprite = backGroundImage.sprite;
            }
            backGroundImage.sprite = sprite;
        }
    }
    public void ResetBackGround()
    {
        if (oldSprite != null)
        {
            backGroundImage.sprite = oldSprite;
            oldSprite = null;
        }
    }
    public void MoveGround(int viewId, bool smooth = false)
    {
        var camTb = GF.DataTable.GetDataTable<CameraViewTable>();
        var camRow = camTb.GetDataRow(viewId);
        var offset = camRow.Offset;
        // 将世界坐标转换为屏幕坐标
        Vector3 screenPos = CameraController.Instance.MainCam.WorldToScreenPoint(offset);

        // 将屏幕坐标转换为Canvas的本地坐标
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            screenPos,
            canvas.worldCamera,
            out Vector2 localPos
        );
        if (smooth)
        {
            StartCoroutine(SmothOffset(localPos));
        }
        else
        {
            Ground.anchoredPosition = localPos;
        }
    }
    internal IEnumerator SmothOffset(Vector2 offset)
    {
        Vector2 startPos = Ground.anchoredPosition;
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Ground.anchoredPosition = Vector2.Lerp(startPos, offset, elapsed / duration);
            yield return null;
        }

        Ground.anchoredPosition = offset; // 确保精确到达终点
        yield return null;
    }
}
