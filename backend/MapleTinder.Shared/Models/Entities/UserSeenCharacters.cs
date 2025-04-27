using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleTinder.Shared.Models.Entities
{
    public class UserSeenCharacter
    {
        public int UserId { get; set; }
        public int CharacterId { get; set; }
    }
}
