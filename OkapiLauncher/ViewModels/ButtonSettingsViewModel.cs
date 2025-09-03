using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OkapiLauncher.Controls.Utilities;
using OkapiLauncher.Services;

namespace OkapiLauncher.ViewModels;
public partial class ButtonSettingsViewModel : ObservableObject
{
    public partial class CheckedButton : ObservableObject
    {
        private readonly ButtonSettingsViewModel _parent;
        [ObservableProperty]
        private bool _enabled;
        public VisibleButtons Button { get; }
        public CheckedButton(VisibleButtons button, bool enabled, ButtonSettingsViewModel parent)
        {
            Button = button;
            Enabled = enabled;
            _parent = parent;
        }
        [RelayCommand]
        public void MoveUp()
        {
            _parent.MoveButtonUp(this);
        }
        [RelayCommand]
        public void MoveDown()
        {
            _parent.MoveButtonDown(this);
        }
    }
    [ObservableProperty]
    private bool _showDisabledButtons;
    [ObservableProperty]
    private int _iconSize;
    [ObservableProperty]
    private ButtonSettings _settings;
    private readonly IGeneralSettingsService _generalSettingsService;

    public ObservableCollection<CheckedButton> ButtonOrdering { get; }
    public ButtonSettingsViewModel(IGeneralSettingsService generalSettingsService)
    {
        _showDisabledButtons = generalSettingsService.ButtonSettings.ShowDisabledButtons;
        _iconSize = generalSettingsService.ButtonSettings.IconSize;
        var checkeds = generalSettingsService.ButtonSettings.ListOrder.Select(x => new CheckedButton(x, generalSettingsService.ButtonSettings.VisibleButtons.HasFlag(x),this));
        ButtonOrdering = new ObservableCollection<CheckedButton>(checkeds);
        foreach (var c in ButtonOrdering)
        {
            c.PropertyChanged += ButtonCheckedChanged;
        }
        ButtonOrdering.CollectionChanged += ButtonOrdering_CollectionChanged;
        _generalSettingsService = generalSettingsService;
        UpdateSettings();
    }
   
    private void UpdateSettings()
    {
        if(ButtonOrdering is null)
        {
            return;
        }
        var visible = VisibleButtons.None;
        foreach (var c in ButtonOrdering)
        {
            if (c.Enabled)
            {
                visible |= c.Button;
            }
        }
        Settings = new ButtonSettings()
        {
            ListOrder = ButtonOrdering.Select(x => x.Button),
            VisibleButtons = visible,
            ShowDisabledButtons = ShowDisabledButtons,
            IconSize = IconSize,
        };
        _generalSettingsService.ButtonSettings = Settings;
    }
    partial void OnIconSizeChanged(int value)
    {
        UpdateSettings();
    }
    partial void OnShowDisabledButtonsChanged(bool value)
    {
        UpdateSettings();
    }
    private void ButtonOrdering_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdateSettings();
    }

    private void ButtonCheckedChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        UpdateSettings();
    }

    private void MoveButtonOrdering(CheckedButton button, int move)
    {
        var old = ButtonOrdering.IndexOf(button);
        var newindex = old + move;
        newindex = Math.Clamp(newindex, 0, ButtonOrdering.Count - 1);
        ButtonOrdering.Move(old, newindex);
    }

    internal void MoveButtonUp(CheckedButton button)
    {
        MoveButtonOrdering(button, -1);
    }

    internal void MoveButtonDown(CheckedButton button)
    {
        MoveButtonOrdering(button, +1);
    }
    [RelayCommand]
    private void Reset()
    {
        var checkeds = ButtonSettings.Default.ListOrder.Select(x => new CheckedButton(x, ButtonSettings.Default.VisibleButtons.HasFlag(x), this));
        foreach(var b in ButtonOrdering)
        {
            b.PropertyChanged -= ButtonCheckedChanged;
        }
        ButtonOrdering.Clear();
        foreach(var b in checkeds)
        {
            ButtonOrdering.Add(b);
            b.PropertyChanged += ButtonCheckedChanged;
        }
        ShowDisabledButtons = ButtonSettings.Default.ShowDisabledButtons;
        IconSize = ButtonSettings.Default.IconSize;
    }
}
