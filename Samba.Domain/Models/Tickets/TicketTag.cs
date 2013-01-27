﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Customers;
using Samba.Domain.Models.Settings;
using Samba.Infrastructure;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Tickets
{
    public class TicketTag : IEntity, IStringCompareable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int TicketTagGroupId { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public string Display { get { return !string.IsNullOrEmpty(Name) ? Name : "X"; } }

        private static TicketTag _emptyTicketTag;
        public static TicketTag Empty
        {
            get { return _emptyTicketTag ?? (_emptyTicketTag = new TicketTag()); }
        }

        public string GetStringValue()
        {
            return Name;
        }
    }
}
