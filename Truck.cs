using System;

namespace Project3
{
    public class Truck : IAuto
    {
        public string BrandName { get; set; } = "Не указано";
        public string ModelName { get; set; } = "Не указано";
        public float Power { get; set; } = 0;
        public float MaxSpeed { get; set; } = 0;
        public string Type { get; set; } = "Грузовая";
        public string RegNumber { get; set; }
        public int Wheel { get; set; } = 4;
        public float BodyVolume { get; set; } = 0;
        public int Port { get; set; }

    }
}
