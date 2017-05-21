using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Monokrome.DataModels
{
    public class Score
    {
        public string Id { get; set; }
        public string Value { get; set; }
        public DateTime DateCreated { get; set; }

        public string PlayerId { get; set; }
        public string GameVariantId { get; set; }
    }
}
