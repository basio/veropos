﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Inventory;
using Samba.Infrastructure.Data;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Modules.InventoryModule
{
    class RecipeItemViewModel : ObservableObject
    {
        private readonly IWorkspace _workspace;

        public RecipeItemViewModel(RecipeItem model, IWorkspace workspace)
        {
            Model = model;
            _workspace = workspace;
        }

        public RecipeItem Model { get; set; }
        public InventoryItem InventoryItem { get { return Model.InventoryItem; } set { Model.InventoryItem = value; } }

        private IEnumerable<string> _inventoryItemNames;
        public IEnumerable<string> InventoryItemNames
        {
            get { return _inventoryItemNames ?? (_inventoryItemNames = AppServices.DataAccessService.GetInventoryItemNames()); }
        }

        public string Name
        {
            get
            {
                return Model.InventoryItem != null ? Model.InventoryItem.Name : string.Format("- {0} -", Localization.Properties.Resources.Select);
            }
            set
            {
                UpdateInventoryItem(value);
                RaisePropertyChanged("Name");
                RaisePropertyChanged("UnitName");
            }
        }

        public string UnitName
        {
            get
            {
                return Model.InventoryItem != null ? Model.InventoryItem.BaseUnit : "";
            }
        }

        public decimal Quantity
        {
            get { return Model.Quantity; }
            set
            {
                Model.Quantity = value;
                RaisePropertyChanged("Quantity");
            }
        }

        private void UpdateInventoryItem(string value)
        {
            var i = _workspace.Single<InventoryItem>(x => x.Name.ToLower() == value.ToLower());
            InventoryItem = i;
        }
    }
}
