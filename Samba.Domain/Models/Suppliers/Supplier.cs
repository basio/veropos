using System;
using Samba.Infrastructure.Data;

namespace Samba.Domain.Models.Suppliers
{
    public class Supplier : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Note { get; set; }
        public DateTime AccountOpeningDate { get; set; }
      
        private static readonly Supplier _null = new Supplier { Name = "*" };
        public static Supplier Null { get { return _null; } }

        public Supplier()
        {
            AccountOpeningDate = DateTime.Now;
        }
    }
}
