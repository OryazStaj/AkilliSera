using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class IlaclamaTakip
{
    public int IlaclamaId { get; set; }

    public int? HastalikId { get; set; }

    public string? IlacAdi { get; set; }

    public DateTime? UygulamaZamani { get; set; }

    public virtual BitkiHastalik? Hastalik { get; set; }
}
