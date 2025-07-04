using System;

namespace backendDistributor.Models
{
    public class GRPOAttachment
    {
        public Guid Id { get; set; }
        public Guid GRPOId { get; set; } // Foreign Key
        public string? FileName { get; set; }
        public string? FilePath { get; set; }

        public GRPO? GRPO { get; set; } // Navigation property
    }
}