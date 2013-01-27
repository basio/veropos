﻿using System;
using Samba.Domain.Models.Settings;

namespace Samba.Domain
{
    public class SettingGetter
    {
        private readonly ProgramSetting _programSetting;

        public SettingGetter(ProgramSetting programSetting)
        {
            _programSetting = programSetting;
        }

        public string SettingName { get { return _programSetting.Name; } }

        public string StringValue { get { return _programSetting.Value; } set { _programSetting.Value = value; } }

        public DateTime DateTimeValue
        {
            get
            {
                DateTime result;
                DateTime.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        public int IntegerValue
        {
            get
            {
                int result;
                int.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        public decimal DecimalValue
        {
            get
            {
                decimal result;
                decimal.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        public bool BoolValue
        {
            get
            {
                bool result;
                bool.TryParse(_programSetting.Value, out result);
                return result;
            }
            set { _programSetting.Value = value.ToString(); }
        }

        private static SettingGetter _nullSetting;
        public static SettingGetter NullSetting
        {
            get { return _nullSetting ?? (_nullSetting = new SettingGetter(new ProgramSetting())); }
        }
    }
}
