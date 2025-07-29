using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLens.Models
{
    public class Dashboard
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string DashboardData { get; set; } = string.Empty; // JSON data for DevExpress Dashboard

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastModifiedDate { get; set; }

        public DateTime? LastModifiedAt { get; set; }

        public string? LastModifiedBy { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPublic { get; set; } = false;

        [StringLength(50)]
        public string Category { get; set; } = "General";

        public List<string> Tags { get; set; } = new List<string>();

        public int ViewCount { get; set; } = 0;
    }
}