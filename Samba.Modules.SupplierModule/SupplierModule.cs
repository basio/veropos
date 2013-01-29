using System.ComponentModel.Composition;
using Microsoft.Practices.Prism.Events;
using Microsoft.Practices.Prism.MefExtensions.Modularity;
using Microsoft.Practices.Prism.Regions;
using Samba.Domain.Models.Suppliers;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Interaction;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SupplierModule
{
    [ModuleExport(typeof(SupplierModule))]
    public class SupplierModule : ModuleBase
    {
        private readonly IRegionManager _regionManager;
        private readonly SupplierSelectorView _SupplierSelectorView;
        private SupplierListViewModel _SupplierListViewModel;
        public ICategoryCommand ListSuppliersCommand { get; set; }

        [ImportingConstructor]
        public SupplierModule(IRegionManager regionManager, SupplierSelectorView SupplierSelectorView)
        {
            _regionManager = regionManager;
            _SupplierSelectorView = SupplierSelectorView;
            ListSuppliersCommand = new CategoryCommand<string>(Resources.SupplierList, Resources.Products, OnSupplierListExecute) { Order = 40 };
            PermissionRegistry.RegisterPermission(PermissionNames.MakeAccountTransaction, PermissionCategories.Cash, Resources.CanMakeAccountTransaction);
            PermissionRegistry.RegisterPermission(PermissionNames.CreditOrDeptAccount, PermissionCategories.Cash, Resources.CanMakeCreditOrDeptTransaction);
        }

        private void OnSupplierListExecute(string obj)
        {
            if (_SupplierListViewModel == null)
                _SupplierListViewModel = new SupplierListViewModel();
            CommonEventPublisher.PublishViewAddedEvent(_SupplierListViewModel);
        }

        protected override void OnInitialization()
        {
            _regionManager.RegisterViewWithRegion(RegionNames.MainRegion, typeof(SupplierSelectorView));

            EventServiceFactory.EventService.GetEvent<GenericEvent<VisibleViewModelBase>>().Subscribe(s =>
            {
                if (s.Topic == EventTopicNames.ViewClosed)
                {
                    if (s.Value == _SupplierListViewModel)
                        _SupplierListViewModel = null;
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Department>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.SelectSupplier)
                {
                    ActivateSupplierView();
                   ((SupplierSelectorViewModel)_SupplierSelectorView.DataContext).RefreshSelectedSupplier();
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<EventAggregator>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateSupplierView)
                {
                    ActivateSupplierView();
                  ((SupplierSelectorViewModel)_SupplierSelectorView.DataContext).RefreshSelectedSupplier();
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<Supplier>>().Subscribe(x =>
            {
                if (x.Topic == EventTopicNames.ActivateSupplierAccount)
                {
                    ActivateSupplierView();
                    ((SupplierSelectorViewModel)_SupplierSelectorView.DataContext).DisplaySupplierAccount(x.Value);
                }
            });

            EventServiceFactory.EventService.GetEvent<GenericEvent<PopupData>>().Subscribe(
                x =>
                {
                    if (x.Topic == EventTopicNames.PopupClicked && x.Value.EventMessage == "SelectSupplier")
                    {
                        ActivateSupplierView();
                        ((SupplierSelectorViewModel)_SupplierSelectorView.DataContext).SearchSupplier(x.Value.DataObject as string);
                    }
                }
                );
        }

        private void ActivateSupplierView()
        {
            AppServices.ActiveAppScreen = AppScreens.SupplierList;
            _regionManager.Regions[RegionNames.MainRegion].Activate(_SupplierSelectorView);
        }

        protected override void OnPostInitialization()
        {
            CommonEventPublisher.PublishDashboardCommandEvent(ListSuppliersCommand);
        }
    }
}
