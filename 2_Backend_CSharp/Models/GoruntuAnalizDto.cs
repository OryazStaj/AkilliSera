namespace AkilliSera_API.Models;

/// <summary>
/// Python Yapay Zeka modülünden (analiz.py) gelen görüntü analizi sonuç paketi.
/// </summary>
public class GoruntuAnalizDto
{
    public int? SeraId { get; set; }
    public string BitkiEvresi { get; set; } = "Tohum";
    public bool YaprakTespitEdildiMi { get; set; }
    public bool DomatesTespitEdildiMi { get; set; }
    public List<HastalikDetayDto> HastalikDetaylari { get; set; } = new();
    public List<DomatesDetayDto> DomatesDetaylari { get; set; } = new();
    public string? FotografYolu { get; set; }
    public string? AnalizZamani { get; set; }
    public double GuvenEsigi { get; set; }
    public string? Aciklama { get; set; }
}

public class HastalikDetayDto
{
    public string HastalikAdi { get; set; } = string.Empty;
    public double GuvenSkoru { get; set; }
}

public class DomatesDetayDto
{
    public string Durum { get; set; } = string.Empty;
    public double GuvenSkoru { get; set; }
}
