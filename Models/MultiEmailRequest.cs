using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PlusCP.Models
{
    public class MultiEmailRequest
    {
        public List<Dictionary<string, string>> TableData { get; set; }
        public string[] CCemails { get; set; }
    }
}