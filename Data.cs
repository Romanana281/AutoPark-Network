using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project3
{
    public partial class Data : Form
    {
        private Timer progressTimer;
        public BackgroundWorker loadWorker;

        private string currentBrand = "";
        private string currentType = "";
        private bool isServerLoading = false;

        private Client client;

        ListCar list;
        DataGridView dataGridView1;

        public Data(ListCar list, string brandName)
        {
            InitializeComponent();
            this.list = list;
            this.currentBrand = brandName;
            this.Text = $"Данные для марки: {brandName}";
            client = list.client;

            progressTimer = new Timer();
            progressTimer.Interval = 100;
            progressTimer.Tick += ProgressTimer_Tick;

            loadWorker = new BackgroundWorker();
            loadWorker.WorkerSupportsCancellation = true;
            loadWorker.WorkerReportsProgress = true;
            loadWorker.DoWork += LoadWorker_DoWork;
            loadWorker.ProgressChanged += LoadWorker_ProgressChanged;
            loadWorker.RunWorkerCompleted += LoadWorker_RunWorkerCompleted;

            dataGridView2.CellValueChanged += DataGridView2_CellValueChanged;
            dataGridView2.CellValidating += DataGridView2_CellValidating;
            dataGridView2.CellBeginEdit += DataGridView2_CellBeginEdit;
            dataGridView2.CellEndEdit += DataGridView2_CellEndEdit;
            dataGridView2.MultiSelect = false;
        }

        private void DataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            list.Enabled = false;
        }

        private void DataGridView2_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {

            DataGridViewRow row = dataGridView2.Rows[e.RowIndex];
            IAuto auto = row?.DataBoundItem as IAuto;
            string columnName = dataGridView2.Columns[e.ColumnIndex].Name;
            bool isValid = true;

            if (auto is Car &&
                (string.IsNullOrWhiteSpace(row.Cells[columnName].Value?.ToString()) ||
                 !int.TryParse(row.Cells[columnName].Value?.ToString(), out int airbags) ||
                 !int.TryParse(row.Cells[columnName].Value?.ToString(), out int wheels))
                )
            {
                isValid = false;
            }
            else if (auto is Truck &&
                (string.IsNullOrWhiteSpace(row.Cells[columnName].Value?.ToString()) ||
                 !float.TryParse(row.Cells[columnName].Value?.ToString(), out float bodyVolume) ||
                 !string.IsNullOrEmpty(row.Cells[columnName].Value?.ToString().Trim()))
                )
            {
                isValid = false;
            }

            if (isValid)
            {
                list.Enabled = true;
            }
            else
            {
                dataGridView2.BeginEdit(true);
            }
        }

        public void LoadDataForBrand(string brandName, string type, DataGridView data)
        {
            dataGridView1 = data;
            if (string.IsNullOrWhiteSpace(brandName)) return;

            progressBar1.Visible = true;
            progressBar1.Value = 0;
            isServerLoading = false;

            if (loadWorker.IsBusy)
            {
                loadWorker.CancelAsync();
            }

            progressTimer.Start();

            IAuto selectedAuto = null;
            if (dataGridView1.CurrentRow?.DataBoundItem is IAuto auto)
                selectedAuto = auto;
            
            currentType = type;

            loadWorker.RunWorkerAsync(new { BrandName = brandName, Prototype = selectedAuto });
        }

        private void LoadWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var arg = e.Argument as dynamic;
            string brandName = arg.BrandName;
            IAuto prototype = arg.Prototype;

            var loadedData = Loader.load(brandName, prototype, (auto) =>
            {
                loadWorker.ReportProgress(0, auto);
            });

            e.Result = new { BrandName = brandName, Data = loadedData };
        }

        private void LoadWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var auto = e.UserState as IAuto;
            if (auto == null) return;

            string brandName = auto.BrandName;

            list.AddDictionary(auto);

            if (!string.IsNullOrEmpty(brandName))
            {
                var brandRow = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(row =>
                           row.DataBoundItem is IAuto rowAuto &&
                           rowAuto.BrandName == brandName &&
                           rowAuto.ModelName == auto.ModelName);

                if (brandRow != null)
                {
                    if (!string.IsNullOrEmpty(auto.Type))
                    {
                        currentType = auto.Type;
                    }

                    dataGridView1.ClearSelection();
                    ShowTable(brandRow);
                }
            }
        }

        private void LoadWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Загрузка была отменена", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (e.Error != null)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {e.Error.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = e.Result as dynamic;
            string brandName = result.BrandName;
            List<IAuto> loadedData = result.Data;

            var existingCars = list.cars.ContainsKey(brandName) ? list.cars[brandName] : new List<Car>();
            var existingTrucks = list.trucks.ContainsKey(brandName) ? list.trucks[brandName] : new List<Truck>();

            foreach (var auto in loadedData)
            {
                if (auto is Car newCar)
                {
                    if (!existingCars.Any(c => c.RegNumber == newCar.RegNumber))
                    {
                        list.AddDictionary(newCar);
                    }
                }
                else if (auto is Truck newTruck)
                {
                    if (!existingTrucks.Any(t => t.RegNumber == newTruck.RegNumber))
                    {
                        list.AddDictionary(newTruck);
                    }
                }
            }

            if (dataGridView1.Rows.Count > 0)
            {
                var brandRow = dataGridView1.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(row =>
                        row.DataBoundItem is IAuto auto &&
                        auto.BrandName == brandName);

                if (brandRow != null && brandRow.Index == dataGridView1.CurrentRow?.Index)
                {
                    ShowTable(brandRow);
                }
            }


            isServerLoading = true;
            progressBar1.Value = 0;

            BackgroundWorker newServerWorker = new BackgroundWorker();
            newServerWorker.WorkerSupportsCancellation = true;
            newServerWorker.WorkerReportsProgress = true;

            newServerWorker.DoWork += (s, args) =>
            {
                var arg = args.Argument as dynamic;
                string bName = arg.BrandName;
                string bType = arg.Type;
                Client bClient = arg.Client;

                var serverData = Loader.LoadFromServer(bName, bType, bClient, (auto) =>
                {
                    newServerWorker.ReportProgress(0, auto);
                });

                args.Result = new { BrandName = bName, Data = serverData };
            };

            newServerWorker.ProgressChanged += (s, args) =>
            {
                var auto = args.UserState as IAuto;
                if (auto == null) return;

                string bName = auto.BrandName;

                list.AddDictionary(auto);
                if (!list.carList.Any(c =>
                    c.ModelName == auto.ModelName))
                {
                    list.carList.Add(auto);
                }

                if (!string.IsNullOrEmpty(bName))
                {
                    var brandRow = dataGridView1.Rows
                        .Cast<DataGridViewRow>()
                        .FirstOrDefault(row =>
                            row.DataBoundItem is IAuto rowAuto &&
                            rowAuto.BrandName == bName);

                    if (brandRow != null)
                    {
                        dataGridView1.ClearSelection();
                        brandRow.Selected = true;
                        ShowTable(brandRow);
                    }
                    else if (dataGridView1.Rows.Count > 0)
                    {
                        dataGridView1.Rows[0].Selected = true;
                        ShowTable(dataGridView1.Rows[0]);
                    }
                }
                else if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.Rows[0].Selected = true;
                    ShowTable(dataGridView1.Rows[0]);
                }
            };

            newServerWorker.RunWorkerCompleted += (s, args) =>
            {
                progressTimer.Stop();
                progressBar1.Visible = false;
                isServerLoading = false;

                if (args.Cancelled) return;
                if (args.Error != null) return;

                var serverResult = args.Result as dynamic;
                string serverBrandName = serverResult.BrandName;
                List<IAuto> serverData = serverResult.Data;

                var existingCar = list.cars.ContainsKey(serverBrandName) ? list.cars[serverBrandName] : new List<Car>();
                var existingTruck = list.trucks.ContainsKey(serverBrandName) ? list.trucks[serverBrandName] : new List<Truck>();

                foreach (var auto in serverData)
                {
                    if (auto is Car newCar)
                    {
                        if (!existingCar.Any(c => c.RegNumber == newCar.RegNumber))
                        {
                            list.AddDictionary(newCar);
                        }
                    }
                    else if (auto is Truck newTruck)
                    {
                        if (!existingTruck.Any(t => t.RegNumber == newTruck.RegNumber))
                        {
                            list.AddDictionary(newTruck);
                        }
                    }
                }

                newServerWorker.Dispose();
            };

            newServerWorker.RunWorkerAsync(new { BrandName = brandName, Type = currentType, Client = client });

        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (!isServerLoading)
            {
                progressBar1.Value = Loader.getProgress();
            }
            else
            {
                progressBar1.Value = Loader.getServerProgress();
            }
        }

        private void DataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (loadWorker.IsBusy) return;

            if (!(dataGridView1.CurrentRow?.DataBoundItem is IAuto auto)) return;

            DataGridViewRow row = dataGridView2.Rows[e.RowIndex];

            string regNumber = row.Cells["RegNumber"].Value?.ToString();
            if (auto is Car)
            {
                if (!list.cars.ContainsKey(auto.BrandName))
                {
                    list.cars[auto.BrandName] = new List<Car>();
                }

                string oldRegNumber = row.Tag?.ToString();
                var c = list.cars[auto.BrandName].Find(a => a.RegNumber == oldRegNumber);

                if (c != null)
                {
                    c.RegNumber = regNumber;
                    c.Multimedia = row.Cells["Multimedia"].Value?.ToString();

                    if (int.TryParse(row.Cells["Airbags"].Value?.ToString(), out int airbag))
                    {
                        c.Airbags = airbag;
                    }
                    row.Tag = regNumber;
                }
                else if (!string.IsNullOrWhiteSpace(regNumber) &&
                    !string.IsNullOrWhiteSpace(row.Cells["Multimedia"].Value?.ToString()) &&
                    int.TryParse(row.Cells["Airbags"].Value?.ToString(), out int airbag) &&
                    airbag > 0)
                {
                    var newCar = new Car()
                    {
                        BrandName = auto.BrandName,
                        ModelName = auto.ModelName,
                        RegNumber = regNumber,
                        Multimedia = row.Cells["Multimedia"].Value?.ToString(),
                        Airbags = airbag
                    };

                    list.cars[auto.BrandName].Add(newCar);

                    row.Tag = regNumber;
                }
            }
            else if (auto is Truck)
            {
                if (!list.trucks.ContainsKey(auto.BrandName))
                {
                    list.trucks[auto.BrandName] = new List<Truck>();
                }

                string oldRegNumber = row.Tag?.ToString();
                var c = list.trucks[auto.BrandName].Find(a => a.RegNumber == oldRegNumber);

                if (c != null)
                {
                    c.RegNumber = regNumber;
                    if (int.TryParse(row.Cells["Wheel"].Value?.ToString(), out int wheels))
                    {
                        c.Wheel = wheels;
                    }

                    if (int.TryParse(row.Cells["BodyVolume"].Value?.ToString(), out int bodyVolumes))
                    {
                        c.BodyVolume = bodyVolumes;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(regNumber) &&
                    int.TryParse(row.Cells["Wheel"].Value?.ToString(), out int wheels) &&
                    wheels > 0 &&
                    int.TryParse(row.Cells["BodyVolume"].Value?.ToString(), out int bodyVolumes) &&
                    bodyVolumes > 0)
                {
                    Truck newTruck = new Truck()
                    {
                        BrandName = auto.BrandName,
                        ModelName = auto.ModelName,
                        RegNumber = regNumber,
                        Wheel = wheels,
                        BodyVolume = bodyVolumes
                    };

                    list.trucks[auto.BrandName].Add(newTruck);

                    row.Tag = regNumber;
                }
            }
        }

        private void DataGridView2_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (loadWorker.IsBusy) return;

            DataGridViewRow row = dataGridView2.Rows[e.RowIndex];
            string columnName = dataGridView2.Columns[e.ColumnIndex].Name;

            string errorMessage = "";

            try
            {
                switch (columnName)
                {
                    case "RegNumber":
                        string regNumber = e.FormattedValue?.ToString()?.Trim().ToUpper();
                        if (string.IsNullOrWhiteSpace(regNumber))
                        {
                            errorMessage = "Гос. номер не может быть пустым";
                        }
                        else if (regNumber.Length > 15)
                        {
                            errorMessage = "Гос. номер не может превышать 15 символов";
                        }
                        else if (dataGridView2.Rows.Cast<DataGridViewRow>()
                            .Where(r => r.Index != e.RowIndex)
                            .Any(r => r.Cells["RegNumber"].Value?.ToString() == regNumber))
                        {
                            errorMessage = "Такой гос. номер уже существует";
                        }
                        break;

                    case "Airbags":
                        if (!int.TryParse(e.FormattedValue?.ToString(), out int airbags) || airbags <= 0)
                        {
                            errorMessage = "Количество подушек безопасности должно быть больше 0";
                        }
                        else if (airbags > 20)
                        {
                            errorMessage = "Количество подушек безопасности не может превышать 20";
                        }
                        break;

                    case "Wheel":
                        if (!int.TryParse(e.FormattedValue?.ToString(), out int wheels) || wheels <= 0)
                        {
                            errorMessage = "Количество колес должно быть числом больше 0";
                        }
                        else if (wheels > 20)
                        {
                            errorMessage = "Количество колес не может превышать 20";
                        }
                        break;

                    case "BodyVolume":
                        if (!float.TryParse(e.FormattedValue?.ToString(), out float bodyVolume) || bodyVolume <= 0)
                        {
                            errorMessage = "Объем кузова должен быть больше 0";
                        }
                        else if (bodyVolume > 1000)
                        {
                            errorMessage = "Объем кузова не может превышать 1000";
                        }
                        break;

                    case "Multimedia":
                        string multimedia = e.FormattedValue?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(multimedia) && multimedia.Length > 100)
                        {
                            errorMessage = "Название мультимедиа не может превышать 100 символов";
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    dataGridView2.Rows[e.RowIndex].ErrorText = errorMessage;
                }
                else
                {
                    dataGridView2.Rows[e.RowIndex].ErrorText = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка валидации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        public void ShowTable(DataGridViewRow rows)
        {
            list.UpdateColor(rows);

            if (rows?.DataBoundItem is IAuto auto)
            {

                if (auto.BrandName != currentBrand) return;

                if (auto.BrandName == "Не указано" || auto.ModelName == "Не указано" || auto.Power <= 0 || auto.MaxSpeed <= 0) return;
                if (dataGridView2 == null) return;

                dataGridView2?.Rows.Clear();
                dataGridView2.Columns.Clear();
                dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                DataGridViewColumn portColumn = new DataGridViewTextBoxColumn();
                portColumn.Name = "Port";
                portColumn.HeaderText = "Порт";
                portColumn.ReadOnly = true;
                dataGridView2.Columns.Add(portColumn);

                DataGridViewColumn regNumber = new DataGridViewColumn();
                regNumber.Name = "RegNumber";
                regNumber.HeaderText = "Гос. номер";
                regNumber.CellTemplate = new DataGridViewTextBoxCell();
                dataGridView2.Columns.Add(regNumber);

                if (auto.Type == "Легковая")
                {
                    DataGridViewColumn multimedia = new DataGridViewColumn();
                    multimedia.Name = "Multimedia";
                    multimedia.HeaderText = "Название мультимедиа";
                    multimedia.CellTemplate = new DataGridViewTextBoxCell();
                    dataGridView2.Columns.Add(multimedia);

                    DataGridViewColumn airbags = new DataGridViewColumn();
                    airbags.Name = "Airbags";
                    airbags.HeaderText = "Подушки безопасности";
                    airbags.CellTemplate = new DataGridViewTextBoxCell();
                    dataGridView2.Columns.Add(airbags);

                    if (list.cars.ContainsKey(auto.BrandName))
                    {
                        foreach (Car car in list.cars[auto.BrandName])
                        {
                            if (auto.BrandName == car.BrandName)
                            {
                                int rowIndex = dataGridView2.Rows.Add(car.Port, car.RegNumber, car.Multimedia, car.Airbags);
                                dataGridView2.Rows[rowIndex].Tag = car.RegNumber;
                            }
                        }
                    }
                }
                else if (auto.Type == "Грузовая")
                {
                    DataGridViewColumn wheel = new DataGridViewColumn();
                    wheel.Name = "Wheel";
                    wheel.HeaderText = "Колеса";
                    wheel.CellTemplate = new DataGridViewTextBoxCell();
                    dataGridView2.Columns.Add(wheel);

                    DataGridViewColumn bodyVolume = new DataGridViewColumn();
                    bodyVolume.Name = "BodyVolume";
                    bodyVolume.HeaderText = "Объем кузова";
                    bodyVolume.CellTemplate = new DataGridViewTextBoxCell();
                    dataGridView2.Columns.Add(bodyVolume);

                    if (list.trucks.ContainsKey(auto.BrandName))
                    {
                        foreach (Truck truck in list.trucks[auto.BrandName])
                        {
                            if (auto.BrandName == truck.BrandName)
                            {
                                int rowIndex = dataGridView2.Rows.Add(truck.Port, truck.RegNumber, truck.Wheel, truck.BodyVolume);
                                dataGridView2.Rows[rowIndex].Tag = truck.RegNumber;
                            }
                        }
                    }
                }
            }
        }
    }
}
