﻿using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;
using System.Linq;

namespace Samba.Modules.MenuModule
{
    public class MenuItemListViewModel : EntityCollectionViewModelBase<MenuItemViewModel, MenuItem>
    {
        public MenuItemListViewModel()
        {
            CreateBatchMenuItems = new CaptionCommand<string>(Resources.BatchCreteProducts, OnCreateBatchMenuItems);
            CustomCommands.Add(CreateBatchMenuItems);
        }

        public ICaptionCommand CreateBatchMenuItems { get; set; }

        private void OnCreateBatchMenuItems(string value)
        {
            var values = InteractionService.UserIntraction.GetStringFromUser(
                Resources.BatchCreteProducts,
                Resources.BatchCreateProductsDialogHint);

            var createdItems = new DataCreationService().BatchCreateMenuItems(values, Workspace);
            Workspace.CommitChanges();

            foreach (var mi in createdItems)
            {
                var mv = CreateNewViewModel(mi);
                mv.Init(Workspace);
                Items.Add(mv);
            }
        }

        protected override System.Collections.Generic.IEnumerable<MenuItem> SelectItems()
        {
            return Workspace.All<MenuItem>();
        }

        protected override MenuItemViewModel CreateNewViewModel(MenuItem model)
        {
            return new MenuItemViewModel(model);
        }

        protected override MenuItem CreateNewModel()
        {
            return MenuItem.Create();
        }

        protected override string CanDeleteItem(MenuItem model)
        {
            var count = Dao.Count<ScreenMenuItem>(x => x.MenuItemId == model.Id);
            if (count > 0) return Resources.DeleteErrorProductUsedInMenu;
            if (count == 0) count = Dao.Count<Recipe>(x => x.Portion.MenuItemId == model.Id);
            if (count > 0) return Resources.DeleteErrorProductUsedInReceipt;
            count = Dao.Count<MenuItemProperty>(x => x.MenuItemId == model.Id);
            if (count > 0) return Resources.DeleteErrorProductUsedInMenuItemProperty;
            count = Dao.Count<PrinterMap>(x => x.MenuItem.Id == model.Id);
            if (count > 0) return Resources.DeleteErrorProductUsedInPrinterMap;
            return base.CanDeleteItem(model);
        }
    }
}
