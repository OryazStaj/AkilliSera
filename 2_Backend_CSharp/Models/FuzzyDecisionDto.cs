namespace AkilliSera_API.Models;

public class FuzzyDecisionDto
{
    public KararlarDto Kararlar { get; set; } = new();
}

public class KararlarDto
{
    public KontrolKarariDto Sulama { get; set; } = new();
    public KontrolKarariDto Havalandirma { get; set; } = new();
    public KontrolKarariDto Isitma { get; set; } = new();
}

public class KontrolKarariDto
{
    public double Sure { get; set; }
    public double Seviye { get; set; }
    public string Karar { get; set; } = string.Empty;
}
