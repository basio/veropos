using FluentMigrator;
using Samba.Infrastructure.Settings;

namespace Samba.Persistance.DBMigration
{
    [Migration(21)]
    public class Migration_021 : Migration
    {
        public override void Up()
        {
            Create.Column("SupplierID").OnTable("CashTransactions").AsInt32().WithDefaultValue(-1);
            Create.Column("SupplierID").OnTable("AccountTransactions").AsInt32().WithDefaultValue(-1);
            Create.Table("Suppliers")
              .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                .WithColumn("Name").AsString(128).Nullable()
                .WithColumn("PhoneNumber").AsString(128).Nullable()
                .WithColumn("Note").AsString(128).Nullable()
                 .WithColumn("Address").AsString(128).Nullable()
                 .WithColumn("AccountOpeningDate").AsDateTime().Nullable();
           // Create.Column("SupplierId").OnTable("Transcations").AsInt32().Nullable();

        }

        public override void Down()
        {
            //do nothing
        }
    }
}