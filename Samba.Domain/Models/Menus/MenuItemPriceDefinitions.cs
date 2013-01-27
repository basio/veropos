﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Tickets;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Menus
{
    public class MenuItemPriceDefinition : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [StringLength(10)]
        public string PriceTag { get; set; }
    }
}
