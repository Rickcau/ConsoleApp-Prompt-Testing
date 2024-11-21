using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace ConsoleApp_Prompt_Testing.Models
{


    public class ChatProviderValidationResponse
    {
        [JsonPropertyName("chatResponse")]
        public string? ChatResponse { get; set; }

        [JsonPropertyName("query")]
        public string? Query { get; set; }

        [JsonPropertyName("validationResponse")]
        public string? ValidationResponse { get; set; }
    }

    //public class ValidationResponse
    //{
    //    [JsonPropertyName("user_question_answer")]
    //    public List<UserQuestionAnswer> UserQuestionAnswer { get; set; } = new();
    //}

    //public class UserQuestionAnswer
    //{
    //    [JsonPropertyName("user_question")]
    //    public string? UserQuestion { get; set; }

    //    [JsonPropertyName("generated_sql")]
    //    public string? GeneratedSql { get; set; }

    //    [JsonPropertyName("sql_results")]
    //    public string? SqlResults { get; set; }

    //    [JsonPropertyName("thoughts")]
    //    public string? Thoughts { get; set; }

    //    [JsonPropertyName("rating")]
    //    public int Rating { get; set; }
    //}
}
