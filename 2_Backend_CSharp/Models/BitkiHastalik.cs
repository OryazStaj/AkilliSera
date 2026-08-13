using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class BitkiHastalik
{
    public int HastalikId { get; set; }

    public int? BitkiId { get; set; }

    public string? HastalikAdi { get; set; }

    public decimal? HastalikOrani { get; set; }

    public string? FotografYolu { get; set; }

    public virtual SeraDurum? Bitki { get; set; }

    public virtual ICollection<IlaclamaTakip> IlaclamaTakips { get; set; } = new List<IlaclamaTakip>();
}
