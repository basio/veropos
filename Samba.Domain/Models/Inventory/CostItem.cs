﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Menus;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Inventory
{
    public class CostItem : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int PeriodicConsumptionId { get; set; }
        public virtual MenuItemPortion Portion { get; set; }
        public decimal Quantity { get; set; }
        public decimal CostPrediction { get; set; }
        public decimal Cost { get; set; }
    }
}
