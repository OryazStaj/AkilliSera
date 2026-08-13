using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class SeraDurum
{
    public int SeraId { get; set; }

    public string? SeraAdi { get; set; }

    public int? AktifEvreId { get; set; }

    public DateTime? SonGuncellemeZamani { get; set; }

    public decimal? KoordinatEnlem { get; set; }

    public decimal? KoordinatBoylam { get; set; }

    public virtual ICollection<AksiyonLoglari> AksiyonLoglaris { get; set; } = new List<AksiyonLoglari>();

    public virtual BitkiEvreleri? AktifEvre { get; set; }

    public virtual ICollection<BitkiHastalik> BitkiHastaliks { get; set; } = new List<BitkiHastalik>();

    public virtual ICollection<DisOrtamLoglari> DisOrtamLoglaris { get; set; } = new List<DisOrtamLoglari>();

    public virtual ICollection<KameraLoglari> KameraLoglaris { get; set; } = new List<KameraLoglari>();

    public virtual ICollection<SensorLoglari> SensorLoglaris { get; set; } = new List<SensorLoglari>();
}
