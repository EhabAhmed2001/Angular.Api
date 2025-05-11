using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Talabat.Core.Entities.Order_Aggregate;

namespace Talabat.Core.Entities.Identity
{
	public class AppUser : IdentityUser
	{
        public string DisplayNams { get; set; }

        public Address Address { get; set; }

    }
}
