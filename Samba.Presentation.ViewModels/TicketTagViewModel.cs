﻿using System;
using System.Collections.Generic;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Tickets;
using Samba.Persistance.Data;
using Samba.Presentation.Common;

namespace Samba.Presentation.ViewModels
{
    public class TicketTagViewModel : ObservableObject
    {
        public TicketTagViewModel(TicketTag model)
        {
            Model = model;
        }

        public TicketTag Model { get; set; }

        public string Name
        {
            get { return Model.Name; }
            set { Model.Name = value; }
        }

        public int AccountId
        {
            get { return Model.AccountId; }
            set { Model.AccountId = value; }
        }

        public string AccountName
        {
            get { return Model.AccountName; }
            set
            {
                Model.AccountName = value;
                AccountId = value != null ? Dao.SingleWithCache<Customer>(x => x.Name == Model.AccountName).Id : 0;
            }
        }

        private IEnumerable<string> _accountNames;
        public IEnumerable<string> AccountNames
        {
            get
            {
                return _accountNames ??
                    (_accountNames = Dao.Select<Customer, string>(x => x.Name, x => x.InternalAccount));
            }
        }

        public string Display { get { return Model.Display; } }

        public void Refresh()
        {
            RaisePropertyChanged("Name");
        }
    }
}
