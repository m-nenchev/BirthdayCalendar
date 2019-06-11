using System;
using System.Linq;
using System.Windows.Forms;
using Bc.Engine;

namespace BirthdayCalendar
{
    public partial class Form1 : Form
    {
        private IBcManager _bcManager;

        private IBcItem _current;
        private int _currentIndex;
        private bool _editMode;

        public Form1()
        {
            InitializeComponent();
            // Do not allow to pick up a date for the birthday in the future
            var now = DateTime.Now;
            dtpDateOfBirth.MaxDate = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Get an instance of the BcManager via the Factory
            _bcManager = BcEngineFactory.BcManager;
            // Load all BcItems from the file into memory
            _bcManager.ReadAllBcItems();
            // Get the first item in the list.
            _current = _bcManager.AllItems.FirstOrDefault();
            _currentIndex = _current != null ? 0 : -1;
            SetCurrentToUi();
            SetEditMode();
            LoadTodaysBirthdays();
        }

        private void SetCurrentToUi()
        {
            if (_currentIndex < 0 || _currentIndex >= _bcManager.AllItems.Count)
            {
                if (_currentIndex < 0)
                {
                    _currentIndex = -1;
                }

                if (_currentIndex >= _bcManager.AllItems.Count)
                {
                    _currentIndex = _bcManager.AllItems.Count - 1;
                }

                _current = null;
                tbFirstName.Text = string.Empty;
                tbLastName.Text = string.Empty;
                dtpDateOfBirth.Value = DateTime.Now;
                dtpDateOfBirth.Text = string.Empty;
                tbAge.Text = string.Empty;
                tbHadBday.Text = string.Empty;
            }
            else
            {
                _current = _bcManager.AllItems[_currentIndex];
                tbFirstName.Text = _current.FirstName;
                tbLastName.Text = _current.LastName;
                dtpDateOfBirth.Value = _current.DateOfBirth;
                tbAge.Text = _current.Age.ToString();
                tbHadBday.Text = _current.HadBirthdayThisYear ? "Yes" : "No";
            }

            if (!_bcManager.AllItems.Any())
            {
                btnFirst.Enabled = false;
                btnPrev.Enabled = false;
                btnNext.Enabled = false;
                btnLast.Enabled = false;
            }
            else if (_currentIndex <= 0)
            {
                btnFirst.Enabled = false;
                btnPrev.Enabled = false;
                btnNext.Enabled = true;
                btnLast.Enabled = true;
            }
            else if (_currentIndex >= _bcManager.AllItems.Count - 1)
            {
                btnFirst.Enabled = true;
                btnPrev.Enabled = true;
                btnNext.Enabled = false;
                btnLast.Enabled = false;
            }
            else
            {
                btnFirst.Enabled = true;
                btnPrev.Enabled = true;
                btnNext.Enabled = true;
                btnLast.Enabled = true;
            }

            btnDelete.Visible = _current != null;
        }

        private void SetEditMode()
        {
            btnNew.Visible = !_editMode;
            btnDelete.Visible = !_editMode && _current != null;
            btnAdd.Visible = _editMode;
            btnCancel.Visible = _editMode;
            tbFirstName.ReadOnly = !_editMode;
            tbLastName.ReadOnly = !_editMode;
            dtpDateOfBirth.Enabled = _editMode;

            if (_editMode)
            {
                tbFirstName.Text = string.Empty;
                tbLastName.Text = string.Empty;
                dtpDateOfBirth.Value = DateTime.Now;
                tbAge.Text = string.Empty;
                tbHadBday.Text = string.Empty;
            }
        }

        private void BtnFirstClick(object sender, EventArgs e)
        {
            // If the list is not empty then the current index is 0 (the first element)
            // If the list is empty then the current index is -1 (invalid)
            _currentIndex = _bcManager.AllItems.Count > 0 ? 0 : -1;
            SetCurrentToUi();
        }

        private void BtnLastClick(object sender, EventArgs e)
        {
            // If the list is not empty then the current index is the last item
            // If the list is empty then the current index is -1 (invalid)
            _currentIndex = _bcManager.AllItems.Count - 1;
            SetCurrentToUi();
        }

        private void BtnPrevClick(object sender, EventArgs e)
        {
            _currentIndex--;
            SetCurrentToUi();
        }

        private void BtnNextClick(object sender, EventArgs e)
        {
            _currentIndex++;
            SetCurrentToUi();
        }

        private void BtnNewClick(object sender, EventArgs e)
        {
            _editMode = true;
            SetEditMode();
        }

        private void BtnDeleteClick(object sender, EventArgs e)
        {
            if (MessageBox.Show("Do you want to delete: " + Environment.NewLine + _current.ToString(), "Delete...", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _bcManager.RemoveItem(_current);
                if (_currentIndex >= _bcManager.AllItems.Count)
                {
                    _currentIndex--;
                }

                SetCurrentToUi();
                LoadTodaysBirthdays();
            }
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            _editMode = false;
            SetEditMode();
            SetCurrentToUi();
        }

        private void BtnAddClick(object sender, EventArgs e)
        {
            var newItem = BcEngineFactory.CreateBcItem();
            newItem.FirstName = tbFirstName.Text.Trim();
            newItem.LastName = tbLastName.Text.Trim();
            newItem.DateOfBirth = dtpDateOfBirth.Value;
            if (_bcManager.AddBcItem(newItem))
            {
                _currentIndex = _bcManager.AllItems.Count - 1;
            }

            _editMode = false;
            SetEditMode();
            SetCurrentToUi();
            LoadTodaysBirthdays();
        }

        private void LoadTodaysBirthdays()
        {
            lbBirthdays.Items.Clear();
            var todaysBirthdays = _bcManager.AllItems.Where(item => item.DateOfBirth.Month == DateTime.Now.Month && item.DateOfBirth.Day == DateTime.Now.Day);
            foreach (var todaysBirthday in todaysBirthdays)
            {
                var textToAdd = string.Format("({0}) {1}", todaysBirthday.Age, todaysBirthday.ToString());
                lbBirthdays.Items.Add(textToAdd);
            }
        }

        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Save All will overwrite the source file with the current content.", "Save All...", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
            {
                _bcManager.WriteAllBcItems();
                _bcManager.ReadAllBcItems();
            }
        }
    }
}
