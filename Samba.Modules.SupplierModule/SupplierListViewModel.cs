using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Suppliers;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SupplierModule
{
    class SupplierListViewModel : EntityCollectionViewModelBase<SupplierEditorViewModel, Supplier>
    {
        protected override SupplierEditorViewModel CreateNewViewModel(Supplier model)
        {
            return new SupplierEditorViewModel(model);
        }

        protected override Supplier CreateNewModel()
        {
            return new Supplier();
        }
    }
}
