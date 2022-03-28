using System;
using System.ComponentModel.DataAnnotations;

namespace Web2App.Models
{
    public class QrCodePostModel
    {
        [Required]
        public string OperationId { get; set; }
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        public string Assignee { get; set; }
        [Required]
        public string SecretKey { get; set; }
        [Required]
        public int ClientId { get; set; }
        [Required]
        public string IconUri { get; set; }
        [Required]
        public string CallBackUrl { get; set; }
        [Required]
        public string Type { get; set; }
    }
}
