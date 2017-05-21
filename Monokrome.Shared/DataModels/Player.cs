using System;
using System.Collections.Generic;
using System.Text;

namespace Monokrome.DataModels
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; set; }
        public string OSName { get; set; }
        public string DeviceIdentifier { get; set; }
        public string Password { get; set; }
    }
}
