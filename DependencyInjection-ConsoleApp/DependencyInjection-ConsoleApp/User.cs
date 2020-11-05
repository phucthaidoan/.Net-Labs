using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection_ConsoleApp
{
    public class User : IUser
    {
        public string Name { get; set; }

        public void TruncateName(string name)
        {
            Name = name.Trim();
        }
    }
}
