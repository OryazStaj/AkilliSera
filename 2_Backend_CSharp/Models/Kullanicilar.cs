using System;
using System.Collections.Generic;

namespace AkilliSera_API.Models;

public partial class Kullanicilar
{
    public int KullaniciId { get; set; }

    public string? Isim { get; set; }

    public string? Soyisim { get; set; }

    public string? Eposta { get; set; }

    public string? Sifre { get; set; }
}
