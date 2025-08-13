using UnityEngine;
using Battlehub.RTCommon;

public class RTEUndoBridge : MonoBehaviour
{
    // ✅ 换成 IRuntimeUndo（你这版有）
    private IRuntimeUndo _undo;

    private void Awake()
    {
        // 大多数版本可以直接从 IOC 解析 IRuntimeUndo
        _undo = IOC.Resolve<IRuntimeUndo>();

        // 某些版本没有注册上面的接口，但 IRTE 里有 Undo / UndoRedo 属性
        if (_undo == null)
        {
            var rte = IOC.Resolve<IRTE>();
            if (rte != null)
            {
                // 先找 Undo，再找 UndoRedo（不同版本命名不一）
                var undoProp = rte.GetType().GetProperty("Undo") ?? rte.GetType().GetProperty("UndoRedo");
                if (undoProp != null)
                    _undo = undoProp.GetValue(rte) as IRuntimeUndo;
            }
        }

        if (_undo != null)
        {
            // 这版常见的是 StateChanged（Action 委托）
            _undo.StateChanged += OnUndoRedoCompleted;
        }
    }

    private void OnDestroy()
    {
        if (_undo != null)
        {
            _undo.StateChanged -= OnUndoRedoCompleted;
        }
    }

    private void OnUndoRedoCompleted()
    {
        // 撤销/重做后，通知你的系统做需要的刷新（或触发你已有的精细事件）
        if (ChartManager.Instance != null)
        {
            // 若你已实现了增删差异化事件，这里通常不需要全量刷新；
            // 没有的话，先用全量刷新兜底：
            ChartManager.Instance.NotifyContentChanged();
        }
    }
}