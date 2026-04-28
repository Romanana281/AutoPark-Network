using System;

namespace Project3
{
    public class Car : IAuto
    {
        public string BrandName { get; set; } = "Не указано";
        public string ModelName { get; set; } = "Не указано";
        public float Power { get; set; } = 0;
        public float MaxSpeed { get; set; } = 0;
        public string Type { get; set; } = "Легковая";
        public string RegNumber { get; set; }
        public string Multimedia { get; set; } = "";
        public int Airbags { get; set; } = 2;
        public int Port { get; set; }
    }
}
