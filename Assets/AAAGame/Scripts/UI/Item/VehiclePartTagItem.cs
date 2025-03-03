using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public partial class VehiclePartTagItem : UIItemBase, IPointerClickHandler
{
    private Image chooseImage;
    private Color originalColor;
    private Color swapedColor = new (0.5f, 0.5f, 0.5f);
    private int partId;
    private VehiclePartTypeEnum whichType;
    
    public int PartId { get { return partId; } set { partId = value; } }
    public TextMeshProUGUI VarPartName { get { return varPartName; } set { varPartName = value; } }
    public TextMeshProUGUI VarPerformanceNum { get { return varPerformanceNum; } set { varPerformanceNum = value; } }
    public Image VarPartImage { get { return varPartImage; } set { varPartImage.sprite = value.sprite; } }
    public VehiclePartTypeEnum WhichType { get { return whichType; } set { whichType = value; } }

    protected override void OnInit()
    {
        base.OnInit();
        chooseImage = GetComponent<Image>();
        originalColor = chooseImage.color;
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (originalColor == chooseImage.color)
        {
            chooseImage.color = swapedColor;
            GF.Event.Fire(this, PartItemSelectedEventArgs.Create(PartUIItemSelectedDataType.Selected, partId, whichType));
        }
        else
        {
            chooseImage.color = originalColor;
            GF.Event.Fire(this, PartItemSelectedEventArgs.Create(PartUIItemSelectedDataType.CancelSelected, partId, whichType));
        }
    }

}
