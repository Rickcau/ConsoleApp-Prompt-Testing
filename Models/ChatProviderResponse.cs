using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ConsoleApp_Prompt_Testing.Models
{


    public class ChatProviderResponse
    {
        [JsonPropertyName("chat_Response")]
        public string? ChatResponse { get; set; }

        [JsonPropertyName("query")]
        public string? Query { get; set; }

        [JsonPropertyName("validation_Response")]
        public string? ValidationResponse { get; set; }
    }
}
