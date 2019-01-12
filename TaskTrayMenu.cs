using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static MyLeftClicker.AppSettingData;

namespace MyLeftClicker {
    public partial class TaskTrayMenu : Component {

        #region Declaration
        public class ObserveStateChangedEventArgs : EventArgs {
            public bool Observerd { set; get; }
            public ObserveStateChangedEventArgs(bool observed) {
                this.Observerd = observed;
            }
        }
        public delegate void ObserveStateChangedHandler(object sender, ObserveStateChangedEventArgs e);
        public event ObserveStateChangedHandler OnObserveStateChanged;

        public class ClickCountChangedEventArgs : EventArgs {
            public int ClickCount { set; get; }
            public ClickCountChangedEventArgs(int ClickCount) {
                this.ClickCount = ClickCount;
            }
        }
        public delegate void ClickCountChangedHandler(object sender, ClickCountChangedEventArgs e);
        public event ClickCountChangedHandler OnClickCountChanged;

        public event EventHandler OnExitClicked;

        ToolStripMenuItem _itemObserve;
        ToolStripMenuItem _itemClickCount;
        int _clickCount;
        #endregion

        #region Constructor
        public TaskTrayMenu() {
            InitializeComponent();
            this.Initialize();
        }
        public TaskTrayMenu(int clickCount) {
            InitializeComponent();
            this._clickCount = clickCount;
            this.Initialize();
        }

        public TaskTrayMenu(IContainer container) {
            container.Add(this);
            InitializeComponent();
            this.Initialize();
        }
        #endregion

        #region Public Method
        /// <summary>
        /// set observe menu checked
        /// </summary>
        /// <param name=""></param>
        public void SetObserveChecked(bool isChecked) {
            _itemObserve.Checked = isChecked;
        }
        #endregion

        #region Event
        private void OnItemObserveCheckedClicked(object sender, EventArgs e) {
            _itemObserve.Checked = !_itemObserve.Checked;
            OnObserveStateChanged?.Invoke(this, new ObserveStateChangedEventArgs(_itemObserve.Checked));
        }

        private void OnItemClickCountClicked(object sender, EventArgs e) {
            var dialog = new ClickCountSetting(this._clickCount);
            if (true == dialog.ShowDialog()) {
                this._clickCount = dialog.ClickCount;
                this.SetItemCountText();
                OnClickCountChanged?.Invoke(this, new ClickCountChangedEventArgs(dialog.ClickCount));
            }
        }

        private void OnItemExitClicked(object sender, EventArgs e) {
            OnExitClicked?.Invoke(this, new EventArgs());
        }
        #endregion

        #region Private Method
        /// <summary>
        /// initialize this component
        /// </summary>
        private void Initialize() {
            this.cMenu.SuspendLayout();
            // create context menu by code, bacuase I can't find the way to create nested menu. 
            this.cMenu.Items.Clear();

            _itemObserve = new ToolStripMenuItem();
            _itemObserve.Text = "observe";
            _itemObserve.ToolTipText = "observe to click";
            _itemObserve.Click += OnItemObserveCheckedClicked;
            this.cMenu.Items.Add(_itemObserve);

            _itemClickCount = new ToolStripMenuItem();
            this.SetItemCountText();
            _itemClickCount.ToolTipText = "set click count";
            _itemClickCount.Click += OnItemClickCountClicked;
            this.cMenu.Items.Add(_itemClickCount);

            ToolStripSeparator separator = new ToolStripSeparator();
            this.cMenu.Items.Add(separator);

            ToolStripMenuItem itemExit = new ToolStripMenuItem();
            itemExit.Text = "Exit";
            itemExit.ToolTipText = "Exit Application";
            itemExit.Click += OnItemExitClicked;
            this.cMenu.Items.Add(itemExit);
            this.cMenu.ResumeLayout();
        }

        private void SetItemCountText() {
            this._itemClickCount.Text = string.Format("{0}/s", _clickCount);
        }
        #endregion
    }
}
