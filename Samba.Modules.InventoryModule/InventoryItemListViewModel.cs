﻿using System.Linq;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    class InventoryItemListViewModel : EntityCollectionViewModelBase<InventoryItemViewModel, InventoryItem>
    {
        protected override InventoryItemViewModel CreateNewViewModel(InventoryItem model)
        {
            return new InventoryItemViewModel(model);
        }

        protected override InventoryItem CreateNewModel()
        {
            return new InventoryItem();
        }

        protected override string CanDeleteItem(InventoryItem model)
        {
            var item = Dao.Count<PeriodicConsumptionItem>(x => x.InventoryItem.Id == model.Id);
            if (item > 0) return Resources.DeleteErrorInventoryItemUsedInEndOfDayRecord;
            var item1 = Dao.Count<RecipeItem>(x => x.InventoryItem.Id == model.Id);
            if (item1 > 1) return Resources.DeleteErrorProductUsedInReceipt;
            return base.CanDeleteItem(model);
        }
    }
}
