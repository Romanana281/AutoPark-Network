using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace Project3
{
    public partial class ListCar : Form
    {
        public BindingList<IAuto> carList = new BindingList<IAuto>();
        public Dictionary<string, List<Car>> cars = new Dictionary<string, List<Car>>();
        public Dictionary<string, List<Truck>> trucks = new Dictionary<string, List<Truck>>();

        private Dictionary<string, Data> brandForms = new Dictionary<string, Data>();
        private Random rand = new Random();

        public Server server = new Server();
        public Client client = new Client();

        private bool statusServer = false;
        private Timer statusTimer;

        [Serializable]
        public class SaveData
        {
            public List<Car> Cars { get; set; } = new List<Car>();
            public List<Truck> Trucks { get; set; } = new List<Truck>();
        }

        public ListCar()
        {
            InitializeComponent();
            this.MinimumSize = new System.Drawing.Size(this.Width, 0);
            this.MaximumSize = new System.Drawing.Size(this.Width, int.MaxValue);
            MenuBox.MouseLeave += ShowBox;

            statusTimer = new Timer();
            statusTimer.Interval = 500;
            statusTimer.Tick += StatusTimer_Tick;

            pictureBox1.Image = Properties.Resources.gray_dot;
            label2.Text = "Офлайн";
        }

        private void StatusTimer_Tick(object sender, EventArgs e)
        {
            if (!statusServer)
            {
                pictureBox1.Image = null;
            }
            else
            {
                pictureBox1.Image = Properties.Resources.green_dot;
            }
            statusServer = !statusServer;
        }

        private void ListCar_Load(object sender, EventArgs e)
        {
            bindingSource1.DataSource = carList;
            dataGridView1.DataSource = bindingSource1;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dataGridView1.Columns["BrandName"].HeaderText = "Наименование марки";
            dataGridView1.Columns["ModelName"].HeaderText = "Наименование модели";
            dataGridView1.Columns["Power"].HeaderText = "Мощность л.с.";
            dataGridView1.Columns["MaxSpeed"].HeaderText = "Максимальная скорость";

            dataGridView1.Columns.Remove("Type");
            DataGridViewComboBoxColumn newColumn = new DataGridViewComboBoxColumn();
            newColumn.Name = "Type";
            newColumn.HeaderText = "Тип";
            newColumn.DataPropertyName = "Type";
            newColumn.Items.AddRange("Грузовая", "Легковая");
            dataGridView1.Columns.Add(newColumn);

            dataGridView1.SelectionChanged += DataGridView1_SelectionChangedAsync;
            dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
            dataGridView1.RowsAdded += DataGridView1_RowsAdded;
            dataGridView1.CellValidating += DataGridView1_CellValidating;
            dataGridView1.MultiSelect = false;
        }

        public void AddDictionary(IAuto auto)
        {
            if (string.IsNullOrEmpty(auto.BrandName) || string.IsNullOrWhiteSpace(auto.BrandName)) return;

            if (auto is Car car)
            {
                if (string.IsNullOrEmpty(car.RegNumber) || string.IsNullOrWhiteSpace(car.RegNumber)) return;

                if (!cars.ContainsKey(auto.BrandName))
                {
                    cars[auto.BrandName] = new List<Car>();
                }
                if (!cars[auto.BrandName].Any(c => c.RegNumber == car.RegNumber))
                {
                    cars[auto.BrandName].Add(car);
                    client.SendData(new List<IAuto> { car }, server.port);
                }
            }
            else if (auto is Truck truck)
            {
                if (string.IsNullOrEmpty(truck.RegNumber) || string.IsNullOrWhiteSpace(truck.RegNumber)) return;

                if (!trucks.ContainsKey(auto.BrandName))
                {
                    trucks[auto.BrandName] = new List<Truck>();
                }
                if (!trucks[auto.BrandName].Any(c => c.RegNumber == truck.RegNumber))
                {
                    trucks[auto.BrandName].Add(truck);
                    client.SendData(new List<IAuto> { truck }, server.port);
                }
            }
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            IAuto auto = row.DataBoundItem as IAuto;

            if (auto.BrandName != "Не указано" && auto.ModelName != "Не указано" && auto.Power > 0 && auto.MaxSpeed > 0)
            {
                if (brandForms.ContainsKey(auto.BrandName))
                {
                    brandForms[auto.BrandName].Show();
                }
            }

            string colName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (colName == "BrandName" && auto.BrandName != "Не указано")
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
            else if (colName == "ModelName" && auto.ModelName != "Не указано")
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
            else if (colName == "Power" && auto.Power != 0)
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
            else if (colName == "MaxSpeed" && auto.MaxSpeed != 0)
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].ReadOnly = true;
            else if (colName == "Type")
            {
                int index = carList.IndexOf(auto);
                if (index == -1) return;

                if (row.Cells["Type"].Value?.ToString() == "Грузовая" && auto is Car)
                {
                    IAuto oldCar = carList[index];

                    Truck truck = new Truck
                    {
                        BrandName = oldCar.BrandName,
                        ModelName = oldCar.ModelName,
                        Power = oldCar.Power,
                        MaxSpeed = oldCar.MaxSpeed,
                        Type = "Грузовая"
                    };
                    carList[index] = truck;

                    //if (cars.ContainsKey(oldCar.BrandName))
                    //{
                    //    foreach (var carAuto in cars[oldCar.BrandName].FindAll(l => l.ModelName == oldCar.ModelName))
                    //    {
                    //        Truck t = new Truck
                    //        {
                    //            BrandName = oldCar.BrandName,
                    //            ModelName = oldCar.ModelName,
                    //            Power = oldCar.Power,
                    //            MaxSpeed = oldCar.MaxSpeed,
                    //            Type = "Грузовая",
                    //            RegNumber = carAuto.RegNumber,
                    //            Wheel = rand.Next(4, 13),
                    //            BodyVolume = rand.Next(10, 51)
                    //        };
                    //        AddDictionary(t);

                    //        cars[oldCar.BrandName]
                    //           .RemoveAll(x =>
                    //                x.RegNumber == carAuto.RegNumber &&
                    //                x.ModelName == oldCar.ModelName);
                    //    }
                    //}

                }
                else if (row.Cells["Type"].Value?.ToString() == "Легковая" && auto is Truck)
                {
                    IAuto oldTruck = carList[index];

                    Car car = new Car
                    {
                        BrandName = oldTruck.BrandName,
                        ModelName = oldTruck.ModelName,
                        Power = oldTruck.Power,
                        MaxSpeed = oldTruck.MaxSpeed,
                        Type = "Легковая"
                    };
                    carList[index] = car;

                    //if (trucks.ContainsKey(oldTruck.BrandName))
                    //{
                    //    foreach (var truckAuto in cars[oldTruck.BrandName].FindAll(x => x.ModelName == oldTruck.ModelName))
                    //    {
                    //        Car c = new Car
                    //        {
                    //            BrandName = oldTruck.BrandName,
                    //            ModelName = oldTruck.ModelName,
                    //            Power = oldTruck.Power,
                    //            MaxSpeed = oldTruck.MaxSpeed,
                    //            Type = "Легковая",
                    //            RegNumber = truckAuto.RegNumber,
                    //            Multimedia = $"Multimedia_{rand.Next(1, 6)}",
                    //            Airbags = rand.Next(2, 9)
                    //        };
                    //        AddDictionary(c);

                    //        trucks[oldTruck.BrandName]
                    //            .RemoveAll(x =>
                    //                x.RegNumber == truckAuto.RegNumber &&
                    //                x.ModelName == oldTruck.ModelName);
                    //    }
                    //}
                }
                UpdateColor(row);
            }
        }

        private void DataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            Data data = new Data(this, "");
            if (data.loadWorker.IsBusy) return;

            DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
            IAuto auto = row.DataBoundItem as IAuto;
            if (auto == null) return;

            string columnName = dataGridView1.Columns[e.ColumnIndex].Name;
            string errorMessage = "";

            try
            {
                switch (columnName)
                {
                    case "BrandName":
                        string brandName = e.FormattedValue?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(brandName) || brandName == "Не указано")
                        {
                            errorMessage = "Поле 'Марка' не может быть пустым";
                        }
                        else if (!brandName.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                        {
                            errorMessage = "Поле 'Марка' может содержать только буквы и пробелы";
                        }
                        else if (brandName.Length > 50)
                        {
                            errorMessage = "Название марки не может превышать 50 символов";
                        }
                        break;

                    case "ModelName":
                        string newModelName = e.FormattedValue?.ToString()?.Trim();
                        if (string.IsNullOrWhiteSpace(newModelName) || newModelName == "Не указано")
                        {
                            errorMessage = "Поле 'Модель' не может быть пустым";
                        }
                        else if (newModelName.Length > 50)
                        {
                            errorMessage = "Название модели не может превышать 50 символов";
                        }
                        else if (carList.Any(c =>
                            c.BrandName == auto.BrandName &&
                            c.ModelName == newModelName &&
                            c != auto))
                        {
                            errorMessage = "Модель с таким названием уже существует для этой марки";
                        }
                        break;

                    case "Power":
                        if (!int.TryParse(e.FormattedValue?.ToString(), out int power) || power <= 0)
                        {
                            errorMessage = "Мощность должна быть больше 0";
                        }
                        else if (power > 2000)
                        {
                            errorMessage = "Мощность не может превышать 2000 л.с.";
                        }
                        break;

                    case "MaxSpeed":
                        if (!int.TryParse(e.FormattedValue?.ToString(), out int maxSpeed) || maxSpeed <= 0)
                        {
                            errorMessage = "Максимальная скорость должна быть больше 0";
                        }
                        else if (maxSpeed > 500)
                        {
                            errorMessage = "Максимальная скорость не может превышать 500 км/ч";
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    MessageBox.Show(errorMessage, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                    dataGridView1.Rows[e.RowIndex].ErrorText = errorMessage;
                }
                else
                {
                    dataGridView1.Rows[e.RowIndex].ErrorText = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка валидации: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
            }
        }

        private void DataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (int i = e.RowIndex; i < e.RowIndex + e.RowCount; i++)
            {
                if (i < dataGridView1.Rows.Count)
                {
                    UpdateColor(dataGridView1.Rows[i]);
                }
            }
        }

        private void DataGridView1_SelectionChangedAsync(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow rows = dataGridView1.SelectedRows[0];
                if (rows.DataBoundItem is IAuto auto)
                {
                    rows.Tag = auto;
                    AddDictionary(auto);

                    if (auto.BrandName != "Не указано" && auto.ModelName != "Не указано" && auto.Power > 0 && auto.MaxSpeed > 0)
                    {
                        ShowOrCreateBrandFormAsync(auto.BrandName, auto.Type, rows);
                    }
                }
            }
            dataGridView1.ClearSelection();
        }

        private void ShowOrCreateBrandFormAsync(string brandName, string type, DataGridViewRow row)
        {
            if (!brandForms.ContainsKey(brandName))
            {
                Data dataForm = new Data(this, brandName);
                dataForm.FormClosed += (s, e) =>
                {
                    if (brandForms.ContainsKey(brandName))
                        brandForms.Remove(brandName);
                };
                brandForms[brandName] = dataForm;
                dataForm.Show();

                dataForm.LoadDataForBrand(brandName, type, dataGridView1);
            }
            else
            {
                var dataForm = brandForms[brandName];
                if (!dataForm.loadWorker.IsBusy)
                {
                    dataForm.LoadDataForBrand(brandName, type, dataGridView1);
                }
            }

            brandForms[brandName].ShowTable(row);
        }

        public void UpdateColor(DataGridViewRow rows)
        {
            if (rows?.Cells["Type"]?.Value?.ToString() == "Грузовая")
            {
                rows.DefaultCellStyle.BackColor = Color.LightBlue;
            }
            else
            {
                rows.DefaultCellStyle.BackColor = Color.LightGreen;
            }
        }

        private void ShowBox(object sender, EventArgs e)
        {
            if (!MenuBox.Bounds.Contains(PointToClient(Cursor.Position)))
            {
                MenuBox.Visible = false;
            }
        }

        private void MenuB_Click(object sender, EventArgs e)
        {
            MenuBox.Visible = true; Menu.Visible = true; ShowMenuB.Visible = true; onServer.Visible = true;

            SaveB.Visible = false; LoadB.Visible = false; ExitB.Visible = false;
        }

        private void ShowMenuB_Click(object sender, EventArgs e)
        {
            Menu.Visible = false; ShowMenuB.Visible = false; onServer.Visible = false;

            SaveB.Visible = true; LoadB.Visible = true; ExitB.Visible = true;
        }

        private void SaveB_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "XML files (*.xml)|*.xml";
                saveFileDialog.Title = "Сохранить данные";
                saveFileDialog.DefaultExt = "xml";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        SaveToXml(saveFileDialog.FileName);
                        MessageBox.Show("Данные успешно сохранены!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void LoadB_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "XML files (*.xml)|*.xml";
                openFileDialog.Title = "Загрузить данные";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        LoadFromXml(openFileDialog.FileName);
                        MessageBox.Show("Данные успешно загружены!", "Успех",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void SaveToXml(string filePath)
        {
            var saveData = new SaveData();

            foreach (var auto in carList)
            {
                if (auto is Car car)
                    saveData.Cars.Add(car);
                else if (auto is Truck truck)
                    saveData.Trucks.Add(truck);
            }

            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(stream, saveData);
            }
        }

        private void LoadFromXml(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не найден");

            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            SaveData loadedData;

            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                loadedData = serializer.Deserialize(stream) as SaveData;
            }

            dataGridView1.CellValueChanged -= DataGridView1_CellValueChanged;
            dataGridView1.SelectionChanged -= DataGridView1_SelectionChangedAsync;

            try
            {
                carList.Clear();
                cars.Clear();
                trucks.Clear();

                foreach (var form in brandForms.Values.ToList())
                {
                    form.Close();
                }
                brandForms.Clear();

                foreach (var car in loadedData.Cars)
                {
                    carList.Add(car);
                }

                foreach (var truck in loadedData.Trucks)
                {
                    carList.Add(truck);
                }

                bindingSource1.ResetBindings(false);

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        UpdateColor(row);
                    }
                }

                Loader.ClearCache();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dataGridView1.CellValueChanged += DataGridView1_CellValueChanged;
                dataGridView1.SelectionChanged += DataGridView1_SelectionChangedAsync;
            }
        }

        private void ExitB_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Вы вышли из программы", "Exit", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Exit();
        }

        private void AddModelB_Click(object sender, EventArgs e)
        {
            if (carList.Count > 0)
            {
                var lastCar = carList[carList.Count - 1];

                if (string.IsNullOrWhiteSpace(lastCar.BrandName) || lastCar.BrandName == "Не указано" ||
                    string.IsNullOrWhiteSpace(lastCar.ModelName) || lastCar.ModelName == "Не указано" ||
                    lastCar.Power <= 0 || lastCar.MaxSpeed <= 0)
                {
                    MessageBox.Show("Сначала заполните все поля текущей марки!\n\n- Марка\n- Модель\n- Мощность > 0\n- Макс. скорость > 0",
                                  "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            Car car = new Car();
            carList.Add(car);
        }

        private async void onServer_Click(object sender, EventArgs e)
        {
            if (!server.isStatusServer)
            {
                pictureBox1.Image = Properties.Resources.green_dot;
                label2.Text = "Онлайн";
                statusTimer.Start();
                _ = Task.Run(async () => await server.startDiscovery());
                await server.startServer();
            }
            else
            {
                pictureBox1.Image = Properties.Resources.gray_dot;
                label2.Text = "Офлайн";
                statusTimer.Stop();
                statusServer = false;
                server.StopDiscovery();
                server.StopServer();
            }
        }
    }
}


