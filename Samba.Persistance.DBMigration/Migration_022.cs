using FluentMigrator;
using Samba.Infrastructure.Settings;

namespace Samba.Persistance.DBMigration
{
    [Migration(22)]
    public class Migration_022 : Migration
    {
        public override void Up()
        {
       //  Delete.Column("SupplierId").FromTable("Transactions");
            Create.Column("Supplier_Id").OnTable("Transactions").AsInt32().Nullable();
            
        }

        public override void Down()
        {
            //do nothing
        }
    }
}