using System;

namespace Project3
{
    public interface IAuto
    {
        string BrandName { get; set; }
        string ModelName { get; set; }
        float Power { get; set; }
        float MaxSpeed { get; set; }
        string Type { get; set; }
        int Port { get; set; }
    }

}
