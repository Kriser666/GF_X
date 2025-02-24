using UnityEngine;
using UnityEngine.EventSystems;

public class CarEntity : EntityBase
{
    [Tooltip("旋转速度")]
    [SerializeField]
    private float m_RotationSpeed = 10f;
    [Tooltip("是否启用平滑差值")]
    [SerializeField]
    private bool smoothRotation = true; // 启用平滑插值
    [Tooltip("平滑系数")]
    [SerializeField]
    private float smoothFactor = 10f;   // 平滑系数

    private Vector3 m_TargetRotation;
    protected override void OnInit(object userData)
    {
        base.OnInit(userData);
        m_TargetRotation = transform.eulerAngles;
    }
    protected override void OnShow(object userData)
    {
        base.OnShow(userData);

    }
    protected override void OnUpdate(float elapseSeconds, float realElapseSeconds)
    {
        base.OnUpdate(elapseSeconds, realElapseSeconds);
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButton(0))
        {
            float mouseX = Input.GetAxis("Mouse X") * m_RotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * m_RotationSpeed;

            m_TargetRotation += new Vector3(mouseY, -mouseX, 0);
        }

        // 平滑过渡到目标旋转
        if (smoothRotation)
        {
            transform.rotation = Quaternion.Slerp
            (
                transform.rotation,
                Quaternion.Euler(m_TargetRotation),
                Time.deltaTime * smoothFactor
            );
        }
        else
        {
            transform.eulerAngles = m_TargetRotation;
        }
    }
    protected override void OnHide(bool isShutdown, object userData)
    {

        base.OnHide(isShutdown, userData);
    }


}
