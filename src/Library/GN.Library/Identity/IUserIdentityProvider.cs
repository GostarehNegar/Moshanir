using GN.Library.Shared.Internals;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GN.Library.Identity
{
	/// <summary>
	/// Abstracts functionalities required to get User Identity information.
	/// These informations are normally attained from user directory providers
	/// such as Active Directory as compared to User's information that are 
	/// stored in systems such as Microsoft Dynamic CRM. 
	/// Identity here refers to user indentities stored in authentication 
	/// system which together with user data in databses provides a 
	/// complete set of information about a user.
	/// </summary>
	public interface IUserIdentityProvider
	{
		Task<UserIdentityEntity> LoadUser(string userName);
		Task<IEnumerable<UserIdentityEntity>> FindByIpPhone(string ipPhone, bool includeDisabledUsers = false);
		Task<IEnumerable<UserIdentityEntity>> FindByEmail(string ipPhone, bool includeDisabledUsers = false);
		Task<IEnumerable<UserIdentityEntity>> FindAll(bool includeDisabledUsers = false);
	}
}
