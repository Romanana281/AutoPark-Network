using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project3
{
    public class Loader
    {
        private static Dictionary<string, List<Car>> loadCar = new Dictionary<string, List<Car>>();
        private static Dictionary<string, List<Truck>> loadTruck = new Dictionary<string, List<Truck>>();

        private static Random rand = new Random();
        private static int currentLoad = 0;
        private static int currentServerLoad = 0;
        private static int countCar = 0, countTruck = 0;

        private static string GenerateRegNumber()
        {
            string letters = "АВЕКМНОРСТУХ";
            string numbers = "0123456789";

            char letter1 = letters[rand.Next(letters.Length)];
            char number1 = numbers[rand.Next(numbers.Length)];
            char number2 = numbers[rand.Next(numbers.Length)];
            char number3 = numbers[rand.Next(numbers.Length)];
            char letter2 = letters[rand.Next(letters.Length)];
            char letter3 = letters[rand.Next(letters.Length)];

            return $"{letter1}{number1}{number2}{number3}{letter2}{letter3}";
        }

        public static List<IAuto> load(string brandName, IAuto prototype = null, Action<IAuto> onAutoLoaded = null)
        {
            string autoType = prototype?.Type;

            if (string.IsNullOrEmpty(autoType))
            {
                return LoadBothTypes(brandName, prototype, onAutoLoaded);
            }

            switch (autoType)
            {
                case "Легковая":
                    return LoadCars(brandName, prototype, onAutoLoaded);
                case "Грузовая":
                    return LoadTrucks(brandName, prototype, onAutoLoaded);
                default:
                    return LoadBothTypes(brandName, prototype, onAutoLoaded);
            }

        }

        public static List<IAuto> LoadFromServer(string brandName, string type, Client client, Action<IAuto> onAutoLoaded = null)
        {
            List<IAuto> autos = new List<IAuto>();
            currentServerLoad = 0;

            client.DiscoverServersAsync();
            try
            {
                List<IAuto> serverData = new List<IAuto>();
                var validServers = new List<int>();

                var checkServerTasks = client.discoveredServers.Select(async port =>
                {
                    if (await client.HasRequiredData(port, brandName, type))
                    {
                        return (port, true);
                    }
                    return (port, false);
                }).ToList();

                var checkResults = Task.WhenAll(checkServerTasks).Result;
                validServers.AddRange(checkResults.Where(x => x.Item2).Select(x => x.port));

                var loadTasks = validServers.Select(async (port, index) =>
                {

                    var dataFromServer = await client.GetData(brandName, type, port, index, validServers.Count);

                    foreach (var auto in dataFromServer)
                    {
                        auto.Port = port;
                    }

                    return dataFromServer;
                }).ToList();

                var loadResults = Task.WhenAll(loadTasks).Result;
                serverData.AddRange(loadResults.SelectMany(x => x));

                if (serverData != null && serverData.Any())
                {
                    int totalCount = serverData.Count();
                    int processed = 0;

                    var existingCars = type == "Легковая" && loadCar.ContainsKey(brandName)
                        ? loadCar[brandName].ToList()
                        : new List<Car>();

                    var existingTrucks = type == "Грузовая" && loadTruck.ContainsKey(brandName)
                        ? loadTruck[brandName].ToList()
                        : new List<Truck>();

                    var currentSessionCars = new List<Car>();
                    var currentSessionTrucks = new List<Truck>();

                    var newAutos = serverData.Where(serverAuto =>
                    {
                        if (serverAuto is Car serverCar)
                        {
                            bool isExistingDuplicate = existingCars.Any(existingCar =>
                                existingCar.RegNumber == serverCar.RegNumber &&
                                existingCar.BrandName == serverCar.BrandName &&
                                existingCar.ModelName == serverCar.ModelName
                            );

                            bool isCurrentSessionDuplicate = currentSessionCars.Any(sessionCar =>
                                sessionCar.RegNumber == serverCar.RegNumber &&
                                sessionCar.BrandName == serverCar.BrandName &&
                                sessionCar.ModelName == serverCar.ModelName
                            );

                            if (!isCurrentSessionDuplicate)
                            {
                                currentSessionCars.Add(serverCar);
                            }

                            return !isExistingDuplicate && !isCurrentSessionDuplicate;
                        }
                        else if (serverAuto is Truck serverTruck)
                        {
                            bool isExistingDuplicate = existingTrucks.Any(existingTruck =>
                                existingTruck.RegNumber == serverTruck.RegNumber &&
                                existingTruck.BrandName == serverTruck.BrandName &&
                                existingTruck.ModelName == serverTruck.ModelName
                            );

                            bool isCurrentSessionDuplicate = currentSessionTrucks.Any(sessionTruck =>
                                sessionTruck.RegNumber == serverTruck.RegNumber &&
                                sessionTruck.BrandName == serverTruck.BrandName &&
                                sessionTruck.ModelName == serverTruck.ModelName
                            );

                            if (!isCurrentSessionDuplicate)
                            {
                                currentSessionTrucks.Add(serverTruck);
                            }

                            return !isExistingDuplicate && !isCurrentSessionDuplicate;
                        }
                        return true;
                    }).ToList();

                    if (!newAutos.Any())
                    {
                        currentServerLoad = 100;
                        return type == "Легковая" ? existingCars.Cast<IAuto>().ToList()
                               : existingTrucks.Cast<IAuto>().ToList();
                    }

                    foreach (var auto in newAutos)
                    {
                        if (auto != null)
                        {
                            Thread.Sleep(rand.Next(0, 501));

                            autos.Add(auto);

                            if (auto is Car car)
                            {
                                if (!loadCar.ContainsKey(brandName))
                                    loadCar[brandName] = new List<Car>();
                                loadCar[brandName].Add(car);
                            }
                            else if (auto is Truck truck)
                            {
                                if (!loadTruck.ContainsKey(brandName))
                                    loadTruck[brandName] = new List<Truck>();
                                loadTruck[brandName].Add(truck);
                            }

                            processed++;
                            currentServerLoad = (int)((double)processed / newAutos.Count * 100);

                            onAutoLoaded?.Invoke(auto);
                        }
                    }

                    if (type == "Легковая")
                        return existingCars.Concat(autos.OfType<Car>()).Cast<IAuto>().ToList();
                    else
                        return existingTrucks.Concat(autos.OfType<Truck>()).Cast<IAuto>().ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки с сервера: {ex.Message}");
            }

            currentServerLoad = 100;
            return autos;
        }

        private static List<IAuto> LoadCars(string brandName, IAuto prototype, Action<IAuto> onAutoLoaded)
        {
            if (loadCar.ContainsKey(brandName))
            {
                currentLoad = 100;
                return new List<IAuto>(loadCar[brandName]);
            }

            List<IAuto> cars = new List<IAuto>();
            currentLoad = 0;
            int count = rand.Next(5, 11);

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(rand.Next(0, 501));

                countCar++;
                Car newCar = new Car
                {
                    BrandName = brandName,
                    ModelName = prototype?.ModelName ?? "Модель_" + i,
                    Power = prototype?.Power ?? 100,
                    MaxSpeed = prototype?.MaxSpeed ?? 150,
                    Type = "Легковая",
                    RegNumber = GenerateRegNumber(),
                    Multimedia = $"Multimedia_{rand.Next(1, 6)}",
                    Airbags = rand.Next(2, 9)
                };

                cars.Add(newCar);
                currentLoad = (int)((double)(i + 1) / count * 100);
                onAutoLoaded?.Invoke(newCar);
            }

            loadCar[brandName] = cars.ConvertAll(a => (Car)a);
            ShowNotification();
            ResetCounters();

            return cars;
        }

        private static List<IAuto> LoadTrucks(string brandName, IAuto prototype, Action<IAuto> onAutoLoaded)
        {
            if (loadTruck.ContainsKey(brandName))
            {
                currentLoad = 100;
                return new List<IAuto>(loadTruck[brandName]);
            }

            List<IAuto> trucks = new List<IAuto>();
            currentLoad = 0;
            int count = rand.Next(5, 11);

            for (int i = 0; i < count; i++)
            {
                Thread.Sleep(rand.Next(0, 501));

                countTruck++;
                Truck newTruck = new Truck
                {
                    BrandName = brandName,
                    ModelName = prototype?.ModelName ?? "Модель_" + i,
                    Power = prototype?.Power ?? 150,
                    MaxSpeed = prototype?.MaxSpeed ?? 120,
                    Type = "Грузовая",
                    RegNumber = GenerateRegNumber(),
                    Wheel = rand.Next(4, 13),
                    BodyVolume = rand.Next(10, 51)
                };

                trucks.Add(newTruck);
                currentLoad = (int)((double)(i + 1) / count * 100);
                onAutoLoaded?.Invoke(newTruck);
            }

            loadTruck[brandName] = trucks.ConvertAll(a => (Truck)a);
            ShowNotification();
            ResetCounters();

            return trucks;
        }

        private static List<IAuto> LoadBothTypes(string brandName, IAuto prototype, Action<IAuto> onAutoLoaded)
        {
            List<IAuto> allAutos = new List<IAuto>();

            var cars = LoadCars(brandName, prototype, onAutoLoaded);
            allAutos.AddRange(cars);

            var trucks = LoadTrucks(brandName, prototype, onAutoLoaded);
            allAutos.AddRange(trucks);

            return allAutos;
        }

        private static void ShowNotification()
        {
            NotifyIcon notifyIcon = new NotifyIcon();
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(3000, "Уведомление", $"Было загружено: Легковых {countCar}, Грузовых {countTruck}", ToolTipIcon.Info);
        }

        private static void ResetCounters()
        {
            countCar = 0;
            countTruck = 0;
        }

        public static int getProgress()
        {
            return currentLoad;
        }

        public static int getServerProgress()
        {
            return currentServerLoad;
        }

        public static void ClearCache()
        {
            loadCar.Clear();
            loadTruck.Clear();
        }

    }
}