using AkilliSera_API.Data;
using AkilliSera_API.Models;


namespace AkilliSera_API.Services
{
    public class DataBaseService
    {
        private readonly AkilliSeraDbContext _context;
        public DataBaseService(AkilliSeraDbContext context)
        {
            _context = context;
        }
        public void Verikaydet(SensorLoglari yeniVeri)
        {
            try
            {
                _context.SensorLoglaris.Add(yeniVeri);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Sensör verisi kaydedilirken hata oluştu: " + ex.Message);
            }
        }
        public List<SensorLoglari> SensorGecmisi()
        {
            return _context.SensorLoglaris.ToList();
        }
        public void IlaclamaEkle(IlaclamaTakip yeniIlaclama)
        {
            try
            {
                _context.IlaclamaTakips.Add(yeniIlaclama);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine("İlaçlama kaydı eklenirken hata oluştu: " + ex.Message);
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
                Console.WriteLine("Kayıt sırasında hata oluştu: " + ex.Message);
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
                Console.WriteLine("Giriş yapılırken hata oluştu: " + ex.Message);
                return null;
            }
        }
    }
}
