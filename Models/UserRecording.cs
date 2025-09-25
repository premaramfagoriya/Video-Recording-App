using System;
using System.ComponentModel.DataAnnotations;

namespace RecordingApp.Models
{
    public class UserRecording
    {
        public int Id { get; set; }

        [Required]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime RecordingDate { get; set; }

        public long FileSize { get; set; }

        public int Duration { get; set; } // in seconds
    }
}