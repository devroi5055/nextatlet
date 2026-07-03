using System;
using System.Collections.Generic;
using System.Text;

namespace NextAtlet.Infrastructure.ExternalServices.Cvr
{
    public class CvrApiOptions
    {
        public const string SectionName = "CvrApi";

        public string BaseUrl { get; set; } = "";
        public string AccessToken { get; set; } = "";
        public int TimeoutSeconds { get; set; } = 10;
    }
}
