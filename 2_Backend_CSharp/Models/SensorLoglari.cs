using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class SensorLoglari
{
    public int LogId { get; set; }

    public int? SeraId { get; set; }

    public decimal? OrtamSicakligi { get; set; }

    public decimal? OrtamNemi { get; set; }

    public decimal? ToprakNemi { get; set; }

    public DateTime? KayitZamani { get; set; }

    public virtual SeraDurum? Sera { get; set; }
}
