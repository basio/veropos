﻿using System;
using System.Collections.Generic;
using Samba.Domain.Models.Suppliers;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;

namespace Samba.Modules.SupplierModule
{
    public class SupplierEditorViewModel : EntityViewModelBase<Supplier>
    {
        public SupplierEditorViewModel(Supplier model)
            : base(model)
        {
        }

        public override Type GetViewType()
        {
            return typeof(SupplierEditorView);
        }

        public override string GetModelTypeString()
        {
            return Resources.Supplier;
        }

        private IEnumerable<string> _groupCodes;
      //  public IEnumerable<string> GroupCodes { get { return _groupCodes ?? (_groupCodes = Dao.Distinct<Supplier>(x => x.GroupCode)); } }

     //   public string GroupValue { get { return Model.GroupCode; } }
     //   public string GroupCode { get { return Model.GroupCode ?? ""; } set { Model.GroupCode = value; } }
        public string PhoneNumber { get { return Model.PhoneNumber; } set { Model.PhoneNumber = value; } }
        public string Address { get { return Model.Address; } set { Model.Address = value; } }
        public string Note { get { return Model.Note; } set { Model.Note = value; } }
      //  public bool InternalAccount { get { return Model.InternalAccount; } set { Model.InternalAccount = value; } }
        public string PhoneNumberInputMask { get { return AppServices.SettingService.PhoneNumberInputMask; } }
    }
}
