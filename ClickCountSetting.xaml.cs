using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace MyLeftClicker {
    /// <summary>
    /// ClickCountSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class ClickCountSetting : Window {

        #region Declaration
        public int ClickCount { set; get; }
        #endregion

        #region Constructor
        public ClickCountSetting() {
            InitializeComponent();
            this.ClickCount = AppSettingData.ClickCountUnit;
            this.cClickCount.Text = AppSettingData.ClickCountUnit.ToString();
        }
        public ClickCountSetting(int count) {
            InitializeComponent();
            this.ClickCount = count;
            this.cClickCount.Text = count.ToString();
        }
        #endregion

        #region Control Event
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cClickCount_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !new Regex("[0-9]").IsMatch(e.Text);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cClickCount_PreviewExecuted(object sender, ExecutedRoutedEventArgs e) {
            if (e.Command == ApplicationCommands.Paste) {
                e.Handled = true;
            }
        }
        private void OK_Click(object sender, RoutedEventArgs e) {
            if (this.cClickCount.Text.Length == 0) {
                this.DialogResult = false;
            } else {
                int count;
                if (int.TryParse(this.cClickCount.Text,out count)) {
                    this.ClickCount = count;
                    this.DialogResult = true;
                } else {
                    this.DialogResult = false;
                }
            }
            this.Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e) {
            this.DialogResult = false;
            this.Close();
        }
        #endregion

    }
}
