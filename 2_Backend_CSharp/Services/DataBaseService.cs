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
    }
}
