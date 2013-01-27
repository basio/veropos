﻿using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Practices.Prism.Commands;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Presentation.Terminal
{
    public delegate void TagUpdatedEventHandler(TicketTagGroup item);

    public class SelectedTicketItemEditorViewModel : ObservableObject
    {
        public event TagUpdatedEventHandler TagUpdated;

        public TicketItemViewModel SelectedItem
        {
            get { return DataContext.SelectedTicketItem; }
            private set { DataContext.SelectedTicketItem = value; }
        }

        public DelegateCommand<MenuItemPortionViewModel> PortionSelectedCommand { get; set; }
        public DelegateCommand<MenuItemPropertyViewModel> PropertySelectedCommand { get; set; }
        public DelegateCommand<MenuItemGroupedPropertyItemViewModel> PropertyGroupSelectedCommand { get; set; }


        public DelegateCommand<TicketTagViewModel> TicketTagSelectedCommand { get; set; }
        public ICaptionCommand AddTicketTagCommand { get; set; }

        public ObservableCollection<MenuItemPortionViewModel> SelectedItemPortions { get; set; }
        public ObservableCollection<MenuItemPropertyGroupViewModel> SelectedItemPropertyGroups { get; set; }
        public ObservableCollection<MenuItemGroupedPropertyViewModel> SelectedItemGroupedPropertyItems { get; set; }

        public ObservableCollection<TicketTagViewModel> TicketTags { get; set; }

        private string _customTag;
        public string CustomTag
        {
            get { return _customTag; }
            set
            {
                _customTag = value;
                RaisePropertyChanged("CustomTag");
            }
        }

        public int TagColumnCount { get { return TicketTags.Count % 7 == 0 ? TicketTags.Count / 7 : (TicketTags.Count / 7) + 1; } }

        public SelectedTicketItemEditorViewModel()
        {
            SelectedItemPortions = new ObservableCollection<MenuItemPortionViewModel>();
            SelectedItemPropertyGroups = new ObservableCollection<MenuItemPropertyGroupViewModel>();
            SelectedItemGroupedPropertyItems = new ObservableCollection<MenuItemGroupedPropertyViewModel>();
            TicketTags = new ObservableCollection<TicketTagViewModel>();

            PortionSelectedCommand = new DelegateCommand<MenuItemPortionViewModel>(OnPortionSelected);
            PropertySelectedCommand = new DelegateCommand<MenuItemPropertyViewModel>(OnPropertySelected);
            PropertyGroupSelectedCommand = new DelegateCommand<MenuItemGroupedPropertyItemViewModel>(OnPropertyGroupSelected);
            TicketTagSelectedCommand = new DelegateCommand<TicketTagViewModel>(OnTicketTagSelected);
            AddTicketTagCommand = new CaptionCommand<string>(Resources.AddTag, OnTicketTagAdded, CanAddTicketTag);
        }

        public void InvokeTagUpdated(TicketTagGroup item)
        {
            TagUpdatedEventHandler handler = TagUpdated;
            if (handler != null) handler(item);
        }

        private bool CanAddTicketTag(string arg)
        {
            return !string.IsNullOrEmpty(CustomTag);
        }

        private void OnTicketTagAdded(string obj)
        {
            var cachedTag = AppServices.MainDataContext.SelectedDepartment.TicketTagGroups.Single(
                x => x.Id == SelectedTicketTag.Id);
            Debug.Assert(cachedTag != null);
            var ctag = cachedTag.TicketTags.SingleOrDefault(x => x.Name.ToLower() == CustomTag.ToLower());
            if (ctag == null && cachedTag.SaveFreeTags)
            {
                using (var workspace = WorkspaceFactory.Create())
                {
                    var tt = workspace.Single<TicketTagGroup>(x => x.Id == SelectedTicketTag.Id);
                    Debug.Assert(tt != null);
                    var tag = tt.TicketTags.SingleOrDefault(x => x.Name.ToLower() == CustomTag.ToLower());
                    if (tag == null)
                    {
                        tag = new TicketTag() { Name = CustomTag };
                        tt.TicketTags.Add(tag);
                        workspace.Add(tag);
                        workspace.CommitChanges();
                    }
                }
            }
            DataContext.SelectedTicket.UpdateTag(SelectedTicketTag, new TicketTag { Name = CustomTag });
            CustomTag = string.Empty;
            InvokeTagUpdated(SelectedTicketTag);
        }

        public string TicketNote
        {
            get { return DataContext.SelectedTicket != null ? DataContext.SelectedTicket.Note : ""; }
            set { DataContext.SelectedTicket.Note = value; }
        }

        public bool IsTicketNoteVisible { get { return SelectedTicketTag == null && SelectedItem == null && DataContext.SelectedTicket != null; } }
        public bool IsTagEditorVisible { get { return TicketTags.Count > 0 && SelectedItem == null && DataContext.SelectedTicket != null; } }
        public bool IsFreeTagEditorVisible { get { return SelectedTicketTag != null && SelectedTicketTag.FreeTagging; } }

        public bool IsEditorsVisible
        {
            get { return SelectedItem != null; }
        }

        public bool IsPortionsVisible
        {
            get
            {
                return SelectedItem != null
                    && !SelectedItem.IsVoided
                    && !SelectedItem.IsLocked
                    && SelectedItem.Model.PortionCount > 1;
            }
        }

        private bool _isKeyboardVisible;

        public bool IsKeyboardVisible
        {
            get { return _isKeyboardVisible; }
            set { _isKeyboardVisible = value; RaisePropertyChanged("IsKeyboardVisible"); }
        }

        public TicketTagGroup SelectedTicketTag { get; set; }

        public void ShowKeyboard()
        {
            IsKeyboardVisible = true;
        }

        public void HideKeyboard()
        {
            IsKeyboardVisible = false;
        }

        public void Refresh(TicketItemViewModel ticketItem, TicketTagGroup selectedTicketTag)
        {
            HideKeyboard();
            SelectedTicketTag = null;
            SelectedItemPortions.Clear();
            SelectedItemPropertyGroups.Clear();
            SelectedItemGroupedPropertyItems.Clear();
            TicketTags.Clear();

            SelectedItem = ticketItem;

            if (ticketItem != null)
            {
                var mi = AppServices.DataAccessService.GetMenuItem(ticketItem.Model.MenuItemId);
                if (mi.Portions.Count > 1) SelectedItemPortions.AddRange(mi.Portions.Select(x => new MenuItemPortionViewModel(x)));

                SelectedItemGroupedPropertyItems.AddRange(mi.PropertyGroups.Where(x => !string.IsNullOrEmpty(x.GroupTag))
                    .GroupBy(x => x.GroupTag)
                    .Select(x => new MenuItemGroupedPropertyViewModel(SelectedItem, x)));

                SelectedItemPropertyGroups.AddRange(mi.PropertyGroups
                    .Where(x => string.IsNullOrEmpty(x.GroupTag))
                    .Select(x => new MenuItemPropertyGroupViewModel(x)));

                foreach (var ticketItemPropertyViewModel in ticketItem.Properties.ToList())
                {
                    var tip = ticketItemPropertyViewModel;
                    var mig = SelectedItemPropertyGroups.Where(x => x.Model.Id == tip.Model.PropertyGroupId).SingleOrDefault();
                    if (mig != null) mig.Properties.SingleOrDefault(x => x.Name == tip.Model.Name).TicketItemProperty = ticketItemPropertyViewModel.Model;
                    
                    var sig = SelectedItemGroupedPropertyItems.SelectMany(x => x.Properties).Where(
                            x => x.MenuItemPropertyGroup.Id == tip.Model.PropertyGroupId).FirstOrDefault();
                    if (sig != null) 
                        sig.TicketItemProperty = ticketItemPropertyViewModel.Model;
                }
            }
            else
            {
                if (selectedTicketTag != null)
                {
                    SelectedTicketTag = selectedTicketTag;

                    if (selectedTicketTag.FreeTagging)
                    {
                        TicketTags.AddRange(Dao.Query<TicketTagGroup>(x => x.Id == selectedTicketTag.Id, x => x.TicketTags).SelectMany(x => x.TicketTags).OrderBy(x => x.Name).Select(x => new TicketTagViewModel(x)));
                    }
                    else
                    {
                        TicketTags.AddRange(selectedTicketTag.TicketTags.Select(x => new TicketTagViewModel(x)));
                    }
                    RaisePropertyChanged("TicketTags");
                }
                else
                {
                    RaisePropertyChanged("TicketNote");
                    ShowKeyboard();
                }
            }

            RaisePropertyChanged("SelectedItem");
            RaisePropertyChanged("IsPortionsVisible");
            RaisePropertyChanged("IsEditorsVisible");
            RaisePropertyChanged("IsTicketNoteVisible");
            RaisePropertyChanged("IsTagEditorVisible");
            RaisePropertyChanged("IsFreeTagEditorVisible");
            RaisePropertyChanged("TagColumnCount");
        }

        public void CloseView()
        {
            SelectedItem = null;
            SelectedItemPortions.Clear();
            SelectedItemPropertyGroups.Clear();
            SelectedItemGroupedPropertyItems.Clear();
            TicketTags.Clear();
            SelectedTicketTag = null;
        }

        private void OnPortionSelected(MenuItemPortionViewModel obj)
        {
            SelectedItem.UpdatePortion(obj.Model, AppServices.MainDataContext.SelectedDepartment.PriceTag);
            foreach (var model in SelectedItemPortions)
            {
                model.Refresh();
            }
        }

        private void OnPropertySelected(MenuItemPropertyViewModel obj)
        {
            var mig = SelectedItemPropertyGroups.FirstOrDefault(propertyGroup => propertyGroup.Properties.Contains(obj));
            Debug.Assert(mig != null);
            SelectedItem.ToggleProperty(mig.Model, obj.Model);

            foreach (var model in SelectedItemPropertyGroups)
            {
                model.Refresh(SelectedItem.Properties);
            }
        }

        private void OnPropertyGroupSelected(MenuItemGroupedPropertyItemViewModel obj)
        {
            obj.TicketItemProperty =
              SelectedItem.ToggleProperty(obj.MenuItemPropertyGroup, obj.NextProperty);
            obj.UpdateNextProperty(obj.NextProperty);
        }

        private void OnTicketTagSelected(TicketTagViewModel obj)
        {
            if (DataContext.SelectedTicket != null && SelectedTicketTag != null)
            {
                DataContext.SelectedTicket.UpdateTag(SelectedTicketTag, obj.Model);
                foreach (var model in TicketTags)
                {
                    model.Refresh();
                }
                InvokeTagUpdated(SelectedTicketTag);
            }
        }
    }
}
