using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLens.Models
{
    public class User : IdentityUser
    {
        public override string Id { get; set; }
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Viewer"; // Viewer, Designer, Admin

        [BsonElement("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        [BsonElement("LastLoginDate")]
        public DateTime? LastLoginDate { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(100)]
        public string? FirstName { get; set; }

        [StringLength(100)]
        public string? LastName { get; set; }

        public string FullName => $"{FirstName} {LastName}".Trim();
    }
}