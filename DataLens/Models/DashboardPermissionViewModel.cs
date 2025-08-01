using System.ComponentModel.DataAnnotations;

namespace DataLens.Models
{
    public class DashboardPermissionViewModel
    {
        public int DashboardId { get; set; }
        
        [Display(Name = "Dashboard Adı")]
        public string DashboardName { get; set; } = string.Empty;
        
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;
        
        [Display(Name = "Kategori")]
        public string Category { get; set; } = string.Empty;
        
        [Display(Name = "Oluşturan")]
        public string CreatedBy { get; set; } = string.Empty;
        
        [Display(Name = "Oluşturma Tarihi")]
        public DateTime CreatedDate { get; set; }
        
        [Display(Name = "Genel Erişim")]
        public bool IsPublic { get; set; }
        
        // Kullanıcı İzinleri
        public List<UserPermissionItem> UserPermissions { get; set; } = new List<UserPermissionItem>();
        
        // Grup İzinleri
        public List<GroupPermissionItem> GroupPermissions { get; set; } = new List<GroupPermissionItem>();
        
        // Yeni izin ekleme için
        [Display(Name = "Kullanıcı Seç")]
        public string SelectedUserId { get; set; } = string.Empty;
        
        [Display(Name = "Grup Seç")]
        public string SelectedGroupId { get; set; } = string.Empty;
        
        [Display(Name = "İzin Türü")]
        public string PermissionType { get; set; } = string.Empty;
        
        // Dropdown listeleri için
        public List<UserItem> AvailableUsers { get; set; } = new List<UserItem>();
        public List<GroupItem> AvailableGroups { get; set; } = new List<GroupItem>();
    }
    
    public class UserPermissionItem
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PermissionType { get; set; } = string.Empty;
        public DateTime GrantedDate { get; set; }
        public string GrantedBy { get; set; } = string.Empty;
    }
    
    public class GroupPermissionItem
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string PermissionType { get; set; } = string.Empty;
        public DateTime GrantedDate { get; set; }
        public string GrantedBy { get; set; } = string.Empty;
    }
    
    public class UserItem
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
    
    public class GroupItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}