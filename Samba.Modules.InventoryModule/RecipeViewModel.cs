﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using System.Linq;

namespace Samba.Modules.InventoryModule
{
    class RecipeViewModel : EntityViewModelBase<Recipe>
    {
        public RecipeViewModel(Recipe model)
            : base(model)
        {
            AddInventoryItemCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Inventory), OnAddInventoryItem, CanAddInventoryItem);
            DeleteInventoryItemCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Inventory), OnDeleteInventoryItem, CanDeleteInventoryItem);
        }


        public override Type GetViewType()
        {
            return typeof(RecipeView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Recipe;
        }

        public ICaptionCommand AddInventoryItemCommand { get; set; }
        public ICaptionCommand DeleteInventoryItemCommand { get; set; }

        private ObservableCollection<RecipeItemViewModel> _recipeItems;
        public ObservableCollection<RecipeItemViewModel> RecipeItems
        {
            get { return _recipeItems ?? (_recipeItems = new ObservableCollection<RecipeItemViewModel>(Model.RecipeItems.Select(x => new RecipeItemViewModel(x, _workspace)))); }
        }

        private RecipeItemViewModel _selectedRecipeItem;
        public RecipeItemViewModel SelectedRecipeItem
        {
            get { return _selectedRecipeItem; }
            set
            {
                _selectedRecipeItem = value;
                RaisePropertyChanged("SelectedRecipeItem");
            }
        }

        private string _selectedMenuItemName;
        public string SelectedMenuItemName
        {
            get { return _selectedMenuItemName; }
            set
            {
                _selectedMenuItemName = value;
                if (SelectedMenuItem == null || SelectedMenuItem.Name != value)
                {
                    var mi = _workspace.Single<MenuItem>(x => x.Name.ToLower() == _selectedMenuItemName.ToLower());
                    SelectedMenuItem = mi;
                    if (mi != null && mi.Portions.Count == 1)
                        Portion = mi.Portions[0];
                }
                RaisePropertyChanged("SelectedMenuItemName");
            }
        }

        private IEnumerable<string> _menuItemNames;
        public IEnumerable<string> MenuItemNames
        {
            get { return _menuItemNames ?? (_menuItemNames = Dao.Select<MenuItem, string>(x => x.Name, null)); }
        }

        private MenuItem _selectedMenuItem;
        private IWorkspace _workspace;

        public MenuItem SelectedMenuItem
        {
            get { return GetMenuItem(); }
            set
            {
                _selectedMenuItem = value;
                if (value != null)
                { SelectedMenuItemName = value.Name; }
                else Portion = null;
                RaisePropertyChanged("SelectedMenuItem");
            }
        }

        private MenuItem GetMenuItem()
        {
            if (_selectedMenuItem == null)
            {
                if (Model.Portion != null)
                    SelectedMenuItem = _workspace.Single<MenuItem>(x => x.Id == Model.Portion.MenuItemId);
            }
            return _selectedMenuItem;
        }

        public MenuItemPortion Portion
        {
            get { return Model.Portion; }
            set
            {
                Model.Portion = value;
                RaisePropertyChanged("Portion");
            }
        }

        public decimal FixedCost
        {
            get { return Model.FixedCost; }
            set { Model.FixedCost = value; }
        }

        protected override void Initialize(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        private void OnDeleteInventoryItem(string obj)
        {
            if (SelectedRecipeItem != null)
            {
                if (SelectedRecipeItem.Model.Id > 0)
                    _workspace.Delete(SelectedRecipeItem.Model);
                Model.RecipeItems.Remove(SelectedRecipeItem.Model);
                RecipeItems.Remove(SelectedRecipeItem);
            }
        }

        private void OnAddInventoryItem(string obj)
        {
            var ri = new RecipeItem();
            Model.RecipeItems.Add(ri);
            var riv = new RecipeItemViewModel(ri, _workspace);
            RecipeItems.Add(riv);
            SelectedRecipeItem = riv;
        }

        private bool CanAddInventoryItem(string arg)
        {
            return Portion != null;
        }

        private bool CanDeleteInventoryItem(string arg)
        {
            return SelectedRecipeItem != null;
        }

        protected override string GetSaveErrorMessage()
        {
            if (Model.RecipeItems.Any(x => x.InventoryItem == null || x.Quantity == 0))
                return Resources.SaveErrorZeroOrNullInventoryLines;
            if (Model.Portion == null)
                return Resources.APortionShouldSelected;
            var count = Dao.Count<Recipe>(x => x.Portion.Id == Model.Portion.Id && x.Id != Model.Id);
            if (count > 0)
                return string.Format(Resources.ThereIsAnotherRecipeFor_f, SelectedMenuItem.Name);
            return base.GetSaveErrorMessage();
        }
    }
}
