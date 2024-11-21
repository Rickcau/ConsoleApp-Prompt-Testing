using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp_Prompt_Testing.Models
{
    public class ChatProviderRequest
    {
        public string? SessionId { get; set; }

        [Required]
        public string? UserId { get; set; }

        [Required]
        public required string Prompt { get; set; }

        [DefaultValue(false)]
        public bool RunValidation { get; set; } = false;
    }
}
