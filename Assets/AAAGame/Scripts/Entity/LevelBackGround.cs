using UnityEngine;
using UnityEngine.UI;

public class LevelBackGround : UIFormBase
{
    [SerializeField]
    private Image backGroundImage;
    private Sprite oldSprite = null;
    [SerializeField]
    private Canvas canvas;
    public Sprite OldSprite { get { return oldSprite; } }

    protected override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        if (backGroundImage == null)
        {
            backGroundImage = transform.GetComponentInChildren<Image>();
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
