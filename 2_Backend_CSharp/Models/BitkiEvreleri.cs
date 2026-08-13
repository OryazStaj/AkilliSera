using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class BitkiEvreleri
{
    public int EvreId { get; set; }

    public string? BitkiAdi { get; set; }

    public string? EvreAdi { get; set; }

    public decimal? MinSicaklik { get; set; }

    public decimal? MinToprakNemi { get; set; }

    public decimal? MaxToprakNemi { get; set; }

    public decimal? MinOrtamNemi { get; set; }

    public decimal? MaxOrtamNemi { get; set; }

    public decimal? GunduzMinSicaklik { get; set; }

    public decimal? GunduzMaxSicaklik { get; set; }

    public decimal? GeceMinSicaklik { get; set; }

    public decimal? GeceMaxSicaklik { get; set; }

    public virtual ICollection<SeraDurum> SeraDurums { get; set; } = new List<SeraDurum>();
}
