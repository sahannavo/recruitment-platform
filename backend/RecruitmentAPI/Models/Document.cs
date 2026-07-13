using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RecruitmentAPI.Models
{
    [Table("Documents")]
    public class Document
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int DocumentId { get; set; }

        [Required]
        public int CandidateId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; }

        [Required]
        [MaxLength(500)]
        public string BlobUrl { get; set; }

        [Required]
        [MaxLength(50)]
        public string FileType { get; set; }

        [Required]
        public DateTime UploadedAt { get; set; }

        // Additional fields for better document management
        [MaxLength(100)]
        public string DocumentType { get; set; } // CV, CoverLetter, Portfolio, etc.

        public long FileSize { get; set; } // File size in bytes

        [MaxLength(50)]
        public string FileExtension { get; set; } // .pdf, .docx, .txt, etc.

        [MaxLength(255)]
        public string DocumentName { get; set; } // Display name

        [MaxLength(500)]
        public string Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? UpdatedAt { get; set; }

        [MaxLength(100)]
        public string UploadedBy { get; set; } // Who uploaded the document

        public bool IsParsed { get; set; } = false; // Whether AI has parsed this document

        public DateTime? ParsedAt { get; set; } // When AI parsed the document

        public string ParseResult { get; set; } // JSON string of parsed data

        // Navigation Properties
        [ForeignKey("CandidateId")]
        public virtual Candidate Candidate { get; set; }
    }

    public enum DocumentType
    {
        CV = 0,
        CoverLetter = 1,
        Portfolio = 2,
        Certificate = 3,
        Transcript = 4,
        RecommendationLetter = 5,
        Other = 6
    }

    public enum FileType
    {
        PDF = 0,
        DOCX = 1,
        DOC = 2,
        TXT = 3,
        RTF = 4,
        JPEG = 5,
        PNG = 6,
        Other = 7
    }
}