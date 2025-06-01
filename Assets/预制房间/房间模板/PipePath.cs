// 文件名: PipePath.cs
using UnityEngine;
public class PipePath : MonoBehaviour
{
    public Transform topPoint;    // 在Inspector中拖拽管道顶部出口的空对象到这里
    public Transform bottomPoint; // 在Inspector中拖拽管道底部出口的空对象到这里

    // (可选) 在编辑器中绘制路径，方便查看
    void OnDrawGizmosSelected()
    {
        if (topPoint != null && bottomPoint != null)
        {
            Gizmos.color = Color.green;
            // 画一条线代表管道中心路径
            Gizmos.DrawLine(bottomPoint.position, topPoint.position);
            // 标记端点
            Gizmos.DrawWireSphere(topPoint.position, 0.25f);
            Gizmos.DrawWireSphere(bottomPoint.position, 0.25f);
        }
        else if (topPoint != null)
        {
             Gizmos.color = Color.green;
             Gizmos.DrawWireSphere(topPoint.position, 0.25f);
        }
         else if (bottomPoint != null)
        {
             Gizmos.color = Color.green;
             Gizmos.DrawWireSphere(bottomPoint.position, 0.25f);
        }
    }
}
