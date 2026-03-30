using System;
using System.Collections.Generic;

/// <summary>Pure simulation: sim time, triggers, queue, non-preemptive handling, TotalLoss.</summary>
public sealed class NightShiftSimulation
{
    private readonly NightShiftScriptParams _p;
    private readonly Action<string> _log;

    private float _simTime;
    private bool _triggeredC;
    private bool _triggeredB;
    private bool _triggeredA;

    private readonly List<NightShiftTaskId> _pending = new List<NightShiftTaskId>();
    private readonly List<NightShiftTaskId> _playerOrder = new List<NightShiftTaskId>();
    private readonly List<NightShiftTaskId> _completed = new List<NightShiftTaskId>();

    private bool _hasCurrent;
    private NightShiftTaskId _current;
    private float _currentStartTime;

    /// <param name="p">Frozen config for this run.</param>
    /// <param name="log">Optional; e.g. pass msg => Debug.Log for debug Runner.</param>
    public NightShiftSimulation(NightShiftScriptParams p, Action<string> log = null)
    {
        _p = p;
        _log = log ?? (_ => { });
    }

    public float SimTime => _simTime;
    public IReadOnlyList<NightShiftTaskId> Pending => _pending;
    public bool HasCurrentTask => _hasCurrent;
    public NightShiftTaskId CurrentTask => _current;
    public IReadOnlyList<NightShiftTaskId> PlayerOrder => _playerOrder;
    public int CompletedCount => _completed.Count;
    public NightShiftScriptParams Params => _p;

    public void AdvanceSimTime(float deltaSimMinutes)
    {
        _simTime += deltaSimMinutes;
    }

    public void ProcessNewTriggers()
    {
        TryTrigger(NightShiftTaskId.C, _p.TriggerC, ref _triggeredC);
        TryTrigger(NightShiftTaskId.B, _p.TriggerB, ref _triggeredB);
        TryTrigger(NightShiftTaskId.A, _p.TriggerA, ref _triggeredA);
    }

    private void TryTrigger(NightShiftTaskId id, float triggerTime, ref bool done)
    {
        if (done)
            return;
        if (_simTime < triggerTime)
            return;
        done = true;
        _pending.Add(id);
        _log($"t={_simTime:F2}: trigger {id}");
    }

    /// <summary>Call after Advance + triggers each frame. Completes current task if duration elapsed.</summary>
    public void TickHandling()
    {
        if (!_hasCurrent)
            return;
        float duration = GetDuration(_current);
        if (_simTime >= _currentStartTime + duration)
            CompleteCurrentTask();
    }

    /// <returns>False if selection blocked; reason in <paramref name="blockReason"/>.</returns>
    public bool TrySelectTask(NightShiftTaskId id, out string blockReason)
    {
        blockReason = null;
        if (_hasCurrent)
        {
            blockReason = $"blocked {id}: another task is running ({_current})";
            return false;
        }
        if (!_pending.Contains(id))
        {
            blockReason = $"blocked {id}: not in pending list";
            return false;
        }
        if (_completed.Contains(id))
        {
            blockReason = $"blocked {id}: already completed";
            return false;
        }

        _hasCurrent = true;
        _current = id;
        _currentStartTime = _simTime;
        _playerOrder.Add(id);
        _pending.Remove(id);
        _log($"start handling {id} at t={_simTime:F2}");
        return true;
    }

    private void CompleteCurrentTask()
    {
        NightShiftTaskId id = _current;
        if (!_completed.Contains(id))
            _completed.Add(id);
        _hasCurrent = false;
        _log($"complete {id} at t={_simTime:F2}");
    }

    /// <summary>
    /// When all three tasks completed: <paramref name="resultPanel"/> for main UI (research-style block);
    /// <paramref name="summaryMultiline"/> for full numbers / logs.
    /// </summary>
    /// <param name="simTimeAtEnd">Clock value when the night ends (with fast mode: ~elapsed wall seconds).</param>
    /// <param name="fastTimeEqualsWallSeconds">True when Runner uses 1 real second = 1 unit on this clock (label "s").</param>
    public bool TryBuildEndSummary(
        float simTimeAtEnd,
        bool fastTimeEqualsWallSeconds,
        out string resultPanel,
        out string summaryMultiline,
        out string summaryOneLineForLog)
    {
        resultPanel = null;
        summaryMultiline = null;
        summaryOneLineForLog = null;
        if (_completed.Count != 3)
            return false;

        float playerLoss = ComputeTotalLoss(_playerOrder);
        GetOptimal(out string bestOrder, out float bestLoss);
        float regret = playerLoss - bestLoss;

        string timeUnit = fastTimeEqualsWallSeconds ? "s" : "sim min";
        string orderText = OrderToString(_playerOrder);
        resultPanel =
            "Result:\n" +
            $"Total Time: {simTimeAtEnd:F1} {timeUnit}\n" +
            $"Regret: {regret:F1}\n" +
            $"Order: {orderText}\n" +
            $"Optimal: {bestOrder}";

        string condition = !string.IsNullOrWhiteSpace(_p.ConditionName)
            ? _p.ConditionName.Trim()
            : DeriveConditionDisplayName(_p.ScriptLabel);
        string triggerOrder =
            $"{_p.TriggerC:F0}, {_p.TriggerB:F0}, {_p.TriggerA:F0}";
        summaryMultiline =
            $"Condition: {condition}\n" +
            $"Trigger order: {triggerOrder}\n" +
            $"Your order: {orderText}\n" +
            $"Your TotalLoss: {playerLoss:F1}\n" +
            $"Optimal order: {bestOrder}\n" +
            $"Optimal TotalLoss: {bestLoss:F1}\n" +
            $"Regret: {regret:F1}";
        summaryOneLineForLog =
            "[NightShift] " + resultPanel.Replace("\n", " | ") + " || " + summaryMultiline.Replace("\n", " | ");
        return true;
    }

    /// <summary>Uses text before " T=" in scriptLabel (e.g. FastTrigger T=1,2,3 → FastTrigger); otherwise full label.</summary>
    private static string DeriveConditionDisplayName(string scriptLabel)
    {
        if (string.IsNullOrWhiteSpace(scriptLabel))
            return "—";
        const string marker = " T=";
        int idx = scriptLabel.IndexOf(marker, StringComparison.Ordinal);
        if (idx <= 0)
            return scriptLabel.Trim();
        return scriptLabel.Substring(0, idx).Trim();
    }

    public float ComputeTotalLoss(List<NightShiftTaskId> order)
    {
        float endPrev = 0f;
        float total = 0f;

        for (int i = 0; i < order.Count; i++)
        {
            NightShiftTaskId id = order[i];
            float trigger = GetTrigger(id);
            float start = trigger > endPrev ? trigger : endPrev;
            float wait = start > trigger ? start - trigger : 0f;
            total += GetSeverity(id) * wait;
            endPrev = start + GetDuration(id);
        }

        return total;
    }

    public void GetOptimal(out string bestOrderText, out float bestLoss)
    {
        var allOrders = new List<List<NightShiftTaskId>>
        {
            new List<NightShiftTaskId> { NightShiftTaskId.C, NightShiftTaskId.B, NightShiftTaskId.A },
            new List<NightShiftTaskId> { NightShiftTaskId.C, NightShiftTaskId.A, NightShiftTaskId.B },
            new List<NightShiftTaskId> { NightShiftTaskId.B, NightShiftTaskId.C, NightShiftTaskId.A },
            new List<NightShiftTaskId> { NightShiftTaskId.B, NightShiftTaskId.A, NightShiftTaskId.C },
            new List<NightShiftTaskId> { NightShiftTaskId.A, NightShiftTaskId.C, NightShiftTaskId.B },
            new List<NightShiftTaskId> { NightShiftTaskId.A, NightShiftTaskId.B, NightShiftTaskId.C }
        };

        bestLoss = float.MaxValue;
        bestOrderText = "";
        for (int i = 0; i < allOrders.Count; i++)
        {
            float loss = ComputeTotalLoss(allOrders[i]);
            if (loss < bestLoss)
            {
                bestLoss = loss;
                bestOrderText = OrderToString(allOrders[i]);
            }
        }
    }

    private float GetTrigger(NightShiftTaskId id)
    {
        switch (id)
        {
            case NightShiftTaskId.C: return _p.TriggerC;
            case NightShiftTaskId.B: return _p.TriggerB;
            default: return _p.TriggerA;
        }
    }

    private float GetDuration(NightShiftTaskId id)
    {
        switch (id)
        {
            case NightShiftTaskId.C: return _p.DurationC;
            case NightShiftTaskId.B: return _p.DurationB;
            default: return _p.DurationA;
        }
    }

    private int GetSeverity(NightShiftTaskId id)
    {
        switch (id)
        {
            case NightShiftTaskId.C: return _p.SeverityC;
            case NightShiftTaskId.B: return _p.SeverityB;
            default: return _p.SeverityA;
        }
    }

    public static string OrderToString(IReadOnlyList<NightShiftTaskId> order)
    {
        if (order == null || order.Count == 0)
            return "(none)";
        var parts = new string[order.Count];
        for (int i = 0; i < order.Count; i++)
            parts[i] = order[i].ToString();
        return string.Join(" -> ", parts);
    }

    public bool ButtonShouldBeInteractable(NightShiftTaskId id)
    {
        return !_hasCurrent && _pending.Contains(id);
    }
}