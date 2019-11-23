using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XmlDBTest
{
    public partial class MainForm : Form
    {
        // we are using the dataset as our database
        DataSet _dataBase = null;
        
        // category table and the binding source
        DataTable _categoryTable = null;
        BindingSource _categoryBS = null;

        // jobs table and the binding source
        DataTable _jobTable = null;
        BindingSource _jobBS = null;

        public MainForm()
        {
            InitializeComponent();
            // Create a dataset to hold our database in memory
            _dataBase = new DataSet("JobsDatabase");

            // Create a category table
            _categoryTable = new DataTable("Categories");
            _categoryTable.Columns.Add(new DataColumn("CategoryId", typeof(Int32)));
            _categoryTable.Columns.Add(new DataColumn("CategoryName", typeof(string)));
            _categoryTable.Columns.Add(new DataColumn("StartDate", typeof(DateTime)));
            _categoryTable.Columns.Add(new DataColumn("IsFinished", typeof(bool)));
            // Set the primary key
            _categoryTable.PrimaryKey = new DataColumn[] { _categoryTable.Columns["CategoryId"] };
            // Make it autoincrement, 1 by 1
            _categoryTable.Columns["CategoryId"].AutoIncrement = true;
            _categoryTable.Columns["CategoryId"].AutoIncrementSeed = 1;
            // Add the table to the database
            _dataBase.Tables.Add(_categoryTable);

            _jobTable = new DataTable("Jobs");
            _jobTable.Columns.Add(new DataColumn("JobId", typeof(Int32)));
            _jobTable.Columns.Add(new DataColumn("CategoryId", typeof(Int32)));
            _jobTable.Columns.Add(new DataColumn("Description", typeof(string)));
            _jobTable.Columns.Add(new DataColumn("StartDate", typeof(DateTime)));
            _jobTable.Columns.Add(new DataColumn("EndDate", typeof(DateTime)));
            _jobTable.Columns.Add(new DataColumn("IsFinished", typeof(bool)));
            _jobTable.PrimaryKey = new DataColumn[] { _jobTable.Columns["JobId"] };

            // Fantasize with auto increment seed, starts from 100, increases 3 by 3
            _jobTable.Columns["JobId"].AutoIncrement = true;
            _jobTable.Columns["JobId"].AutoIncrementSeed = 100;
            _jobTable.Columns["JobId"].AutoIncrementStep = 3;
            _dataBase.Tables.Add(_jobTable);

            // Set table relations
            _dataBase.Relations.Add("CategoryJob", _categoryTable.Columns["CategoryId"], _jobTable.Columns["CategoryId"]);
            _dataBase.Relations["CategoryJob"].Nested = true;

            // Create a binding source and set it as a datasource for our grid
            _categoryBS = new BindingSource();
            _categoryBS.DataSource = _categoryTable;
            dgvCategory.DataSource = _categoryBS;

            _jobBS = new BindingSource();
            _jobBS.DataSource = _jobTable;
            dgvJob.DataSource = _jobBS;

            // set RTB data binding
            richTextBox1.DataBindings.Add("Text", _jobBS, "Description");

            // filter jobs according to the category
            _categoryBS.CurrentChanged += _categoryBS_CurrentChanged;

            // make grids faster
            SetGridDoubleBufferingOn(dgvCategory);
            SetGridDoubleBufferingOn(dgvJob);
        }

        private void _categoryBS_CurrentChanged(object sender, EventArgs e)
        {
            if (_categoryBS.Current == null)
                _jobBS.Filter = "";
            else
                _jobBS.Filter = "CategoryId=" + (_categoryBS.Current as DataRowView).Row["CategoryId"];
        }

        private void çıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void SaveDatabase_Click(object sender, EventArgs e)
        {
            dgvCategory.EndEdit();
            dgvJob.EndEdit();
            using (SaveFileDialog sd = new SaveFileDialog())
            {
                sd.Filter = "XML files (*.xml)|*.xml";
                var res = sd.ShowDialog();

                if (res == DialogResult.OK)
                {
                    _dataBase.WriteXml(sd.FileName);
                }
            }
        }

        // get the current category Id for our new job
        private void newJobToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_categoryBS.Current == null)
                return;

            var row = (DataRowView)_jobBS.AddNew();
            row["CategoryID"] = (_categoryBS.Current as DataRowView).Row["CategoryID"];
            row["StartDate"] = DateTime.Now;
        }

        private void addNewCategory_Click(object sender, EventArgs e)
        {
            var row = (DataRowView)_categoryBS.AddNew();
            row["StartDate"] = DateTime.Now;
        }

        private void openDatabase_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog od = new OpenFileDialog())
            {
                od.Filter = "XML files (*.xml)|*.xml";
                var res = od.ShowDialog();

                if (res == DialogResult.OK)
                {
                    _dataBase.ReadXml(od.FileName);
                }
            }
        }
        public void SetGridDoubleBufferingOn(DataGridView dgv)
        {
            // Double buffering can make DGV slow in remote desktop
            if (!SystemInformation.TerminalServerSession)
            {
                typeof(DataGridView).InvokeMember(
                   "DoubleBuffered",
                   BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                   null,
                   dgv,
                   new object[] { true });
            }
        }
    }
}
