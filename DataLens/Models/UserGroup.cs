using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLens.Models
{
    public class UserGroup
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        [StringLength(100)]
        public string GroupName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string Type { get; set; } = "Standard"; // Standard, Department, Project, Custom

        public string? ParentGroupId { get; set; }

        public int? MaxMembers { get; set; }

        public bool AllowSelfJoin { get; set; } = false;

        [StringLength(200)]
        public string? Tags { get; set; }

        [BsonElement("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        public bool IsActive { get; set; } = true;

        public string CreatedBy { get; set; } = string.Empty;
    }
}