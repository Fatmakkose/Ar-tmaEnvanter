using System.Net.Http.Json;
using Newtonsoft.Json;
using AritmaEnvanter.Mobile.Models;

namespace AritmaEnvanter.Mobile.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = "https://knglx2sj-5235.euw.devtunnels.ms/api/MobileApi/";

        public ApiService()
        {
            _httpClient = new HttpClient();
            // Dev Tunnel "Mavi Ekran"ı geçmek için gerekli header
            _httpClient.DefaultRequestHeaders.Add("X-Free-Tunnel-Bypass", "true");
        }

        public async Task<(bool Success, string Message, string? FullName)> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}login", new { Email = email, Password = password });
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(content);

                if (response.IsSuccessStatusCode && result != null && (bool?)result.success == true)
                {
                    return (true, (string?)result.message ?? "Giriş başarılı.", (string?)result.fullName);
                }
                return (false, (string?)result?.message ?? "Giriş başarısız.", null);
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}", null);
            }
        }

        public async Task<List<LocalStockItem>> GetStocksAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}stocks");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<LocalStockItem>>(content) ?? new List<LocalStockItem>();
                }
            }
            catch (Exception) { }
            return new List<LocalStockItem>();
        }

        public async Task<MobileDashboardSummary?> GetSummaryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}summary");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<MobileDashboardSummary>(content);
                }
            }
            catch (Exception) { }
            return null;
        }

        public async Task<List<MovementItem>> GetMovementsAsync(string type = "", string start = "", string end = "")
        {
            try
            {
                var url = $"{_baseUrl}movements?type={type}&startDate={start}&endDate={end}";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<MovementItem>>(content) ?? new List<MovementItem>();
                }
            }
            catch (Exception) { }
            return new List<MovementItem>();
        }

        public async Task<dynamic?> GetMetadataAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}metadata");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<dynamic>(content);
                }
            }
            catch (Exception) { }
            return null;
        }

        public async Task<dynamic?> GetMaterialDetailsAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}material-details/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<dynamic>(content);
                }
            }
            catch (Exception) { }
            return null;
        }

        public async Task<List<MetadataItem>> GetShelvesAsync(int warehouseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}shelves/{warehouseId}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<List<MetadataItem>>(content) ?? new List<MetadataItem>();
                }
            }
            catch (Exception) { }
            return new List<MetadataItem>();
        }

        public async Task<(bool Success, string Message)> PerformOperationAsync(object request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}perform-operation", request);
                var content = await response.Content.ReadAsStringAsync();
                
                // Eğer JSON değilse düz metin olarak döndür
                if (!content.Trim().StartsWith("{"))
                {
                    return (response.IsSuccessStatusCode, content);
                }

                var result = JsonConvert.DeserializeObject<dynamic>(content);
                return (response.IsSuccessStatusCode, (string?)result?.message ?? (response.IsSuccessStatusCode ? "İşlem başarılı" : "İşlem başarısız"));
            }
            catch (Exception ex)
            {
                return (false, $"Bağlantı hatası: {ex.Message}");
            }
        }
    }
}
