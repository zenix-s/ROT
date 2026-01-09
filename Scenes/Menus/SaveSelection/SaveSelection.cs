using Godot;
using RotOfTime.Autoload;
using RotOfTime.Core;
using RotOfTime.Core.GameData;

namespace RotOfTime.Scenes.Menus.SaveSelection;

public partial class SaveSelection : Node2D
{
    private Button _backButton;
    private SaveSlotUI[] _slots;
    private VBoxContainer _slotsContainer;

    public override void _Ready()
    {
        _slotsContainer = GetNode<VBoxContainer>("CanvasLayer/CenterContainer/VBoxContainer/SlotsContainer");
        _backButton = GetNode<Button>("CanvasLayer/CenterContainer/VBoxContainer/BackButton");

        SetupSlots();
        RefreshSlots();

        if (_backButton != null)
            _backButton.Pressed += OnBackPressed;
    }

    private void SetupSlots()
    {
        _slots = new SaveSlotUI[3];
        for (int i = 0; i < 3; i++)
        {
            if (_slotsContainer?.GetChild(i) is not Control slotNode) continue;

            _slots[i] = new SaveSlotUI(slotNode, i + 1);
            _slots[i].SlotSelected += OnSlotSelected;
            _slots[i].DeleteRequested += OnDeleteRequested;
        }
    }

    private void RefreshSlots()
    {
        var slotData = SaveManager.Instance.GetAllSlotInfo();
        for (int i = 0; i < _slots.Length; i++) _slots[i]?.UpdateDisplay(slotData[i]);
    }

    private void OnSlotSelected(int slotId, bool hasData)
    {
        if (hasData)
            GameManager.Instance.Load(slotId);
        else
            GameManager.Instance.NewGame(slotId);

        // Transition to game based on current tower level
        SceneExtensionManager.TowerLevel level = GameManager.Instance.Data.CurrentLevel;
        SceneManager.Instance.RequestSceneChange(SceneExtensionManager.TowerLevelToGameScene(level));
    }

    private void OnDeleteRequested(int slotId)
    {
        SaveManager.Instance.DeleteSave(slotId);
        RefreshSlots();
    }

    private void OnBackPressed()
    {
        SceneManager.Instance.RequestMenuChange(SceneExtensionManager.MenuScene.Start);
    }
}

public partial class SaveSlotUI : GodotObject
{
    [Signal]
    public delegate void DeleteRequestedEventHandler(int slotId);

    [Signal]
    public delegate void SlotSelectedEventHandler(int slotId, bool hasData);

    private readonly Button _deleteButton;
    private readonly Label _infoLabel;

    private readonly Control _root;
    private readonly Button _selectButton;
    private readonly int _slotId;
    private readonly Label _slotLabel;
    private bool _hasData;

    public SaveSlotUI(Control root, int slotId)
    {
        _root = root;
        _slotId = slotId;

        // Node structure: Slot -> HBoxContainer -> VBoxContainer/ButtonContainer -> Children
        _slotLabel = root.GetNodeOrNull<Label>("HBoxContainer/VBoxContainer/SlotLabel");
        _infoLabel = root.GetNodeOrNull<Label>("HBoxContainer/VBoxContainer/InfoLabel");
        _selectButton = root.GetNodeOrNull<Button>("HBoxContainer/ButtonContainer/SelectButton");
        _deleteButton = root.GetNodeOrNull<Button>("HBoxContainer/ButtonContainer/DeleteButton");

        if (_selectButton != null)
            _selectButton.Pressed += OnSelectPressed;

        if (_deleteButton != null)
            _deleteButton.Pressed += OnDeletePressed;
    }

    public void UpdateDisplay(GameData data)
    {
        _hasData = data != null;

        if (_slotLabel != null)
            _slotLabel.Text = $"Slot {_slotId}";

        if (_infoLabel != null)
        {
            if (data != null)
                _infoLabel.Text =
                    $"{data.CurrentLevel}\nTime: {data.GetFormattedPlayTime()}\nLast: {data.GetFormattedLastSaved()}";
            else
                _infoLabel.Text = "Empty";
        }

        if (_selectButton != null)
            _selectButton.Text = data != null ? "Continue" : "New Game";

        if (_deleteButton != null)
            _deleteButton.Visible = data != null;
    }

    private void OnSelectPressed()
    {
        EmitSignal(SignalName.SlotSelected, _slotId, _hasData);
    }

    private void OnDeletePressed()
    {
        EmitSignal(SignalName.DeleteRequested, _slotId);
    }
}
