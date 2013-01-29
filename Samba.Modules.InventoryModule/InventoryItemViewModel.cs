using System;
using System.Collections.Generic;
using Samba.Domain.Models.Inventory;
using Samba.Localization.Properties;
using Samba.Persistance.Data;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.InventoryModule
{
    public class InventoryItemViewModel : EntityViewModelBase<InventoryItem>
    {
        public InventoryItemViewModel(InventoryItem model)
            : base(model)
        {
        }

        public override Type GetViewType()
        {
            return typeof(InventoryItemView);
        }

        public override string GetModelTypeString()
        {
            return Resources.InventoryItem;
        }

        
        public string BaseUnit
        {
            get { return Model.BaseUnit; }
            set
            {
                Model.BaseUnit = value; RaisePropertyChanged("BaseUnit");
                RaisePropertyChanged("PredictionUnit");
            }
        }

        public string TransactionUnit
        {
            get { return Model.TransactionUnit; }
            set
            {
                Model.TransactionUnit = value;
                RaisePropertyChanged("TransactionUnit");
                RaisePropertyChanged("PredictionUnit");
            }
        }

        public int TransactionUnitMultiplier
        {
            get { return Model.TransactionUnitMultiplier; }
            set
            {
                Model.TransactionUnitMultiplier = value;
                RaisePropertyChanged("TransactionUnitMultiplier");
                RaisePropertyChanged("PredictionUnit");
            }
        }

        public string PredictionUnit
        {
            get
            {
                return TransactionUnitMultiplier > 0 ? TransactionUnit : BaseUnit;
            }
        }

        //public string GroupValue { get { return Model.GroupCode; } }

        protected override string GetSaveErrorMessage()
        {
            if (Dao.Single<InventoryItem>(x => x.Name.ToLower() == Model.Name.ToLower() && x.Id != Model.Id) != null)
                return string.Format(Resources.SaveErrorDuplicateItemName_f, Resources.InventoryItem);
            return base.GetSaveErrorMessage();
        }
    }
}
