﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SkoLifeWinSrv.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.8.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.ConnectionString)]
        [global::System.Configuration.DefaultSettingValueAttribute("Data Source=.;Initial Catalog=skolife;Persist Security Info=True;User ID=sa;Passw" +
            "ord=2491983")]
        public string SkoLifeDBConnection {
            get {
                return ((string)(this["SkoLifeDBConnection"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://Win10OpenSource/SkoLifeAsyncSrv/api/SkolifeAsync/getconfig.php")]
        public string GetConfigSrvURL {
            get {
                return ((string)(this["GetConfigSrvURL"]));
            }
            set {
                this["GetConfigSrvURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://Win10OpenSource/SkoLifeAsyncSrv/api/SkolifeAsync/postdata.php")]
        public string PostDataSrvURL {
            get {
                return ((string)(this["PostDataSrvURL"]));
            }
            set {
                this["PostDataSrvURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("00:01:00")]
        public global::System.TimeSpan ExecuteInterval {
            get {
                return ((global::System.TimeSpan)(this["ExecuteInterval"]));
            }
            set {
                this["ExecuteInterval"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://Win10OpenSource/SkoLifeAsyncSrv/api/SkolifeAsync/requestdata.php")]
        public string RequestDataSrvURL {
            get {
                return ((string)(this["RequestDataSrvURL"]));
            }
            set {
                this["RequestDataSrvURL"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://Win10OpenSource/SkoLifeAsyncSrv/api/SkolifeAsync/postdyn.php")]
        public string PostDynSrvURL {
            get {
                return ((string)(this["PostDynSrvURL"]));
            }
            set {
                this["PostDynSrvURL"] = value;
            }
        }
    }
}
