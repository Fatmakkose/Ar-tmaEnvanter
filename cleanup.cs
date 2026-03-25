using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ArıtmaEnvanter.Data;
using ArıtmaEnvanter.Models.Entities;

namespace CleanupTool
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
                Console.WriteLine("--- Database Cleanup ---");
                
                // Identify Malzemeler with no UrunTuru and no Category
                var candidates = db.Malzemeler
                    .Where(m => string.IsNullOrEmpty(m.UrunTuru) && m.KategoriId == null)
                    .ToList();
                
                Console.WriteLine($"Found {candidates.Count} candidate Malzemeler for deletion.");
                
                int deletedCount = 0;
                foreach (var m in candidates)
                {
                    // Check if they have any stock or assignments
                    var hasStock = db.DepoStoklar.Any(s => s.MalzemeId == m.Id && s.Miktar > 0);
                    var hasZimmet = db.Zimmetler.Any(z => z.MalzemeId == m.Id);
                    
                    if (!hasStock && !hasZimmet)
                    {
                        db.Malzemeler.Remove(m);
                        deletedCount++;
                    }
                }
                
                if (deletedCount > 0)
                {
                    db.SaveChanges();
                    Console.WriteLine($"Successfully deleted {deletedCount} redundant Malzeme records.");
                }
                else
                {
                    Console.WriteLine("No redundant records found to delete.");
                }
                
                Console.WriteLine("--- End Cleanup ---");
            }
        }
    }
}
