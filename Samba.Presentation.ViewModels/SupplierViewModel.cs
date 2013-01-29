using System;
using System.Collections.Generic;
using System.Linq;
using Samba.Domain.Models.Suppliers;
using Samba.Domain.Models.Tickets;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common;
using Samba.Services;

namespace Samba.Presentation.ViewModels
{
    public class SupplierViewModel : ObservableObject
    {
        public Supplier Model { get; set; }

        public SupplierViewModel(Supplier model)
        {
            Model = model;
        }

        public int Id { get { return Model.Id; } }
        public string Name { get { return Model.Name; } set { Model.Name = value.Trim(); RaisePropertyChanged("Name"); } }
        public string PhoneNumber { get { return Model.PhoneNumber; } set { Model.PhoneNumber = !string.IsNullOrEmpty(value) ? value.Trim() : ""; RaisePropertyChanged("PhoneNumber"); } }
        public string Address { get { return Model.Address; } set { Model.Address = value; RaisePropertyChanged("Address"); } }
        public string Note { get { return Model.Note; } set { Model.Note = value; RaisePropertyChanged("Note"); } }
        public string PhoneNumberText { get { return PhoneNumber != null ? FormatAsPhoneNumber(PhoneNumber) : PhoneNumber; } }
        public DateTime AccountOpeningDate { get { return Model.AccountOpeningDate; } set { Model.AccountOpeningDate = value; } }
        
        public decimal AccountBalance { get; private set; }
        public bool IsNotNew { get { return Model.Id > 0; } }

        private static string FormatAsPhoneNumber(string phoneNumber)
        {
            var phoneNumberInputMask = AppServices.SettingService.PhoneNumberInputMask;
            if (phoneNumber.Length == phoneNumberInputMask.Count(x => x == '#'))
            {
                decimal d;
                decimal.TryParse(phoneNumber, out d);
                return d.ToString(phoneNumberInputMask);
            }
            return phoneNumber;
        }

        public void UpdateDetailedInfo()
        {
            AccountBalance = CashService.GetAccountBalanceForSupplier(Model.Id);
        }
    }
}
