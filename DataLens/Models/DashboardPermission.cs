using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLens.Models
{
    public class DashboardPermission
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        public string DashboardId { get; set; } = string.Empty;

        public string? UserId { get; set; } // Null if permission is for a group

        public string? GroupId { get; set; } // Null if permission is for a user

        [Required]
        [StringLength(20)]
        public string PermissionType { get; set; } = string.Empty; // View, Edit, Delete, Share

        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        public string GrantedBy { get; set; } = string.Empty;

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public static class PermissionTypes
    {
        public const string View = "View";
        public const string Edit = "Edit";
        public const string Delete = "Delete";
        public const string Share = "Share";
        public const string FullControl = "FullControl";
    }

    public static class UserRoles
    {
        public const string Viewer = "Viewer";
        public const string Designer = "Designer";
        public const string Admin = "Admin";
    }
}