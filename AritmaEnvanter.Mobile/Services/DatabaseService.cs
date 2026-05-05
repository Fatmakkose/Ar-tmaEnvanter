using SQLite;
using AritmaEnvanter.Mobile.Models;

namespace AritmaEnvanter.Mobile.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection? _database;

        public DatabaseService()
        {
        }

        async Task Init()
        {
            if (_database is not null)
                return;

            var databasePath = Path.Combine(FileSystem.AppDataDirectory, "AritmaEnvanter.db3");

            _database = new SQLiteAsyncConnection(databasePath);

            await _database.CreateTableAsync<LocalStockItem>();
        }

        public async Task<List<LocalStockItem>> GetStocksAsync()
        {
            await Init();
            return await _database!.Table<LocalStockItem>().ToListAsync();
        }

        public async Task SaveStocksAsync(List<LocalStockItem> items)
        {
            await Init();
            await _database!.RunInTransactionAsync((SQLiteConnection conn) =>
            {
                conn.DeleteAll<LocalStockItem>();
                foreach (var item in items)
                {
                    conn.Insert(item);
                }
            });
        }

        public async Task ClearAllDataAsync()
        {
            await Init();
            await _database!.DeleteAllAsync<LocalStockItem>();
        }
    }
}
