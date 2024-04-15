using System.Collections.Generic;

namespace TextCommandFramework.Models;

public class UserList
{
    public string UserListId { get; set; }
    public List<Profile> Profiles { get; set; }
}