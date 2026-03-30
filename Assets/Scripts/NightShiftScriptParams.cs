using System;
using UnityEngine;

/// <summary>Immutable snapshot of one night script (T, d, S, labels).</summary>
public readonly struct NightShiftScriptParams
{
    public string ScriptLabel { get; }
    public string ScriptId { get; }
    /// <summary>Overrides condition line in result UI when non-empty.</summary>
    public string ConditionName { get; }
    public float TriggerC { get; }
    public float TriggerB { get; }
    public float TriggerA { get; }
    public float DurationC { get; }
    public float DurationB { get; }
    public float DurationA { get; }
    public int SeverityC { get; }
    public int SeverityB { get; }
    public int SeverityA { get; }

    public NightShiftScriptParams(
        string scriptLabel, string scriptId, string conditionName,
        float triggerC, float triggerB, float triggerA,
        float durationC, float durationB, float durationA,
        int severityC, int severityB, int severityA)
    {
        ScriptLabel = scriptLabel;
        ScriptId = scriptId;
        ConditionName = conditionName ?? "";
        TriggerC = triggerC;
        TriggerB = triggerB;
        TriggerA = triggerA;
        DurationC = durationC;
        DurationB = durationB;
        DurationA = durationA;
        SeverityC = severityC;
        SeverityB = severityB;
        SeverityA = severityA;
    }

    public static NightShiftScriptParams From(NightShiftScriptConfig c)
    {
        if (c == null)
            throw new ArgumentNullException(nameof(c));
        return new NightShiftScriptParams(
            c.scriptLabel, c.scriptId, c.conditionName,
            c.triggerC, c.triggerB, c.triggerA,
            c.durationC, c.durationB, c.durationA,
            c.severityC, c.severityB, c.severityA);
    }
}