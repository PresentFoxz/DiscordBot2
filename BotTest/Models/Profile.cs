using System;
using System.Collections.Generic;
using System.Linq;

namespace TextCommandFramework.Models;

public class Profile
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ProfileId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ulong DiscordId { get; set; }
    public int Hp { get; set; } = 10;
    public int Money { get; set; } = 100;
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public List<int> Inventory { get; set; } = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 };
    public List<int> Damage { get; set; } = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 };
    public List<int> Value { get; set; } = new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 };
    public List<int> ShopItemsSave { get; set; } = new List<int> { 0, 0, 0 };
    public int ItemSelected { get; set; } = 0;
    public int Fight { get; set; } = -1;
    public string CName { get; set; } = string.Empty;
    public int CExpGain { get; set; } = 0;
    public int CHP { get; set; } = 0;
    public int CDamage { get; set; } = 0;
}