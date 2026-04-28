using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project3
{
    public class Server
    {
        public bool isStatusServer = false;
        public bool isStatusDiscovery = true;

        private TcpListener tcpListener;
        public int port;

        public Dictionary<string, List<Car>> sharedCars = new Dictionary<string, List<Car>>();
        public Dictionary<string, List<Truck>> sharedTrucks = new Dictionary<string, List<Truck>>();

        public static int GetFreePort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }

        public async Task startServer()
        {
            port = GetFreePort();
            tcpListener = new TcpListener(IPAddress.Any, port);
            try
            {
                tcpListener.Start();
                isStatusServer = true;
                Console.WriteLine($"Сервер запущен на {port}. Ожидание подключений... ");

                while (isStatusServer)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                    Console.WriteLine("Клиент подключен.");

                    _ = Task.Run(async () => await ProcessClientAsync(tcpClient));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сервера: {ex.Message}");
            }
            finally
            {
                tcpListener?.Stop();
                isStatusServer = false;
                Console.WriteLine("Сервер остановлен.");
            }
        }

        public async Task startDiscovery()
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 9999));

                while (isStatusDiscovery)
                {

                    try
                    {
                        var result = await udpClient.ReceiveAsync();
                        string msg = Encoding.UTF8.GetString(result.Buffer);
                        if (msg == "DISCOVER_SERVER")
                        {
                            Console.WriteLine($"Discovery от {result.RemoteEndPoint}");
                            string response = $"SERVER_PORT:{port}";
                            byte[] bytes = Encoding.UTF8.GetBytes(response);
                            await udpClient.SendAsync(bytes, bytes.Length, result.RemoteEndPoint);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UDP error: {ex.Message}");
                    }

                }
            }
        }

        public void StopServer()
        {
            isStatusServer = false;
            tcpListener?.Stop();
        }

        public void StopDiscovery()
        {
            isStatusDiscovery = false;
        }

        private async Task ProcessClientAsync(TcpClient tcpClient)
        {
            try
            {
                using (tcpClient)
                {
                    var stream = tcpClient.GetStream();
                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.AutoFlush = true;

                        string request = await reader.ReadLineAsync();

                        if (request.StartsWith("ACCEPT_DATA:")) // Отправляем данные
                        {
                            string response = null;
                            var details = request.Substring(12).Split(':');

                            string brandName = details[0];
                            string type = details[1];
                            int index = int.Parse(details[2]);
                            int count = int.Parse(details[3]);

                            if (type == "Легковая" && sharedCars.ContainsKey(brandName))
                            {
                                var allCars = sharedCars[brandName]
                                    .OrderBy(c => c.RegNumber)
                                    .ToList();

                                List<Car> carsForClient = allCars
                                     .Where((_, i) => i % count == index)
                                     .ToList();

                                response = string.Join(";", carsForClient.Select(a =>
                                    $"{a.BrandName}|{a.ModelName}|{a.Power}|{a.MaxSpeed}|{a.Type}|{a.RegNumber}|" +
                                    $"{a.Multimedia}|{a.Airbags}"
                                ));
                            }
                            else if (type == "Грузовая" && sharedTrucks.ContainsKey(brandName))
                            {
                                var allTrucks = sharedTrucks[brandName]
                                    .OrderBy(t => t.RegNumber)
                                    .ToList();

                                List<Truck> trucksForClient = allTrucks
                                     .Where((_, i) => i % count == index)
                                     .ToList();

                                response = string.Join(";", trucksForClient.Select(a =>
                                    $"{a.BrandName}|{a.ModelName}|{a.Power}|{a.MaxSpeed}|{a.Type}|{a.RegNumber}|" +
                                    $"{a.Wheel}|{a.BodyVolume}"
                                ));
                            }


                            if (response != null)
                            {
                                Console.WriteLine($"Отправлен ответ: {response}");
                                await writer.WriteLineAsync(response);
                                return;
                            }

                            else
                            {
                                await writer.WriteLineAsync("Brand not found");
                                return;
                            }
                        }
                        else if (request.StartsWith("SEND_DATA:")) // Получаем данные
                        {
                            foreach (var cars in request.Substring(10).Split(';'))
                            {
                                if (string.IsNullOrWhiteSpace(cars)) continue;

                                var details = cars.Split('|');

                                if (details[4] == "Легковая")
                                {
                                    var car = new Car
                                    {
                                        BrandName = details[0],
                                        ModelName = details[1],
                                        Power = int.Parse(details[2]),
                                        MaxSpeed = int.Parse(details[3]),
                                        Type = details[4],
                                        RegNumber = details[5],
                                        Multimedia = details[6],
                                        Airbags = int.Parse(details[7])
                                    };
                                    AddDictionary(car);
                                }
                                else
                                {
                                    var truck = new Truck
                                    {
                                        BrandName = details[0],
                                        ModelName = details[1],
                                        Power = int.Parse(details[2]),
                                        MaxSpeed = int.Parse(details[3]),
                                        Type = details[4],
                                        RegNumber = details[5],
                                        Wheel = int.Parse(details[6]),
                                        BodyVolume = int.Parse(details[7])
                                    };

                                    AddDictionary(truck);
                                }
                            }
                        }
                        else if (request.StartsWith("CHECK_DATA:"))
                        {
                            var parts = request.Split(':');
                            string checkBrand = parts[1];
                            string checkType = parts[2];

                            bool hasData = false;
                            if (checkType == "Легковая")
                            {
                                hasData = sharedCars.ContainsKey(checkBrand);
                            }
                            else if (checkType == "Грузовая")
                            {
                                hasData = sharedTrucks.ContainsKey(checkBrand);
                            }

                            writer.WriteLine(hasData ? "HAS_DATA" : "NO_DATA");
                            return;
                        }
                        else
                        {
                            await writer.WriteLineAsync("Request not found");
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка обработки клиента: {ex.Message}");
            }
        }

        public void AddDictionary(IAuto auto)
        {
            if (string.IsNullOrEmpty(auto.BrandName) || string.IsNullOrWhiteSpace(auto.BrandName)) return;
            if (auto is Car car)
            {
                if (string.IsNullOrEmpty(car.RegNumber) || string.IsNullOrWhiteSpace(car.RegNumber)) return;
                if (!sharedCars.ContainsKey(auto.BrandName))
                {
                    sharedCars[auto.BrandName] = new List<Car>();
                }
                if (!sharedCars[auto.BrandName].Any(c => c.RegNumber == car.RegNumber))
                {
                    sharedCars[auto.BrandName].Add(car);
                }
            }
            else if (auto is Truck truck)
            {
                if (string.IsNullOrEmpty(truck.RegNumber) || string.IsNullOrWhiteSpace(truck.RegNumber)) return;
                if (!sharedTrucks.ContainsKey(auto.BrandName))
                {
                    sharedTrucks[auto.BrandName] = new List<Truck>();
                }
                if (!sharedTrucks[auto.BrandName].Any(c => c.RegNumber == truck.RegNumber))
                {
                    sharedTrucks[auto.BrandName].Add(truck);
                }
            }
        }
    }
}
