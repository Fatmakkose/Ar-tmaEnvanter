using System.Collections.Generic;

namespace AritmaEnvanter.Models.DTOs
{
    public class MobileLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class MobileLoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string UserEmail { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

    public class StockItemDto
    {
        public int Id { get; set; }
        public int MalzemeId { get; set; }
        public string MaterialName { get; set; }
        public string WarehouseName { get; set; }
        public string ShelfName { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedDate { get; set; }
        public string MaterialType { get; set; } // Liquid, Solid, Barrel, Part
    }

    public class MobileDashboardSummary
    {
        public int CriticalStockCount { get; set; }
        public int TodayEntryCount { get; set; }
        public int TodayExitCount { get; set; }
    }

    public class StockMovementRequest
    {
        public int StokId { get; set; }
        public decimal Amount { get; set; }
        public string Type { get; set; } // "Giris" or "Cikis"
        public string Description { get; set; }
        public int? PersonelId { get; set; } // Required for "Cikis" if assigned to personnel
        public string? FormNo { get; set; }
    }

    public class MobileTalepRequest
    {
        public string Description { get; set; }
        public List<TalepSatirDto> Items { get; set; }
    }

    public class TalepSatirDto
    {
        public int StokId { get; set; }
        public decimal Quantity { get; set; }
    }

    public class MovementItemDto
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public decimal Amount { get; set; }
        public string Unit { get; set; }
        public string TransactionType { get; set; } // GIR, CIK, TRN
        public string Date { get; set; }
        public string User { get; set; }
        public string WarehouseName { get; set; }
        public string ShelfName { get; set; }
        public string Specification { get; set; }
    }

    public class FormAlanDto
    {
        public int Id { get; set; }
        public string AlanAdi { get; set; }
        public string AlanTipi { get; set; } // TextBox, DropDown, etc.
        public bool Gerekli { get; set; }
        public string Secenekler { get; set; }
    }

    public class MaterialDetailsDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Unit { get; set; }
        public List<FormAlanDto> DynamicFields { get; set; }
    }

    public class MobileStockOperationRequest
    {
        public string OperationType { get; set; } // GIRIS or CIKIS
        public int? MalzemeId { get; set; }
        public int? StokId { get; set; } // Used for exit
        public decimal Amount { get; set; }
        public int? WarehouseId { get; set; }
        public int? ShelfId { get; set; }
        public string? ShelfNo { get; set; }
        public int? CompanyId { get; set; }
        public int? PersonnelId { get; set; }
        public string? ExitType { get; set; } // Sarf, Demirbaş
        public string? Note { get; set; }
        public List<DynamicFieldValue>? DynamicFields { get; set; }
    }

    public class DynamicFieldValue
    {
        public int FieldId { get; set; }
        public string Value { get; set; }
    }

    public class TalepDetailDto
    {
        public int Id { get; set; }
        public int FormNo { get; set; }
        public string RequesterName { get; set; }
        public string Date { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public List<TalepSatirDetailDto> Items { get; set; }
    }

    public class TalepSatirDetailDto
    {
        public int Id { get; set; }
        public string MaterialName { get; set; }
        public string Specification { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
    }
}
