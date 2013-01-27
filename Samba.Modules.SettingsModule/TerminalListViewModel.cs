﻿using Samba.Domain.Models.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;

namespace Samba.Modules.SettingsModule
{
    public class TerminalListViewModel : EntityCollectionViewModelBase<TerminalViewModel, Terminal>
    {
        protected override TerminalViewModel CreateNewViewModel(Terminal model)
        {
            return new TerminalViewModel(model);
        }

        protected override Terminal CreateNewModel()
        {
            return new Terminal();
        }

        protected override string CanDeleteItem(Terminal model)
        {
            var count = Workspace.Count<Terminal>();
            if (count == 1) return Resources.DeleteErrorShouldHaveAtLeastOneTerminal;
            return base.CanDeleteItem(model);
        }
    }
}
