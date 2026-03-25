using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ArýtmaEnvanter.Data;
using ArýtmaEnvanter.Models.Entities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ArýtmaEnvanter.Services
{
    public class AssetTransactionService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AssetTransactionService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> TransferAssetAsync(int urunId, int yeniLokasyonId, string? aciklama = null)
        {
            var urun = await _context.Urunler.FindAsync(urunId);
            if (urun == null) return false;

            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            var transaction = new AssetTransaction
            {
                UrunId = urunId,
                EskiLokasyonId = urun.LokasyonId,
                YeniLokasyonId = yeniLokasyonId,
                Aciklama = aciklama,
                IslemYapanKullaniciId = userId,
                IslemTarihi = DateTime.UtcNow
            };

            // Entity Framework Execution Strategy with Transactions for safety
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var dbTransaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _context.AssetTransactions.AddAsync(transaction);

                    urun.LokasyonId = yeniLokasyonId;
                    _context.Urunler.Update(urun);

                    await _context.SaveChangesAsync();
                    await dbTransaction.CommitAsync();
                }
                catch
                {
                    await dbTransaction.RollbackAsync();
                    throw;
                }
            });

            return true;
        }
    }
}
