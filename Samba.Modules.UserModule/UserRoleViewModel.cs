﻿using System;
using System.Collections.Generic;
using Samba.Domain.Models.Tickets;
using Samba.Domain.Models.Users;
using Samba.Infrastructure.Data;
using Samba.Localization.Properties;
using Samba.Presentation.Common.ModelBase;
using Samba.Services;
using System.Linq;

namespace Samba.Modules.UserModule
{
    public class UserRoleViewModel : EntityViewModelBase<UserRole>
    {
        private IWorkspace _workspace;

        public UserRoleViewModel(UserRole role)
            : base(role)
        { }

        private IEnumerable<PermissionViewModel> _permissions;
        public IEnumerable<PermissionViewModel> Permissions
        {
            get { return _permissions ?? (_permissions = GetPermissions()); }
        }

        private IEnumerable<PermissionViewModel> GetPermissions()
        {
            if (Model.Permissions.Count() < PermissionRegistry.PermissionNames.Count)
            {
                foreach (var pName in PermissionRegistry.PermissionNames.Keys.Where(pName => Model.Permissions.SingleOrDefault(x => x.Name == pName) == null))
                {
                    Model.Permissions.Add(new Permission { Name = pName, Value = 0 });
                }
            }
            return Model.Permissions.Where(x => PermissionRegistry.PermissionNames.ContainsKey(x.Name)).Select(x => new PermissionViewModel(x));
        }

        private IEnumerable<Department> _departments;
        public IEnumerable<Department> Departments
        {
            get { return _departments ?? (_departments = _workspace.All<Department>()); }
        }

        public int DepartmentId
        {
            get { return Model.DepartmentId; }
            set { Model.DepartmentId = value; }
        }

        public bool IsAdmin
        {
            get { return Model.IsAdmin; }
            set { Model.IsAdmin = value; }
        }

        public override Type GetViewType()
        {
            return typeof(UserRoleView);
        }

        public override string GetModelTypeString()
        {
            return Resources.UserRole;
        }

        protected override void Initialize(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        protected override string GetSaveErrorMessage()
        {
            if (DepartmentId == 0)
                return Resources.SaveErrorSelectDefaultDepartment;
            return base.GetSaveErrorMessage();
        }
    }
}
