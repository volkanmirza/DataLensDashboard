using System.ComponentModel.DataAnnotations;

namespace DataLens.Models
{
    public class NotificationViewModel
    {
        public string Id { get; set; } = string.Empty;
        
        [Display(Name = "Başlık")]
        public string Title { get; set; } = string.Empty;
        
        [Display(Name = "Mesaj")]
        public string Message { get; set; } = string.Empty;
        
        [Display(Name = "Tür")]
        public string Type { get; set; } = string.Empty;
        
        [Display(Name = "Okundu mu?")]
        public bool IsRead { get; set; }
        
        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; }
        
        [Display(Name = "Okunma Tarihi")]
        public DateTime? ReadDate { get; set; }
        
        [Display(Name = "İlgili Varlık ID")]
        public string? RelatedEntityId { get; set; }
        
        [Display(Name = "İlgili Varlık Türü")]
        public string? RelatedEntityType { get; set; }
        
        [Display(Name = "Oluşturan")]
        public string CreatedBy { get; set; } = string.Empty;
        
        public bool IsActive { get; set; } = true;
        
        // Helper properties for display
        public string TypeDisplayName => Type switch
        {
            "dashboard" => "Dashboard",
            "system" => "Sistem",
            "permission" => "İzin",
            "security" => "Güvenlik",
            _ => "Diğer"
        };
        
        public string TypeBadgeClass => Type switch
        {
            "dashboard" => "badge-primary",
            "system" => "badge-info",
            "permission" => "badge-warning",
            "security" => "badge-danger",
            _ => "badge-secondary"
        };
        
        public string FormattedCreatedDate => CreatedDate.ToString("dd.MM.yyyy HH:mm");
        
        public string FormattedReadDate => ReadDate?.ToString("dd.MM.yyyy HH:mm") ?? "Okunmadı";
    }
}