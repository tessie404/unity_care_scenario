using UnityEngine;

[CreateAssetMenu(fileName = "NightShiftScript_Baseline", menuName = "NightShift/Script Config", order = 0)]
public class NightShiftScriptConfig : ScriptableObject
{
    [Header("Labels (for logs / result UI)")]
    public string scriptLabel = "Baseline T=3,4,6";
    public string scriptId = "baseline_v1";
    [Tooltip("Shown as \"Condition:\" in end summary. Empty = use text before \" T=\" in scriptLabel, or full scriptLabel.")]
    public string conditionName = "";

    [Header("Task button labels (optional)")]
    public string taskButtonLabelC = "C · bed exit";
    public string taskButtonLabelB = "B · wet floor";
    public string taskButtonLabelA = "A · incontinence";

    [Header("Task hover hints — Play mode (empty = no hint for that button)")]
    [Tooltip("Play: text when pointer hovers button C. Needs Runner Hover Hint Text + NightShiftUiPointerHint.cs.")]
    public string taskHoverHintC = "C: handle bed-exit / fall-risk event.";
    [Tooltip("Play: text when pointer hovers button B. Needs Runner Hover Hint Text + NightShiftUiPointerHint.cs.")]
    public string taskHoverHintB = "B: handle wet floor or spill.";
    [Tooltip("Play: text when pointer hovers button A. Needs Runner Hover Hint Text + NightShiftUiPointerHint.cs.")]
    public string taskHoverHintA = "A: handle incontinence care.";

    [Header("Trigger times (simulated minutes)")]
    public float triggerC = 3f;
    public float triggerB = 4f;
    public float triggerA = 6f;

    [Header("Handling durations (minutes)")]
    public float durationC = 2f;
    public float durationB = 3f;
    public float durationA = 10f;

    [Header("Severity (loss per minute of waiting)")]
    public int severityC = 9;
    public int severityB = 8;
    public int severityA = 6;
}
