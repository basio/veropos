﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Samba.Domain.Models.Settings;
using Samba.Presentation.Common.ModelBase;
using Samba.Presentation.Common.Services;
using Samba.Services;

namespace Samba.Modules.SettingsModule
{
    class TriggerListViewModel : EntityCollectionViewModelBase<TriggerViewModel, Trigger>
    {
        protected override TriggerViewModel CreateNewViewModel(Trigger model)
        {
            return new TriggerViewModel(model);
        }

        protected override Trigger CreateNewModel()
        {
            return new Trigger();
        }

        protected override void OnDeleteItem(object obj)
        {
            base.OnDeleteItem(obj);
            MethodQueue.Queue("UpdateCronObjects", TriggerService.UpdateCronObjects);
        }
    }
}
