﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Samba.Domain.Models.Inventory;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;

namespace Samba.Modules.MenuModule
{
    public class MenuItemViewModel : EntityViewModelBase<MenuItem>
    {
        private IWorkspace _workspace;

        private IEnumerable<string> _groupCodes;
        public IEnumerable<string> GroupCodes { get { return _groupCodes ?? (_groupCodes = Dao.Distinct<MenuItem>(x => x.GroupCode)); } }

        private IEnumerable<string> _tags;
        public IEnumerable<string> Tags { get { return _tags ?? (_tags = Dao.Distinct<MenuItem>(x => x.Tag)); } }

        private ObservableCollection<PortionViewModel> _portions;
        public ObservableCollection<PortionViewModel> Portions
        {
            get { return _portions ?? (_portions = new ObservableCollection<PortionViewModel>(GetPortions(Model))); }
        }

        private ObservableCollection<MenuItemPropertyGroupViewModel> _propertyGroups;
        public ObservableCollection<MenuItemPropertyGroupViewModel> PropertyGroups
        {
            get { return _propertyGroups ?? (_propertyGroups = new ObservableCollection<MenuItemPropertyGroupViewModel>(GetProperties(Model))); }
        }

        private IEnumerable<VatTemplateViewModel> _vatTemplates;
        public IEnumerable<VatTemplateViewModel> VatTemplates
        {
            get { return _vatTemplates ?? (_vatTemplates = _workspace.All<VatTemplate>().Select(x => new VatTemplateViewModel(x))); }
        }

        public VatTemplate VatTemplate { get { return Model.VatTemplate; } set { Model.VatTemplate = value; } }

        public PortionViewModel SelectedPortion { get; set; }

        public ICaptionCommand AddPortionCommand { get; set; }
        public ICaptionCommand DeletePortionCommand { get; set; }

        public MenuItemPropertyGroupViewModel SelectedProperty { get; set; }

        public ICaptionCommand AddPropertyGroupCommand { get; set; }
        public ICaptionCommand DeletePropertyGroupCommand { get; set; }

        public string GroupCode
        {
            get { return Model.GroupCode ?? ""; }
            set { Model.GroupCode = value; }
        }

        public string Tag
        {
            get { return Model.Tag ?? ""; }
            set { Model.Tag = value; }
        }

        public string Barcode
        {
            get { return Model.Barcode ?? ""; }
            set { Model.Barcode = value; }
        }

        public MenuItemViewModel(MenuItem model)
            : base(model)
        {
            AddPortionCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.Portion), OnAddPortion);
            DeletePortionCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.Portion), OnDeletePortion, CanDeletePortion);

            AddPropertyGroupCommand = new CaptionCommand<string>(string.Format(Resources.Add_f, Resources.PropertyGroup), OnAddPropertyGroup);
            DeletePropertyGroupCommand = new CaptionCommand<string>(string.Format(Resources.Delete_f, Resources.PropertyGroup), OnDeletePropertyGroup, CanDeletePropertyGroup);
        }

        private bool CanDeletePropertyGroup(string arg)
        {
            return SelectedProperty != null;
        }

        private void OnDeletePropertyGroup(string obj)
        {
            Model.PropertyGroups.Remove(SelectedProperty.Model);
            PropertyGroups.Remove(SelectedProperty);
        }

        private void OnAddPropertyGroup(string obj)
        {
            var selectedValues =
                InteractionService.UserIntraction.ChooseValuesFrom(_workspace.All<MenuItemPropertyGroup>().ToList<IOrderable>(),
                Model.PropertyGroups.ToList<IOrderable>(), Resources.PropertyGroups, string.Format(Resources.SelectPropertyGroupsHint_f, Model.Name),
                Resources.PropertyGroup, Resources.PropertyGroups);

            foreach (MenuItemPropertyGroup selectedValue in selectedValues)
            {
                if (!Model.PropertyGroups.Contains(selectedValue))
                    Model.PropertyGroups.Add(selectedValue);
            }

            _propertyGroups = new ObservableCollection<MenuItemPropertyGroupViewModel>(GetProperties(Model));

            RaisePropertyChanged("PropertyGroups");
        }


        public string GroupValue { get { return Model.GroupCode; } }

        private void OnAddPortion(string value)
        {
            var portion = MenuItem.AddDefaultMenuPortion(Model);
            Portions.Add(new PortionViewModel(portion));
        }

        private void OnDeletePortion(string value)
        {
            if (SelectedPortion != null)
            {
                var c = Dao.Count<Recipe>(x => x.Portion.Id == SelectedPortion.Model.Id);
                if (c == 0)
                {
                    if (SelectedPortion.Model.Id > 0 && Model.Id > 0)
                        _workspace.Delete(SelectedPortion.Model);
                    Model.Portions.Remove(SelectedPortion.Model);
                    Portions.Remove(SelectedPortion);
                }
            }
        }

        private bool CanDeletePortion(string value)
        {
            return SelectedPortion != null;
        }

        public override string GetModelTypeString()
        {
            return Resources.Product;
        }

        protected override void Initialize(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        public override Type GetViewType()
        {
            return typeof(MenuItemView);
        }

        private static IEnumerable<PortionViewModel> GetPortions(MenuItem baseModel)
        {
            return baseModel.Portions.Select(item => new PortionViewModel(item));
        }

        private static IEnumerable<MenuItemPropertyGroupViewModel> GetProperties(MenuItem model)
        {
            return model.PropertyGroups.Select(item => new MenuItemPropertyGroupViewModel(item));
        }

        protected override string GetSaveErrorMessage()
        {
            return base.GetSaveErrorMessage();
        }
    }
}
