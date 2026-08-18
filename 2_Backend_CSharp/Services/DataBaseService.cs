using AkilliSera_API.Data;
using AkilliSera_API.Models;


namespace AkilliSera_API.Services
{
    public class DataBaseService
    {
        private readonly AkilliSeraDbContext _context;
        private readonly ILogger<DataBaseService> _logger; // Loglama için
        public DataBaseService(AkilliSeraDbContext context, ILogger<DataBaseService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public bool Verikaydet(SensorLoglari yeniVeri)
        {
            try
            {
                _context.SensorLoglaris.Add(yeniVeri);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sensör verisi kaydedilirken hata oluştu.");
                return false; // BadRequest dönecek
            }
        }
        public List<SensorLoglari> SensorGecmisi()
        {
            // Listeleme işlemlerinde en yeni kayıtlar önce dönecek şekilde sıralama 
            return _context.SensorLoglaris.OrderByDescending(s => s.KayitZamani).ToList();
        }
        public bool IlaclamaEkle(IlaclamaTakip yeniIlaclama)
        {
            try
            {
                _context.IlaclamaTakips.Add(yeniIlaclama);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İlaçlama kaydı eklenirken hata oluştu.");
                return false;
            }
        }
        public List<IlaclamaTakip> IlaclamaGecmisi()
        {
            return _context.IlaclamaTakips.ToList();
        }
        public List<BitkiHastalik> BitkiHastaliklariniGetir()
        {
            return _context.BitkiHastaliks.ToList(); // DbSet ismin projene göre değişebilir
        }
        public List<BitkiEvreleri> BitkiEvreleriniGetir()
        {
            return _context.BitkiEvreleris.ToList();
        }
        public List<Kullanicilar> KullanicilariGetir()
        {
            return _context.Kullanicilars.ToList();
        }
        public List<Bildirim> BildirimleriGetir()
        {
            return _context.Bildirims.ToList();
        }
        public bool KullaniciKaydet(Kullanicilar yeniKullanici)
        {
            try
            {
                // İsteğe bağlı: Aynı e-posta ile daha önce kayıt olunmuş mu kontrolü
                var varMi = _context.Kullanicilars.Any(k => k.Eposta == yeniKullanici.Eposta);
                if (varMi) return false;

                _context.Kullanicilars.Add(yeniKullanici);
                _context.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt sırasında hata oluştu.");
                return false;
            }
        }

       
        public Kullanicilar KullaniciDogrula(string eposta, string sifre)
        {
            try
            {
                
                var kullanici = _context.Kullanicilars
                    .FirstOrDefault(k => k.Eposta == eposta && k.Sifre == sifre);

                return kullanici; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Giriş yapılırken hata oluştu.");
                return null;
            }
        }
    }
}
