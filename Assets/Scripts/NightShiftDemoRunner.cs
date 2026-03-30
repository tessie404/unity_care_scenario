using UnityEngine;
using UnityEngine.UI;

public class NightShiftDemoRunner : MonoBehaviour
{
    [Header("Time")]
    [Tooltip("If true: 1 real second = 1 simulated minute.")]
    [SerializeField] private bool fastMode = true;

    [Header("Script (data)")]
    [Tooltip("Night shift script asset (T, d, S, labels, hover strings).")]
    [SerializeField] private NightShiftScriptConfig scriptConfig;

    [Header("Optional UI Text")]
    [Tooltip("Legacy Text for simulated time display.")]
    [SerializeField] private Text simTimeText;
    [Tooltip("Legacy Text for pending tasks list.")]
    [SerializeField] private Text pendingText;
    [Tooltip("Legacy Text for current handling task.")]
    [SerializeField] private Text currentText;
    [Tooltip("Legacy Text for end summary.")]
    [SerializeField] private Text resultText;

    [Header("Optional — Play hover (needs NightShiftUiPointerHint.cs)")]
    [Tooltip("Inspector: dedicated Legacy Text. Play: pointer over buttons fills this from SO taskHoverHint*.")]
    [SerializeField] private Text hoverHintText;

    [Header("Optional — hide when night ends")]
    [Tooltip("Set inactive when all tasks done (e.g. parent of sim/pending/current lines). Optional.")]
    [SerializeField] private GameObject hudDuringPlay;
    [Tooltip("Set inactive when night ends (e.g. parent of C/B/A buttons). Optional.")]
    [SerializeField] private GameObject taskButtonsRoot;

    [Header("Optional Buttons")]
    [Tooltip("Button for task C.")]
    [SerializeField] private Button buttonC;
    [Tooltip("Button for task B.")]
    [SerializeField] private Button buttonB;
    [Tooltip("Button for task A.")]
    [SerializeField] private Button buttonA;

    [Header("Cursor (only if a 3D controller locks the mouse)")]
    [Tooltip("Not an input permission. If a character controller hides/locks the cursor, keep this on for UI clicks. Off for UI-only scenes.")]
    [SerializeField] private bool unlockCursorEachFrameForUi = true;

    private NightShiftSimulation _sim;
    private bool _resultWritten;
    private bool _nightEnded;

    private void Start()
    {
        if (scriptConfig == null)
        {
            Debug.LogError("[NightShift] Assign Script Config asset.");
            enabled = false;
            return;
        }

        _sim = new NightShiftSimulation(NightShiftScriptParams.From(scriptConfig));

        if (buttonC != null) buttonC.onClick.AddListener(() => _sim.TrySelectTask(NightShiftTaskId.C, out _));
        if (buttonB != null) buttonB.onClick.AddListener(() => _sim.TrySelectTask(NightShiftTaskId.B, out _));
        if (buttonA != null) buttonA.onClick.AddListener(() => _sim.TrySelectTask(NightShiftTaskId.A, out _));

        ApplyOptionalTaskButtonLabels();
        WirePointerHints();

        RefreshUI();
    }

    private void ApplyOptionalTaskButtonLabels()
    {
        TryApplyButtonText(buttonC, scriptConfig.taskButtonLabelC);
        TryApplyButtonText(buttonB, scriptConfig.taskButtonLabelB);
        TryApplyButtonText(buttonA, scriptConfig.taskButtonLabelA);
    }

    private static void TryApplyButtonText(Button button, string label)
    {
        if (button == null || string.IsNullOrWhiteSpace(label))
            return;
        Text t = button.GetComponentInChildren<Text>(true);
        if (t != null)
            t.text = label;
    }

    private void WirePointerHints()
    {
        TryWirePointerHint(buttonC, scriptConfig.taskHoverHintC);
        TryWirePointerHint(buttonB, scriptConfig.taskHoverHintB);
        TryWirePointerHint(buttonA, scriptConfig.taskHoverHintA);
    }

    private void TryWirePointerHint(Button button, string hint)
    {
        if (hoverHintText == null || button == null || string.IsNullOrWhiteSpace(hint))
            return;
        NightShiftUiPointerHint h = button.GetComponent<NightShiftUiPointerHint>();
        if (h == null) h = button.gameObject.AddComponent<NightShiftUiPointerHint>();
        h.Configure(hoverHintText, hint);
    }

    private void Update()
    {
        if (unlockCursorEachFrameForUi)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (!_nightEnded)
        {
            float delta = fastMode ? Time.deltaTime : Time.deltaTime / 60f;
            _sim.AdvanceSimTime(delta);
            _sim.ProcessNewTriggers();
            _sim.TickHandling();

            if (_sim.CompletedCount == 3 && !_resultWritten)
            {
                if (_sim.TryBuildEndSummary(_sim.SimTime, fastMode, out string panel, out string multi, out string line))
                {
                    _resultWritten = true;
                    _nightEnded = true;
                    if (hudDuringPlay != null)
                        hudDuringPlay.SetActive(false);
                    if (taskButtonsRoot != null)
                        taskButtonsRoot.SetActive(false);
                    if (resultText != null)
                        resultText.text = panel;
                    else
                        Debug.LogWarning("[NightShift] resultText not assigned: end summary only in Console. Use Legacy Text (not TMP) in Inspector.");
                    if (!string.IsNullOrEmpty(line))
                        Debug.Log(line);
                    if (!string.IsNullOrEmpty(multi))
                        Debug.Log("[NightShift] detail: " + multi.Replace("\n", " | "));
                }
            }
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        if (_nightEnded)
        {
            if (simTimeText != null)
                simTimeText.text = "";
            if (pendingText != null)
                pendingText.text = "";
            if (currentText != null)
                currentText.text = "";
            if (hoverHintText != null)
                hoverHintText.text = "";
            return;
        }

        if (simTimeText != null)
            simTimeText.text = $"simTime: {_sim.SimTime:F1}";
        if (pendingText != null)
            pendingText.text = $"Pending: {NightShiftSimulation.OrderToString(_sim.Pending)}";
        if (currentText != null)
        {
            if (!_sim.HasCurrentTask)
                currentText.text = "Current: idle";
            else
                currentText.text = $"Current: handling {_sim.CurrentTask}";
        }

        if (buttonC != null) buttonC.interactable = _sim.ButtonShouldBeInteractable(NightShiftTaskId.C);
        if (buttonB != null) buttonB.interactable = _sim.ButtonShouldBeInteractable(NightShiftTaskId.B);
        if (buttonA != null) buttonA.interactable = _sim.ButtonShouldBeInteractable(NightShiftTaskId.A);
    }
}
