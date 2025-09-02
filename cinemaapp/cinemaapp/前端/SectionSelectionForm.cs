using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using test.Models;
using test.Services;

namespace cinemaapp
{
    public partial class SectionSelectionForm : Form
    {
        private readonly Film _selectedFilm;
        private readonly DateTime _selectedDate;
        private List<Section> _availableSections;
        private IShowingService _showingService;
        private readonly Customer _loggedInCustomer;
        private MainForm _mainForm;

        public SectionSelectionForm(Film film, DateTime date, Customer loggedInCustomer,MainForm mainForm)
        {
            InitializeComponent();
            _selectedFilm = film;
            _selectedDate = date;
            _showingService = Program._showingService;
            _loggedInCustomer = loggedInCustomer;
            _mainForm = mainForm;

            // 设置窗体属性
            this.Text = $"{_selectedFilm.FilmName} - {_selectedDate:yyyy-MM-dd} 场次";
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(600, 800);

            // 初始化ListView
            InitializeListView();

            // 改到这里
            this.Shown += (s, e) => LoadSections();
        }

        private void InitializeListView()
        {
            // 设置ListView基本属性
            listViewSections.View = View.Details;
            listViewSections.FullRowSelect = true;
            listViewSections.GridLines = true;
            listViewSections.MultiSelect = false;
            listViewSections.HideSelection = false;

            // 添加列
            listViewSections.Columns.Add("开始时间", 150, HorizontalAlignment.Center);
            listViewSections.Columns.Add("结束时间", 150, HorizontalAlignment.Center);
            listViewSections.Columns.Add("影厅", 80, HorizontalAlignment.Center);
            listViewSections.Columns.Add("影厅类型", 200, HorizontalAlignment.Center);

            // 双击事件
            listViewSections.DoubleClick += (sender, e) =>
            {
                if (listViewSections.SelectedItems.Count > 0)
                {
                    var selectedSection = (Section)listViewSections.SelectedItems[0].Tag;
                    OpenSeatSelectionForm(selectedSection);
                }
            };
        }

        private void LoadSections()
        {
            listViewSections.Items.Clear();

            DateTime currentTime = DateTime.Now;
            _availableSections = _showingService.GetFilmShowings(_selectedFilm.FilmName, _selectedDate)
                .Where(s => s.TimeSlot.StartTime > currentTime)
                .OrderBy(s => s.TimeSlot.StartTime)
                .ToList();

            if (!_availableSections.Any())
            {
                MessageBox.Show(_selectedDate.Date == DateTime.Today
                    ? "今天没有更多场次了"
                    : "所选日期没有可用场次",
                    "提示",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                DialogResult = DialogResult.Cancel; // 设置对话框结果
                this.Close();
                return;
            }

            foreach (var section in _availableSections)
            {
                var item = new ListViewItem(section.TimeSlot.StartTime.ToString("HH\\:mm"));
                item.SubItems.Add(section.TimeSlot.EndTime.ToString("HH\\:mm"));
                item.SubItems.Add(section.MovieHall.HallNo.ToString());         // int转string
                item.SubItems.Add(section.MovieHall.Category.ToString());
                item.Tag = section;

                listViewSections.Items.Add(item);
            }

            // 自动调整列宽
            //listViewSections.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
        }

        private void OpenSeatSelectionForm(Section section)
        {
            using (var seatForm = new SeatSelectionForm(section, _selectedFilm,_loggedInCustomer,_mainForm))
             {
                if (seatForm.ShowDialog() == DialogResult.OK)
                {
                     this.DialogResult = DialogResult.OK;
                     this.Close();
                }
              }
        }
    }
}