using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace server.Controllers
{
    public static class Constants
    {
        public const string Issuer = Audience;
        public const string Audience = "https://localhost:44357/";
        public const string Secret = "xecretKeywqejaney";
    }
}
