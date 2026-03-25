using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;

namespace DiagnosticTool
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(config.GetConnectionString("DefaultConnection"));

            using (var db = new AppDbContext(optionsBuilder.Options))
            {
                Console.WriteLine("--- Database Diagnostics ---");
                
                var malzCount = db.Malzemeler.Count();
                Console.WriteLine($"Total Malzemeler: {malzCount}");
                
                var urunCount = db.Urunler.Count();
                Console.WriteLine($"Total Urunler: {urunCount}");
                
                var poliSablon = db.FormSablonlar.FirstOrDefault(s => s.Baslik.Contains("Polielektrolit"));
                if (poliSablon != null)
                {
                    Console.WriteLine($"Polielektrolit Sablon ID: {poliSablon.Id}, Kategori ID: {poliSablon.KategoriId}");
                    
                    var relatedMalz = db.Malzemeler.Where(m => m.KategoriId == poliSablon.KategoriId).ToList();
                    Console.WriteLine($"Malzemeler with Category {poliSablon.KategoriId}: {relatedMalz.Count}");
                    
                    if (relatedMalz.Count > 0)
                    {
                        Console.WriteLine("Sample Malzemeler (Top 5):");
                        foreach (var m in relatedMalz.Take(5))
                        {
                            Console.WriteLine($"- ID: {m.Id}, Ad: '{m.Ad}', OlusturmaTarihi: {m.OlusturmaTarihi}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Polielektrolit Sablon not found.");
                }
                
                Console.WriteLine("--- End Diagnostics ---");
            }
        }
    }
}
