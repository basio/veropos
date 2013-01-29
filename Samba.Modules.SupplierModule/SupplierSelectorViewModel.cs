using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using Samba.Domain;
using Samba.Domain.Models.Suppliers;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Transactions;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Presentation.ViewModels;
using Samba.Services;

namespace Samba.Modules.SupplierModule
{
    [Export]
    public class  SupplierSelectorViewModel : ObservableObject
    {
        private readonly Timer _updateTimer;

        public ICaptionCommand CloseScreenCommand { get; set; }
        public ICaptionCommand SelectSupplierCommand { get; set; }
        public ICaptionCommand CreateSupplierCommand { get; set; }
     //   public ICaptionCommand ResetSupplierCommand { get; set; }
      //  public ICaptionCommand FindTicketCommand { get; set; }
        //public ICaptionCommand MakePaymentCommand { get; set; }
        public ICaptionCommand DisplaySupplierAccountCommand { get; set; }
        public ICaptionCommand GetPaymentFromSupplierCommand { get; set; }
        public ICaptionCommand MakePaymentToSupplierCommand { get; set; }
        public ICaptionCommand AddReceivableCommand { get; set; }
        public ICaptionCommand AddLiabilityCommand { get; set; }
        public ICaptionCommand CloseAccountScreenCommand { get; set; }
        public string PhoneNumberInputMask { get { return AppServices.SettingService.PhoneNumberInputMask; } }

        //public Ticket SelectedTicket { get { return AppServices.MainDataContext.SelectedTicket; } }
        public ObservableCollection<SupplierViewModel> FoundSuppliers { get; set; }

        public ObservableCollection<SupplierTransactionViewModel> SelectedSupplierTransactions { get; set; }

        private int _selectedSView;
        public int SelectedSView
        {
            get { return _selectedSView; }
            set { _selectedSView = value; RaisePropertyChanged("SelectedSView"); }
        }

        public SupplierViewModel SelectedSupplier { get { return FoundSuppliers.Count == 1 ? FoundSuppliers[0] : FocusedSupplier; } }

        private SupplierViewModel _focusedSupplier;
        public SupplierViewModel FocusedSupplier
        {
            get { return _focusedSupplier; }
            set
            {
                _focusedSupplier = value;
                RaisePropertyChanged("FocusedSupplier");
                RaisePropertyChanged("SelectedSupplier");
            }
        }

   
        private string _sphoneNumberSearchText;
        public string SPhoneNumberSearchText
        {
            get { return string.IsNullOrEmpty(_sphoneNumberSearchText) ? null : _sphoneNumberSearchText.TrimStart('+', '0'); }
            set
            {
                if (value != _sphoneNumberSearchText)
                {
                    _sphoneNumberSearchText = value;
                    RaisePropertyChanged("SPhoneNumberSearchText");
                    ResetTimer();
                }
            }
        }

        private string _SupplierNameSearchText;
        public string SupplierNameSearchText
        {
            get { return _SupplierNameSearchText; }
            set
            {
                if (value != _SupplierNameSearchText)
                {
                    _SupplierNameSearchText = value;
                    RaisePropertyChanged("SupplierNameSearchText");
                    ResetTimer();
                }
            }
        }

        private string _saddressSearchText;
        public string SAddressSearchText
        {
            get { return _saddressSearchText; }
            set
            {
                if (value != _saddressSearchText)
                {
                    _saddressSearchText = value;
                    RaisePropertyChanged("SAddressSearchText");
                    ResetTimer();
                }
            }
        }

       /* public bool IsResetSupplierVisible
        {
            get
            {
                return (AppServices.MainDataContext.SelectedTicket != null &&
                        AppServices.MainDataContext.SelectedTicket.SupplierId > 0);
            }
        }
        */
        /*public bool IsClearVisible
        {
            get
            {
                return (AppServices.MainDataContext.SelectedTicket != null &&
                        AppServices.MainDataContext.SelectedTicket.SupplierId == 0);
            }
        }
        */
        public bool IsMakePaymentVisible
        {
            get
            {
                return (AppServices.IsUserPermittedFor(PermissionNames.MakePayment));
            }
        }

        private int _activeView;
        public int ActiveView
        {
            get { return _activeView; }
            set { _activeView = value; RaisePropertyChanged("ActiveView"); }
        }

        public string TotalReceivable { get { return SelectedSupplierTransactions.Sum(x => x.Receivable).ToString("#,#0.00"); } }
        public string TotalLiability { get { return SelectedSupplierTransactions.Sum(x => x.Liability).ToString("#,#0.00"); } }
        public string TotalBalance { get { return SelectedSupplierTransactions.Sum(x => x.Receivable - x.Liability).ToString("#,#0.00"); } }

        public SupplierSelectorViewModel()
        {
            UpdateFoundSuppliers();
            _updateTimer = new Timer(500);
            _updateTimer.Elapsed += UpdateTimerElapsed;
            FoundSuppliers = new ObservableCollection<SupplierViewModel>();
            CloseScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseScreen);
          //  SelectSupplierCommand = new CaptionCommand<string>(Resources.SelectSupplier_r, OnSelectSupplier, CanSelectSupplier);
            CreateSupplierCommand = new CaptionCommand<string>(Resources.NewSupplier_r, OnCreateSupplier, CanCreateSupplier);
           // FindTicketCommand = new CaptionCommand<string>(Resources.FindTicket_r, OnFindTicket, CanFindTicket);
         //   ResetSupplierCommand = new CaptionCommand<string>(Resources.ResetSupplier_r, OnResetSupplier, CanResetSupplier);
           // MakePaymentCommand = new CaptionCommand<string>(Resources.GetPayment_r, OnMakePayment, CanMakePayment);
           DisplaySupplierAccountCommand = new CaptionCommand<string>(Resources.SupplierAccount_r, OnDisplaySupplierAccount, CanSelectSupplier);
            MakePaymentToSupplierCommand = new CaptionCommand<string>(Resources.MakePayment_r, OnMakePaymentToSupplierCommand, CanMakePaymentToSupplier);
            GetPaymentFromSupplierCommand = new CaptionCommand<string>(Resources.GetPayment_r, OnGetPaymentFromSupplierCommand, CanMakePaymentToSupplier);
            AddLiabilityCommand = new CaptionCommand<string>(Resources.AddLiability_r, OnAddLiability, CanAddLiability);
            AddReceivableCommand = new CaptionCommand<string>(Resources.AddReceivable_r, OnAddReceivable, CanAddLiability);
            CloseAccountScreenCommand = new CaptionCommand<string>(Resources.Close, OnCloseAccountScreen);

            SelectedSupplierTransactions = new ObservableCollection<SupplierTransactionViewModel>();
        }
        private bool CanSelectSupplier(string arg)
        {
            return true;
        }

        private bool CanAddLiability(string arg)
        {
            return CanSelectSupplier(arg) && AppServices.IsUserPermittedFor(PermissionNames.CreditOrDeptAccount);
        }

        private bool CanMakePaymentToSupplier(string arg)
        {
            return CanSelectSupplier(arg) && AppServices.IsUserPermittedFor(PermissionNames.MakeAccountTransaction);
        }

        private void OnAddReceivable(string obj)
        {
            SelectedSupplier.Model.PublishEvent(EventTopicNames.AddReceivableAmountForSupplier);
            FoundSuppliers.Clear();
        }

        private void OnAddLiability(string obj)
        {
            SelectedSupplier.Model.PublishEvent(EventTopicNames.AddLiabilityAmountForSupplier);
            FoundSuppliers.Clear();
        }

        private void OnCloseAccountScreen(string obj)
        {
           RefreshSelectedSupplier();
        }
        public void RefreshSelectedSupplier()
        {

            ClearSearchValues();

             if (SelectedSupplier != null)
                {
                    SelectedSView = 1;
                    SelectedSupplier.UpdateDetailedInfo();
                    FoundSuppliers.Add(SelectedSupplier);
                }
            
            RaisePropertyChanged("SelectedSupplier");
            RaisePropertyChanged("IsClearVisible");
            RaisePropertyChanged("IsResetSupplierVisible");
            RaisePropertyChanged("IsMakePaymentVisible");
            ActiveView = 0;
            SelectedSupplierTransactions.Clear();
        }

        private void OnGetPaymentFromSupplierCommand(string obj)
        {
            SelectedSupplier.Model.PublishEvent(EventTopicNames.GetPaymentFromSupplier);
            FoundSuppliers.Clear();
        }

        private void OnMakePaymentToSupplierCommand(string obj)
        {
            SelectedSupplier.Model.PublishEvent(EventTopicNames.MakePaymentToSupplier);
            FoundSuppliers.Clear();
        }

        internal void DisplaySupplierAccount(Supplier Supplier)
        {
            FoundSuppliers.Clear();
            if (Supplier != null)
                FoundSuppliers.Add(new SupplierViewModel(Supplier));
            RaisePropertyChanged("SelectedSupplier");
            OnDisplaySupplierAccount("");
        }

        private void OnDisplaySupplierAccount(string obj)
        {
            SaveSelectedSupplier();
            SelectedSupplierTransactions.Clear();
            if (SelectedSupplier != null)
            {
               
                var cashTransactions = Dao.Query<CashTransaction>(x => x.Date > SelectedSupplier.AccountOpeningDate && x.SupplierId == SelectedSupplier.Id);
                var accountTransactions = Dao.Query<AccountTransaction>(x => x.Date > SelectedSupplier.AccountOpeningDate && x.SupplierId == SelectedSupplier.Id);

                var transactions = new List<SupplierTransactionViewModel>();
                
                transactions.AddRange(cashTransactions.Where(x => x.TransactionType == (int)TransactionType.Income)
                    .Select(x => new SupplierTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Liability = x.Amount
                    }));

                transactions.AddRange(cashTransactions.Where(x => x.TransactionType == (int)TransactionType.Expense)
                    .Select(x => new SupplierTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Receivable = x.Amount
                    }));

                transactions.AddRange(accountTransactions.Where(x => x.TransactionType == (int)TransactionType.Liability)
                    .Select(x => new SupplierTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Liability = x.Amount
                    }));

                transactions.AddRange(accountTransactions.Where(x => x.TransactionType == (int)TransactionType.Receivable)
                    .Select(x => new SupplierTransactionViewModel
                    {
                        Description = x.Name,
                        Date = x.Date,
                        Receivable = x.Amount
                    }));

                transactions = transactions.OrderBy(x => x.Date).ToList();

                for (var i = 0; i < transactions.Count; i++)
                {
                    transactions[i].Balance = (transactions[i].Receivable - transactions[i].Liability);
                    if (i > 0) (transactions[i].Balance) += (transactions[i - 1].Balance);
                }

                SelectedSupplierTransactions.AddRange(transactions);
                RaisePropertyChanged("TotalReceivable");
                RaisePropertyChanged("TotalLiability");
                RaisePropertyChanged("TotalBalance");
            }
            ActiveView = 1;
        }

        private bool CanMakePayment(string arg)
        {
            return SelectedSupplier != null ;
        }

        private void OnMakePayment(string obj)
        {
            SelectedSupplier.Model.PublishEvent(EventTopicNames.PaymentRequestedForTicket);
            ClearSearchValues();
        }

       

       
      
        private bool CanCreateSupplier(string arg)
        {
            return SelectedSupplier == null;
        }

        private void OnCreateSupplier(string obj)
        {
            FoundSuppliers.Clear();
            var c = new Supplier
                        {
                            Address = SAddressSearchText,
                            Name = SupplierNameSearchText,
                            PhoneNumber = SPhoneNumberSearchText
                        };
            FoundSuppliers.Add(new SupplierViewModel(c));
            SelectedSView = 1;
            RaisePropertyChanged("SelectedSupplier");
        }

        
        private void SaveSelectedSupplier()
        {
            if (!SelectedSupplier.IsNotNew)
            {
                var ws = WorkspaceFactory.Create();
                ws.Add(SelectedSupplier.Model);
                ws.CommitChanges();
            }
        }

        private void OnSelectSupplier(string obj)
        {
            SaveSelectedSupplier();
         //   SelectedSupplier.Model.PublishEvent(EventTopicNames.SupplierSelectedForTicket);
            ClearSearchValues();
        }

        void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _updateTimer.Stop();
            UpdateFoundSuppliers();
        }

        private void ResetTimer()
        {
            _updateTimer.Stop();

            if (!string.IsNullOrEmpty(SPhoneNumberSearchText)
                || !string.IsNullOrEmpty(SupplierNameSearchText)
                || !string.IsNullOrEmpty(SAddressSearchText))
            {
                _updateTimer.Start();
            }
            else FoundSuppliers.Clear();
        }

        private void UpdateFoundSuppliers()
        {

            IEnumerable<Supplier> result = new List<Supplier>();

            using (var worker = new BackgroundWorker())
            {
                worker.DoWork += delegate
                                     {
                                         bool searchPn = string.IsNullOrEmpty(SPhoneNumberSearchText);
                                         bool searchCn = string.IsNullOrEmpty(SupplierNameSearchText);
                                         bool searchAd = string.IsNullOrEmpty(SAddressSearchText);

                                         if (searchAd == true && searchCn == true && searchPn == true)
                                         {
                                             result = Dao.Query<Supplier>();
                                         }
                                         else
                                         result = Dao.Query<Supplier>(
                                             x =>
                                                (searchPn || x.PhoneNumber.Contains(SPhoneNumberSearchText)) &&
                                                (searchCn || x.Name.ToLower().Contains(SupplierNameSearchText.ToLower())) &&
                                                (searchAd || x.Address.ToLower().Contains(SAddressSearchText.ToLower()))
                                                
                                                );
                                     };

                worker.RunWorkerCompleted +=
                    delegate
                    {

                        AppServices.MainDispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(
                               delegate
                               {
                                   FoundSuppliers.Clear();
                                   FoundSuppliers.AddRange(result.Select(x => new SupplierViewModel(x)));

                                   if (SelectedSupplier != null && SPhoneNumberSearchText == SelectedSupplier.PhoneNumber)
                                   {
                                       SelectedSView = 1;
                                       SelectedSupplier.UpdateDetailedInfo();
                                   }

                                   RaisePropertyChanged("SelectedSupplier");

                                   CommandManager.InvalidateRequerySuggested();
                               }));

                    };

                worker.RunWorkerAsync();
            }
        }

        private void OnCloseScreen(string obj)
        {
            EventServiceFactory.EventService.PublishEvent(EventTopicNames.ActivateNavigation);
            SelectedSView = 0;
            ActiveView = 0;
            SelectedSupplierTransactions.Clear();
        }

      
        private void ClearSearchValues()
        {
            FoundSuppliers.Clear();
            SelectedSView = 0;
            ActiveView = 0;
            SPhoneNumberSearchText = "";
            SAddressSearchText = "";
            SupplierNameSearchText = "";
            UpdateFoundSuppliers();
        }

        public void SearchSupplier(string phoneNumber)
        {
            ClearSearchValues();
            SPhoneNumberSearchText = phoneNumber;
            UpdateFoundSuppliers();
        }
    }
}
