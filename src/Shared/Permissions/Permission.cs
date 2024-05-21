using System.ComponentModel;

namespace Shared.Permissions
{
    public enum Permission
    {
        //employee
        [Description("EMPLOYEE CREATE")]
        EMPLOYEE_CREATE = 1001,
        [Description("EMPLOYEE READ")]
        EMPLOYEE_READ = 1002,
        [Description("EMPLOYEE UPDATE")]
        EMPLOYEE_UPDATE = 1003,
        [Description("EMPLOYEE DELETE")]
        EMPLOYEE_DELETE = 1004,
        //user
        [Description("USER CREATE")]
        USER_CREATE = 2001,
        [Description("USER READ")]
        USER_READ = 2002,
        [Description("USER UPDATE")]
        USER_UPDATE = 2003,
        [Description("USER DELETE")]
        USER_DELETE = 2004,
        //role
        [Description("USER CREATE")]
        ROLE_CREATE = 3001,
        [Description("USER READ")]
        ROLE_READ = 3002,
        [Description("USER UPDATE")]
        ROLE_UPDATE = 3003,
        [Description("USER DELETE")]
        ROLE_DELETE = 3004,
        TEST = 7004,
        TEST2 = 7005,
    }
}
