using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class AksiyonLoglari
{
    public int LogId { get; set; }

    public int? SeraId { get; set; }

    public string? CihazAdi { get; set; }

    public string? Aksiyon { get; set; }

    public DateTime? KayitZamani { get; set; }

    public virtual SeraDurum? Sera { get; set; }
}
