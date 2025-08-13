using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OkapiLauncher.Contracts.Services;
using OkapiLauncher.Controls.Utilities;
using OkapiLauncher.Helpers;
using OkapiLauncher.Models;

namespace OkapiLauncher.Services
{
    class GeneralSettingsService : IGeneralSettingsService
    {
        private readonly IPersistAndRestoreService _persistAndRestoreService;
        private ButtonSettings _buttonSettings = ButtonSettings.Default;
        private bool _initialized;

        public GeneralSettingsService(IPersistAndRestoreService persistAndRestoreService)
        {
            _persistAndRestoreService = persistAndRestoreService;
            if (_persistAndRestoreService.IsDataRestored)
            {
                // if already restored, get
                InitializeData();
            }
            else
            {
                // otherwise wait for restore
                _persistAndRestoreService.DataRestored += PersistAndRestoreService_DataRestored;
            }
        }

        private void PersistAndRestoreService_DataRestored(object? sender, EventArgs e)
        {
            InitializeData();
        }

        private void InitializeData()
        {
            if (!_initialized && _persistAndRestoreService.IsDataRestored)
            {
                App.Current.Properties.InitializeDictKey<ButtonSettings>(nameof(ButtonSettings),converter:null,ButtonSettings.Default);
                _initialized = true;
            }
        }
        public ButtonSettings ButtonSettings
        {
            get => (ButtonSettings)App.Current.Properties[nameof(ButtonSettings)]!;

            set
            {
                App.Current.Properties[nameof(ButtonSettings)] = value;
            }
        }
    }
}
