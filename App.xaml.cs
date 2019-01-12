using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MyLeftClicker {
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application {
        #region Declaration
        private TaskTrayMenu _taskTrayMenu;
        #endregion

        #region Application
        /// <summary>
        /// application startup event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            _taskTrayMenu = new TaskTrayMenu(AppSettingData.GetInstance().ClickCount);
            _taskTrayMenu.SetObserveChecked(AppSettingData.GetInstance().Observered);
            _taskTrayMenu.OnObserveStateChanged += _taskTrayMenu_OnObserveStateChanged;
            _taskTrayMenu.OnClickCountChanged += _taskTrayMenu_OnClickCountChanged;
            _taskTrayMenu.OnExitClicked += _taskTrayMenu_OnExitClicked;

            KeymappingHandler.SetUp();
            KeymappingHandler.ClickCount = AppSettingData.GetInstance().ClickCount;
            if (AppSettingData.GetInstance().Observered) {
                KeymappingHandler.Start();
            }
        }

        /// <summary>
        /// application exit event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);
            _taskTrayMenu.Dispose();
            KeymappingHandler.Stop();
        }
        #endregion

        #region TaskTrayMenuEvent
        private void _taskTrayMenu_OnObserveStateChanged(object sender, TaskTrayMenu.ObserveStateChangedEventArgs e) {
            AppSettingData.GetInstance().Observered = e.Observerd;
            AppSettingData.GetInstance().Save();

            if (e.Observerd) {
                KeymappingHandler.Start();
            } else {
                KeymappingHandler.Stop();
            }
        }

        private void _taskTrayMenu_OnClickCountChanged(object sender, TaskTrayMenu.ClickCountChangedEventArgs e) {
            AppSettingData.GetInstance().ClickCount = e.ClickCount;
            AppSettingData.GetInstance().Save();
            KeymappingHandler.ClickCount = e.ClickCount;
        }

        private void _taskTrayMenu_OnExitClicked(object sender, EventArgs e) {
            KeymappingHandler.Stop();
            base.Shutdown();
        }
        #endregion
    }
}
