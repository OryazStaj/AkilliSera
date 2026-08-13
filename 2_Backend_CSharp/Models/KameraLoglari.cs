using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class KameraLoglari
{
    public int LogId { get; set; }

    public int? SeraId { get; set; }

    public string? DurumBilgisi { get; set; }

    public DateTime? KayitZamani { get; set; }

    public virtual SeraDurum? Sera { get; set; }
}
