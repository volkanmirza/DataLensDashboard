using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DataLens.Models
{
    public class UserGroupMembership
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string GroupId { get; set; } = string.Empty;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public string AddedBy { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime? RemovedDate { get; set; }

        public string? RemovedBy { get; set; }

        public string? Notes { get; set; }
    }
}