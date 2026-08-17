using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class Bildirim
{
    public int BildirimId { get; set; }

    public string? Mesaj { get; set; }

    public bool? OkunduBilgisi { get; set; }

    public DateTime? BildirimZamani { get; set; }
}
