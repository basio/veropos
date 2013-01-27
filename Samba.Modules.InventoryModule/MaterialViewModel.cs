﻿using System.Collections.Generic;
using Samba.Domain.Models.Inventory;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    public class MaterialViewModel : ObservableObject
    {
        public InventoryItem Model { get; set; }
        private readonly IWorkspace _workspace;

        public MaterialViewModel(InventoryItem model, IWorkspace workspace)
        {
            Model = model;
            _workspace = workspace;
        }

        private IEnumerable<string> _inventoryItemNames;
        public IEnumerable<string> InventoryItemNames
        {
            get { return _inventoryItemNames ?? (_inventoryItemNames = AppServices.DataAccessService.GetInventoryItemNames()); }
        }

        public string Name
        {
            get
            {
                return Model != null ? Model.Name : string.Format("- {0} -", Resources.Select);
            }
            set
            {
                UpdateInventoryItem(value);
                RaisePropertyChanged("Name");
            }
        }

        private void UpdateInventoryItem(string name)
        {
            Model = _workspace.Single<InventoryItem>(x => x.Name.ToLower() == name.ToLower());
        }
    }
}
