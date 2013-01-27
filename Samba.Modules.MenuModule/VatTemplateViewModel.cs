﻿using System;
using Samba.Domain.Models.Menus;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.MenuModule
{
    public class VatTemplateViewModel : EntityViewModelBase<VatTemplate>
    {
        public VatTemplateViewModel(VatTemplate model)
            : base(model)
        {
        }

        public string DisplayName
        {
            get
            {
                return string.Format("{0} - {1}", Name, (VatIncluded ? Resources.Included : Resources.Excluded));
            }
        }

        public decimal Rate { get { return Model.Rate; } set { Model.Rate = value; } }

        public bool VatIncluded { get { return Model.VatIncluded; } set { Model.VatIncluded = value; } }

        public override Type GetViewType()
        {
            return typeof(VatTemplateView);
        }

        public override string GetModelTypeString()
        {
            return Resources.VatTemplate;
        }
    }
}
