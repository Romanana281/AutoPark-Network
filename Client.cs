using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;
using System.Net;
namespace Project3
{
    public class Client
    {
        public List<int> discoveredServers = new List<int>();

        public void DiscoverServersAsync(int timeoutMs = 150)
        {
            discoveredServers.Clear();

            using (UdpClient udp = new UdpClient())
            {
                udp.EnableBroadcast = true;
                udp.Client.ReceiveTimeout = timeoutMs;

                byte[] msg = System.Text.Encoding.UTF8.GetBytes("DISCOVER_SERVER");
                udp.Send(msg, msg.Length, new IPEndPoint(IPAddress.Broadcast, 9999));

                var startTime = DateTime.Now;

                while ((DateTime.Now - startTime).TotalMilliseconds < timeoutMs)
                {
                    try
                    {
                        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = udp.Receive(ref remoteEP);
                        string response = System.Text.Encoding.UTF8.GetString(data);

                        if (response.StartsWith("SERVER_PORT:"))
                        {
                            int port = int.Parse(response.Split(':')[1]);
                            if (!discoveredServers.Contains(port))
                                discoveredServers.Add(port);
                        }
                    }
                    catch (SocketException ex)
                    {
                        if (ex.SocketErrorCode == SocketError.TimedOut)
                            continue;

                        Console.WriteLine($"Ошибка udp клиента: {ex}");
                        break;
                    }
                }
            }

            Console.WriteLine("Найдены сервера: " + string.Join(", ", discoveredServers));
        }

        public async Task<List<IAuto>> GetData(string brandName, string type, int port, int processIndex, int processCount) // Принимаем данные
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {

                    Console.WriteLine("Подключаемся к серверу...");
                    await tcpClient.ConnectAsync("127.0.0.1", port);
                    var stream = tcpClient.GetStream();
                    using (var reader = new StreamReader(stream)) using (var writer = new StreamWriter(stream))
                    {
                        writer.AutoFlush = true;
                        string request = $"ACCEPT_DATA:{brandName}:{type}:{processIndex}:{processCount}";
                        Console.WriteLine($"Отправляем запрос: {request}");
                        await writer.WriteLineAsync(request);
                        string response = await reader.ReadLineAsync();
                        Console.WriteLine($"Получен ответ от сервера: {response}");
                        if (response == null) return new List<IAuto>();

                        var result = new List<IAuto>();
                        foreach (var cars in response.Split(';'))
                        {
                            if (string.IsNullOrWhiteSpace(cars)) continue;
                            IAuto auto = null;
                            var details = cars.Split('|');
                            if (details[4] == "Легковая")
                            {
                                auto = new Car
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
                            }
                            else
                            {
                                auto = new Truck
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
                            }
                            if (auto != null) result.Add(auto);
                        }
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка клиента: {ex.Message}");
                return new List<IAuto>();
            }
        }

        public async void SendData(List<IAuto> list, int port) // Отправляем данные
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    Console.WriteLine("Подключаемся к серверу...");
                    await tcpClient.ConnectAsync("127.0.0.1", port);
                    var stream = tcpClient.GetStream();
                    using (var reader = new StreamReader(stream)) using (var writer = new StreamWriter(stream))
                    {
                        var dataList = new List<string>();
                        foreach (var auto in list)
                        {
                            if (auto is Car car)
                            {
                                dataList.Add($"{car.BrandName}|{car.ModelName}|{car.Power}|{car.MaxSpeed}|{car.Type}|{car.RegNumber}|{car.Multimedia}|{car.Airbags}");
                            }
                            else if (auto is Truck truck)
                            {
                                dataList.Add($"{truck.BrandName}|{truck.ModelName}|{truck.Power}|{truck.MaxSpeed}|{truck.Type}|{truck.RegNumber}|{truck.Wheel}|{truck.BodyVolume}");
                            }
                        }
                        string data = string.Join(";", dataList);
                        writer.AutoFlush = true;
                        string request = $"SEND_DATA:{data}";
                        Console.WriteLine($"Отправляем запрос: {request}");
                        await writer.WriteLineAsync(request);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка клиента: {ex.Message}");
            }
        }

        public async Task<bool> HasRequiredData(int port, string brandName, string type)
        {
            try
            {
                using (TcpClient tcpClient = new TcpClient())
                {
                    await tcpClient.ConnectAsync("127.0.0.1", port);
                    var stream = tcpClient.GetStream();

                    using (var reader = new StreamReader(stream))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.AutoFlush = true;

                        string request = $"CHECK_DATA:{brandName}:{type}";
                        await writer.WriteLineAsync(request);

                        string response = await reader.ReadLineAsync();

                        return response == "HAS_DATA";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Проверка сервера {port} не удалась: {ex.Message}");
                return false;
            }
        }
    }
}