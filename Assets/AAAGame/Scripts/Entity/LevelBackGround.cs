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
}
