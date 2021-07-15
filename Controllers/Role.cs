using FilmFree.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace FilmFree.Controllers
{
    public class Role : RoleProvider
    {
        public override string ApplicationName { 
            get { throw new NotImplementedException(); } 
            set { throw new NotImplementedException(); } 
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }
        FilmFreeEntities DB = new FilmFreeEntities();
        public override string[] GetRolesForUser(string username)
        {
            string usrNme= username.Split(',')[0], dgrlma= username.Split(',')[2];
            Kullanicilar kllnci = DB.Kullanicilar.FirstOrDefault(x => x.kAdi == usrNme && x.Dogrulama == dgrlma);
            if (kllnci==null)
            {
                throw new NotImplementedException();
            }
            return new string[] { kllnci.Yetki };

        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}