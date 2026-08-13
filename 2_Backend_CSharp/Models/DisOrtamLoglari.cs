using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class DisOrtamLoglari
{
    public int LogId { get; set; }

    public int? SeraId { get; set; }

    public string? HavaDurumu { get; set; }

    public decimal? DisOrtamSicakligi { get; set; }

    public decimal? DisOrtamNemi { get; set; }

    public DateTime? KayitZamani { get; set; }

    public virtual SeraDurum? Sera { get; set; }
}
