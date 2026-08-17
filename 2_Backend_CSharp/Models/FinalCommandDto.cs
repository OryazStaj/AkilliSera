namespace AkilliSera_API.Models;

public class FinalCommandDto
{
    public double SulamaSuresi { get; set; }
    public string SulamaKararMetni { get; set; } = string.Empty;

    public double FanSeviyesi { get; set; }
    public string FanKararMetni { get; set; } = string.Empty;

    public double IsitmaSeviyesi { get; set; }
    public string IsitmaKararMetni { get; set; } = string.Empty;
}
