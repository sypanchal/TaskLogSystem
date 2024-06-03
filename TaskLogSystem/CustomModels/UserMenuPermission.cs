using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TaskLogSystem.CustomModels
{
    public class UserMenuPermission
    {
        public int MenuID
        {
            get; set;
        }
        public string MenuName
        {
            get; set;
        }
        public int? ParentMenuID
        {
            get; set;
        }
        public string IconName
        {
            get; set;
        }
        public string AccessName
        {
            get; set;
        }
    }
}