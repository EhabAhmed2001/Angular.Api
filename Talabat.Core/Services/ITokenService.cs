﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Talabat.Core.Entities.Identity;

namespace Talabat.Core.Services
{
	public interface ITokenService
	{
		public Task<string> CreateTokenAsync(AppUser user);
	}
}
