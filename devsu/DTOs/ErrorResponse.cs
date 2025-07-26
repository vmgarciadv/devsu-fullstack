using System;
using System.Collections.Generic;

namespace devsu.DTOs
{
    public class ErrorResponse
    {
        public string Type { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string? Instance { get; set; }
        public Dictionary<string, List<string>> Errors { get; set; }

        public ErrorResponse()
        {
            Errors = new Dictionary<string, List<string>>();
        }

        public ErrorResponse(string type, string title, int status, string detail, string? instance = null) : this()
        {
            Type = type;
            Title = title;
            Status = status;
            Detail = detail;
            Instance = instance;
        }
    }
}