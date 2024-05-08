using System;
using Discord;
using Discord.Commands;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TextCommandFramework.Services;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using TextCommandFramework.Models;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using Discord.WebSocket;
using System.ComponentModel;
using System.Text;
using String = System.String;

namespace TextCommandFramework.Modules;

public class PublicModule : ModuleBase<SocketCommandContext>
{
    private delegate Task AsyncSocketMessageHandler(SocketMessageComponent message);

    private static bool _handlerAdded = false;
    private readonly BotContext _db;
    private readonly DiscordSocketClient _client;

    public StringBuilder response = new();

    public PublicModule(BotContext db, DiscordSocketClient client)
    {
        _db = db;
        _client = client;

        if (!_handlerAdded)
        {
            _client.ButtonExecuted += ClientOnButtonExecuted;
            _client.SelectMenuExecuted += ClientOnSelectMenuExecuted;

            _handlers.TryAdd("Account-id", HandleCustomButtonClicked);
            _handlers.TryAdd("Account-Me", HandleCustomButtonClicked);
            _handlers.TryAdd("Account-New", HandleCustomButtonClicked);
            _handlers.TryAdd("Account-Delete", HandleCustomButtonClicked);

            _handlers.TryAdd("Inventory-id", HandleCustomButtonClicked);
            _handlers.TryAdd("Inventory-check", HandleCustomButtonClicked);
            _handlers.TryAdd("Inventory-swap1", HandleCustomButtonClicked);
            _handlers.TryAdd("Inventory-swap2", HandleCustomButtonClicked);

            _handlers.TryAdd("Dungeon-id", HandleCustomButtonClicked);
            _handlers.TryAdd("Dungeon-Crawl", HandleCustomButtonClicked);
            _handlers.TryAdd("Dungeon-Run", HandleCustomButtonClicked);
            _handlers.TryAdd("Dungeon-Fight", HandleCustomButtonClicked);

            _handlers.TryAdd("Shop-id", HandleCustomButtonClicked);
            _handlers.TryAdd("Shop-Restock", HandleCustomButtonClicked);
            _handlers.TryAdd("Buy-menu", HandleCustomButtonClicked);
            _handlers.TryAdd("Weapons-menu", HandleCustomButtonClicked);
            _handlers.TryAdd("Sell-menu", HandleCustomButtonClicked);
            _handlers.TryAdd("View-menu", HandleCustomButtonClicked);

            _handlers.TryAdd("Save", HandleCustomButtonClicked);
            _handlers.TryAdd("Back", HandleCustomButtonClicked);

            _handlers.TryAdd("Help-id", HandleCustomButtonClicked);
            _handlerAdded = true;
        }

    }

    private async Task ClientOnSelectMenuExecuted(SocketMessageComponent component)
    {
        if (_handlers.ContainsKey(component.Data.CustomId))
            await _handlers[component.Data.CustomId](component);
        else
        {
            await component.RespondAsync($"No handler exists for select dropdown {component.Data.CustomId}");
        }
    }

    private Dictionary<string, AsyncSocketMessageHandler> _handlers = new();

    public async Task ClientOnButtonExecuted(SocketMessageComponent component)
    {
        if (_handlers.ContainsKey(component.Data.CustomId))
            await _handlers[component.Data.CustomId](component);
        else
        {
            await component.RespondAsync($"No handler exists for button id {component.Data.CustomId}");
        }
    }

    public async Task HandleCustomButtonClicked(SocketMessageComponent component)
    {
        var builder = new ComponentBuilder();
        var profile = await _db.Profile.FirstOrDefaultAsync(usr => usr.DiscordId == component.User.Id);
        var weapons = await _db.Weapon.OrderBy(w => w.Id).ToListAsync();

        if (component.Data.CustomId == "Back")
        {
            builder = new ComponentBuilder()
                .WithRows(new[]
                {
                    new ActionRowBuilder()
                        .WithButton("Account", "Account-id")
                        .WithButton("Inventory", "Inventory-id"),
                    new ActionRowBuilder()
                        .WithButton("Dungeon", "Dungeon-id")
                        .WithButton("Shop", "Shop-id"),
                    new ActionRowBuilder()
                        .WithButton("HelpCmds", "Help-id")
                        .WithButton("BackupSave", "Save")
                });

            await component.RespondAsync(components: builder.Build());
        }

        if (component.Data.CustomId == "Shop-id")
        {
            builder.WithRows(new[]
            {
                new ActionRowBuilder()
                    .WithButton("Restock", "Shop-Restock")
                    .WithButton("Buy", "Buy-menu")
                    .WithButton("Sell", "Sell-menu")
                    .WithButton("View", "View-menu")
            });

            await component.RespondAsync("Remember to swap to the right item before you sell one.",
                components: builder.Build());
        }

        if (component.Data.CustomId == "Buy-menu")
        {
            // var options = await
            //    _db.Weapon.OrderBy(x => x.Id).Select(x => new SelectMenuOptionBuilder().WithLabel(x.Name).WithValue(x.Id.ToString())).ToListAsync();

            var options = new List<SelectMenuOptionBuilder>();

            foreach (var i in profile.ShopItemsSave)
            {
                var weapon = await _db.Weapon.FindAsync(profile.ShopItemsSave[i]);
                options.Add(new SelectMenuOptionBuilder().WithLabel(weapon!.Name).WithValue(weapon!.Id.ToString()));
            }

            builder.WithSelectMenu("Weapons-menu", options);

            await component.RespondAsync(
                $"Select a weapon to buy. You have {profile.Money} gold and {profile.Inventory.Count - 1} " +
                $"inventory slots open.", components: builder.Build());
        }

        if (component.Data.CustomId == "Weapons-menu")
        {
            var weapon = component.Data.Values.ToString();
            var selectedValue = component.Data.Values.FirstOrDefault();

            if (selectedValue != null && int.TryParse(selectedValue, out var weaponId))
            {
                var weaponSelected = _db.Weapon.FindAsync(weaponId).ToString();
                await HandleShopAsync("Buy", weaponSelected, profile, weapons);
            }
            else
                await component.RespondAsync("Invalid selection.");
        }

        if (component.Data.CustomId == "View-menu")
        {
            await HandleShopAsync("View", null, profile, weapons, component);
        }

        if (component.Data.CustomId == "Sell-menu")
        {
            await HandleShopAsync("Sell", null, profile, weapons, component);
        }

        if (component.Data.CustomId == "Account-id")
        {
            builder.WithRows(new[]
            {
                new ActionRowBuilder()
                    .WithButton("My Account", "Account-Me")
                    .WithButton("New Account", "Account-New")
                    .WithButton("Delete Account", "Account-Delete")
                    .WithButton("Back", "Back")
            });

            await component.RespondAsync(components: builder.Build());
        }

        if (component.Data.CustomId == "Dungeon-id")
        {
            builder.WithRows(new[]
            {
                new ActionRowBuilder()
                    .WithButton("Run", "Dungeon-Run")
                    .WithButton("Crawl", "Dungeon-Crawl")
                    .WithButton("Fight", "Dungeon-Fight")
                    .WithButton("Back", "Back")
            });

            await component.RespondAsync(components: builder.Build());
        }

        if (component.Data.CustomId == "Account-Me")
        {
            await HandleGameAccountAsync("Me", null, profile, weapons, component);
        }

        if (component.Data.CustomId == "Account-New")
        {
            await HandleGameAccountAsync("New", null, profile, weapons, component);
        }

        if (component.Data.CustomId == "Account-Delete")
        {
            await HandleGameAccountAsync("Delete", null, profile, weapons, component);
        }

        if (component.Data.CustomId == "Save")
        {
            if (profile != null)
            {
                await UpdateProfileAsync(profile, component);
            }
        }

        if (component.Data.CustomId == "Inventory-id")
        {
            builder.WithRows(new[]
            {
                new ActionRowBuilder()
                    .WithButton("Check Inventory", "Inventory-check")
                    .WithButton("Swap Inventory", "Inventory-swap1")
            });

            await component.RespondAsync(components: builder.Build());
        }

        if (component.Data.CustomId == "Inventory-check")
        {
            await HandleInventoryAsync("CheckInv", null, profile, weapons, component);
        }

        if (component.Data.CustomId == "Inventory-swap1")
        {
            if (profile != null)
            {
                var inventoryOptions = new List<SelectMenuOptionBuilder>();

                for (int i = 0; i < profile.Inventory.Count - 1; i++)
                {
                    string weaponName = weapons.FirstOrDefault(w => w.Id == profile.Inventory[i])?.Name ?? "Unknown";

                    var option = new SelectMenuOptionBuilder()
                        .WithLabel(weaponName)
                        .WithValue($"Slot {i}: {profile.Inventory[i]}");

                    inventoryOptions.Add(option);
                }

                builder.WithSelectMenu("Inventory-swap2", inventoryOptions);

                await component.RespondAsync("Which slot do you want to swap to?", components: builder.Build());
            }
        }

        if (component.Data.CustomId == "Inventory-swap2")
        {
            var selectedValue = component.Data.Values.FirstOrDefault();
            var slot = selectedValue!.Split(":")[0].Split(" ")[1];

            await HandleInventoryAsync("ItemSwap", slot, profile, weapons, component);
        }

        if (component.Data.CustomId == "Dungeon-id")
        {
            builder.WithRows(new[]
            {
                new ActionRowBuilder()
                    .WithButton("Run", "Dungeon-Run")
                    .WithButton("Crawl", "Dungeon-Crawl")
                    .WithButton("Fight", "Dungeon-Fight")
                    .WithButton("Back", "Back")
            });

            await component.RespondAsync(components: builder.Build());
        }

        if (component.Data.CustomId == "Dungeon-Run")
        {
            if (profile != null)
            {
                await HandleDungeonAsync("Run", null, profile, weapons, component);
            }
        }

        if (component.Data.CustomId == "Dungeon-Crawl")
        {
            if (profile != null)
            {
                await HandleDungeonAsync("Crawl", null, profile, weapons, component);
            }
        }

        if (component.Data.CustomId == "Dungeon-Fight")
        {
            if (profile != null)
            {
                await HandleDungeonAsync("Fight", null, profile, weapons, component);
            }
        }

        if (component.Data.CustomId == "Help-id")
        {
            await HelpAsync(component);
        }

        return;
    }

    [Command("Buttons")]
    public async Task Spawn()
    {
        var builder = new ComponentBuilder()
            .WithRows(new[]
            {
                new ActionRowBuilder()
                    .WithButton("Account", "Account-id")
                    .WithButton("Inventory", "Inventory-id"),
                new ActionRowBuilder()
                    .WithButton("Dungeon", "Dungeon-id")
                    .WithButton("Shop", "Shop-id"),
                new ActionRowBuilder()
                    .WithButton("HelpCmds", "Help-id")
                    .WithButton("BackupSave", "Save")
            });

        /*
        builder.WithSelectMenu("myselectmenu", new List<SelectMenuOptionBuilder>(new[]
        {
            new SelectMenuOptionBuilder().WithLabel("Carrots").WithValue("5").WithDefault(true),
            new SelectMenuOptionBuilder().WithLabel("Lettuce").WithValue("6"),
            new SelectMenuOptionBuilder().WithLabel("Rah").WithValue("7")
        }));
        */

        await ReplyAsync(components: builder.Build());
    }

[Command("WTest")]
    public async Task TestAsync(string subCmd = "", int rah = 0)
    {
        var profile = await _db.Profile.FirstOrDefaultAsync(usr => usr.DiscordId == Context.User.Id);
        var weapons = await _db.Weapon.OrderBy(w => w.Id).ToListAsync();

        for (int i = 0; i < weapons.Count; i++)
        {
            if (weapons[i].Name == subCmd)
            {
                if (rah - 1 > profile.Inventory.Count - 1)
                {
                    await ReplyAsync($"Too big of a number!");
                    break;
                }

                if (rah - 1 < 0)
                {
                    await ReplyAsync($"Too small of a number!");
                    break;
                }

                profile.Inventory[rah - 1] = weapons[i].Id;
                profile.Damage[rah - 1] = weapons[i].Damage;
                profile.Value[rah - 1] = weapons[i].Value;
                await ReplyAsync($"You now own {weapons[i].Name}!");

                if (rah == profile.Inventory.Count)
                {
                    await ReplyAsync(
                        $"The item {weapons[i].Name} is now in the reserved slot, use cmd ( !Game SetItem Replace [ any number from 1 to MaxInv ]!");
                }

                break;
            }
        }
    }

    [Command("Game")]
    public async Task GameAsync(string subCommand = "", string mess2 = "", string nameLookup = "")
    {
        var profile = await _db.Profile.FirstOrDefaultAsync(usr => usr.DiscordId == Context.User.Id);
        var weapons = await _db.Weapon.OrderBy(w => w.Id).ToListAsync();

        switch (subCommand)
        {
            case "Account":
                await HandleGameAccountAsync(mess2, nameLookup, profile, weapons);
                break;

            case "Inventory":
                await HandleInventoryAsync(mess2, nameLookup, profile, weapons);
                break;

            case "Dungeon":
                await HandleDungeonAsync(mess2, nameLookup, profile, weapons);
                break;

            case "SetItem":
                await HandleSetItemAsync(mess2, nameLookup, profile, weapons);
                break;

            case "AllItems":
                await HandleAllItemsAsync(mess2, nameLookup, profile, weapons);
                break;

            case "Shop":
                await HandleShopAsync(mess2, nameLookup, profile, weapons);
                break;

            case "Save":
                await UpdateProfileAsync(profile);
                break;

            case "Help":
                await HelpAsync();
                break;

            case "LevelUp":
                await LevelUpAsync(profile, null, null);
                break;

            default:
                await ReplyAsync("Unknown command entered " + subCommand);
                break;
        }
    }

    public async Task ResponseAsync(StringBuilder r, SocketMessageComponent? component = null)
    {
        if (r == null)
        {
            string rese = "Error: The response is null.";

            if (component is null)
                await ReplyAsync(rese);
            else
                await component.RespondAsync(rese);

            return;
        }
        string response = r.ToString();

        if (component is null) 
            await ReplyAsync(response);
        else
            await component.RespondAsync(response);

        r.Clear();
        return;
    }


    public async Task LevelUpAsync(Profile profile, StringBuilder response, SocketMessageComponent component = null)
    {
        if (profile == null)
        {
            response.AppendLine("You don't have an account! Create one with !Game account new");
            return;
        }

        int experience = profile.Experience;
        int expNeed = 15 + (5 * profile.Level);

        if (experience < expNeed)
        {
            response.AppendLine($"You don't have enough experience to level up! You need {expNeed - experience} more experience.");
        }
        else if (experience >= expNeed)
        {
            profile.Level += 1;
            profile.Experience = 0;
            response.AppendLine($"You leveled up! You are now level {profile.Level}!");
        }

        await UpdateProfileAsync(profile, component);
        return;
    }

    public async Task UpdateProfileAsync(Profile profile, SocketMessageComponent? component = null)
    {
        if (profile == null)
        {
            response.AppendLine("You don't have an account! Create one with !Game account new");
            return;
        }

        await ResponseAsync(response, component);

        _db.Update(profile);
        await _db.SaveChangesAsync();
        return;
    }

    public async Task HandleAllItemsAsync(string mess2, string nameLookup, Profile profile, List<Weapon> weapons, SocketMessageComponent? component = null)
    {
        if (profile == null)
        {
            response.AppendLine("You don't have an account! Create one with !Game account new");
            return;
        }

        if (mess2 == "All")
        {
            response.AppendLine($"Here are the items in the shop!");
            for (int i = 0; i < weapons.Count; i++)
            {
                response.AppendLine($"{i}: ?");
            }
        }

        await ResponseAsync(response, component);
        await UpdateProfileAsync(profile, component);
        return;
    }

    public async Task HandleSetItemAsync(string mess2, string nameLookup, Profile profile, List<Weapon> weapons, SocketMessageComponent? component = null)
    {
        if (profile == null)
        {
            response.AppendLine("You don't have an account! Create one with !Game account new");
            return;
        }

        if (mess2 == "Remove" && profile.Inventory[profile.Inventory.Count] > 0)
        {
            response.AppendLine($"You lost your item {weapons[profile.Inventory[10]].Name} for good.");
            profile.Inventory[profile.Inventory.Count] = 0;
            profile.Damage[profile.Damage.Count] = 0;
            profile.Value[profile.Value.Count] = 0;
        }

        if (component == null)
        {
            if (mess2 == "Replace")
            {
                if (profile != null && (int.Parse(nameLookup) - 1) >= 0 || (int.Parse(nameLookup) - 1) <= 9 && profile.Inventory.Count - 1 != 0)
                {
                    response.AppendLine(
                        $"You lost your item {weapons[profile.Inventory[int.Parse(nameLookup) - 2]].Name} for good." +
                        $"\rIts now replaced with {weapons[profile.Inventory[profile.Inventory.Count - 1] - 1].Name}.");

                    profile.Inventory[(int.Parse(nameLookup) - 1)] = profile.Inventory[profile.Inventory.Count - 1];
                    profile.Damage[(int.Parse(nameLookup) - 1)] = profile.Damage[profile.Damage.Count - 1];
                    profile.Value[(int.Parse(nameLookup) - 1)] = profile.Value[profile.Value.Count - 1];

                    profile.Inventory[profile.Inventory.Count - 1] = 0;
                    profile.Damage[profile.Damage.Count - 1] = 0;
                    profile.Value[profile.Value.Count - 1] = 0;
                }
            }
        }
        else if (component != null)
        {
            if (mess2 == "Replace")
            {
                if (profile != null && (int.Parse(nameLookup) - 1) >= 0 || (int.Parse(nameLookup) - 1) <= 9 && profile.Inventory.Count - 1 != 0)
                {
                    response.AppendLine(
                        $"You lost your item {weapons[profile.Inventory[int.Parse(nameLookup) - 2]].Name} for good." +
                        $"\rIts now replaced with {weapons[profile.Inventory[profile.Inventory.Count - 1] - 1].Name}.");

                    profile.Inventory[(int.Parse(nameLookup) - 1)] = profile.Inventory[profile.Inventory.Count - 1];
                    profile.Damage[(int.Parse(nameLookup) - 1)] = profile.Damage[profile.Damage.Count - 1];
                    profile.Value[(int.Parse(nameLookup) - 1)] = profile.Value[profile.Value.Count - 1];

                    profile.Inventory[profile.Inventory.Count - 1] = 0;
                    profile.Damage[profile.Damage.Count - 1] = 0;
                    profile.Value[profile.Value.Count - 1] = 0;
                }
            }
        }
        

        await ResponseAsync(response, component);
        await UpdateProfileAsync(profile, component);
        return;
    }

    public async Task HandleDungeonAsync(string mess2, string nameLookup, Profile profile, List<Weapon> weapons, SocketMessageComponent? component = null)
    {
        if (profile == null)
        {
            response.AppendLine("You don't have an account! Create one with !Game account new");
            return;
        }

        if (mess2 == "Run" && profile.Fight > 0)
        {
            profile.CName = null;
            profile.CHP = 0;
            profile.CDamage = 0;
            profile.CExpGain = 0;

            profile.Fight = -1;
            response.AppendLine("You fled from the fight.");

            await ResponseAsync(response, component);
            await UpdateProfileAsync(profile, component);
            return;
        }
        else if (mess2 == "Run" && profile.Fight == -1)
        {
            response.AppendLine("You ain't even in a fight yet.");

            await ResponseAsync(response, component);
            return;
        }

        Random rnd = new Random();
        int detect = 0;

        if (mess2 == "Crawl" && profile.Fight < 0)
        {
            int Move = rnd.Next(1, 30);

            if (Move < 20)
            {
                response.AppendLine("You moved, but at what cost?");
                await ResponseAsync(response, component);
                return;
            }
            else if (Move > 20)
            {
                profile.Fight = rnd.Next(0, 3);

                // 0: Chicken, 1: Bee, 2: Poisonous Spider, 3: Wolf
                switch (profile.Fight)
                {
                    case 0:
                        profile.CName = "Chicken";
                        profile.CHP = 3;
                        profile.CDamage = 1;
                        profile.CExpGain = 5;
                        break;
                    case 1:
                        profile.CName = "Bee";
                        profile.CHP = 8;
                        profile.CDamage = 2;
                        profile.CExpGain = 7;
                        break;
                    case 2:
                        profile.CName = "Poisonous Spider";
                        profile.CHP = 12;
                        profile.CDamage = 3;
                        profile.CExpGain = 12;
                        break;
                    case 3:
                        profile.CName = "Wolf";
                        profile.CHP = 20;
                        profile.CDamage = 10;
                        profile.CExpGain = 25;
                        break;
                }
            }

            response.AppendLine($"You're in a Fight with: {profile.CName}! --> !Game Dungeon Fight");
        }

        if (mess2 == "Crawl" && profile.Fight >= 0)
        {
            response.AppendLine($"You're in a Fight with: {profile.CName}! --> !Game Dungeon Fight");
        }

        if (mess2 == "Fight" && profile.Fight >= 0)
        {
            int DMult = (profile.Damage[profile.ItemSelected] + profile.Level + profile.Value[profile.ItemSelected]);
            profile.CHP -= (DMult);

            response.AppendLine($"You swung at your opponent and did {DMult} damage \rYour Hp: {profile.Hp} \r{profile.CName}s Hp: {profile.CHP}");

            if (profile.CHP <= 0)
            {
                response.AppendLine($"You win! Here's the exp you've earned: {profile.CExpGain}");
                profile.Experience += profile.CExpGain;
                profile.CExpGain = 0;
                int random = rnd.Next(0, 6);

                profile.Inventory[10] = weapons[random].Id;
                profile.Damage[10] = weapons[random].Damage;
                profile.Value[10] = weapons[random].Value;

                profile.CExpGain = 0;

                response.AppendLine($"You found {weapons[random].Name}!" +
                             $"\rIt does {weapons[random].Damage} damage!" +
                             $"\rIt has a value of {weapons[random].Value}");

                await LevelUpAsync(profile, response, component);

                for (int i = 0; i < 9; i++)
                {
                    if (profile.Inventory[i] > 0)
                    {
                        detect = 1;
                    }
                }


                if (detect == 0)
                {
                    response.AppendLine(
                        "Your inventory is full! Use ( !SetItem [ Space size ] ) to swap an item with what you just found!" +
                        "\rDefinitely go check ( !Game Inventory CheckInv ) to see what you want to swap it with!" +
                        "\rIf you don't see anything you wanna swap it with, type in ( !Game SetItem Remove )!");
                }

                detect = 0;
                profile.Fight = -1;
            }
        }
        else if (mess2 == "Fight" && profile.Fight == -1)
        {
            response.AppendLine("You just swung at mid air like a crazy man! Are you shadow boxing?");
        }

        await ResponseAsync(response, component);

        await UpdateProfileAsync(profile, component);
        return;
    }
    public async Task HandleInventoryAsync(string mess2, string nameLookup, Profile profile, List<Weapon> weapons, SocketMessageComponent? component = null)
    {
        if (profile == null)
        {
            response.AppendLine("You don't have an account! Create one with !Game account new");
            return;
        }

        if (mess2 == "CheckInv")
        {
            List<string> yourInventory = new List<string>();

            for (int i = 0; i < profile.Inventory.Count - 1; i++)
            {
                if (profile.ItemSelected == i)
                {
                    yourInventory.Add($"{i + 1}: {weapons[profile.Inventory[i] - 1].Name} ( Using )");
                }
                else
                {
                    yourInventory.Add($"{i + 1}: {weapons[profile.Inventory[i] - 1].Name}");
                }
            }
            string inventoryMessage = string.Join("\n", yourInventory);
            response.AppendLine($"This is your Inventory {profile.Name}:\n{inventoryMessage}");
        }

        if (mess2 == "ItemSwap")
        {
            if (component == null)
            {
                profile.ItemSelected = (int.Parse(nameLookup) - 1);
            }
            else if (component != null)
            {
                profile.ItemSelected = (int.Parse(nameLookup));
            }

            if (profile.ItemSelected >= 0 && profile.ItemSelected <= 9)
            {
                response.AppendLine($"You are using {weapons[profile.Inventory[profile.ItemSelected] - 1].Name} now.");
            }
            else if (profile.ItemSelected > 9)
            {
                response.AppendLine($"You don't own this many inventory slots!");
            }
            else if (profile.ItemSelected < 0)
            {
                response.AppendLine($"Inventory starts at slot 1!");
            }
            else
            {
                response.AppendLine($"Error, womp womp.");
            }
        }

        await ResponseAsync(response, component);

        await UpdateProfileAsync(profile, component);
        return;
    }

    public async Task HandleGameAccountAsync(string mess2, string nameLookup, Profile profile, List<Weapon> weapons, SocketMessageComponent? component = null)
    {
        if (profile == null && mess2 == "New" && component == null)
        {
            profile = new Profile
            {
                Name = component.User.GlobalName,
                DiscordId = component.User.Id,
                Money = 100,
                Level = 1
            };

            _db.Profile.Add(profile);
            await _db.SaveChangesAsync();

            response.AppendLine("Account created!");
        }
        else if (profile == null && mess2 == "New" && component == null)
        {
            profile = new Profile
            {
                Name = Context.User.GlobalName,
                DiscordId = Context.User.Id,
                Money = 100,
                Level = 1
            };

            _db.Profile.Add(profile);
            await _db.SaveChangesAsync();

            response.AppendLine("Account created!");
        }
        else if (profile != null && mess2 == "New")
        {
            response.AppendLine("You already have a profile!");

            await ResponseAsync(response, component);
            await UpdateProfileAsync(profile, component);
            return;
        } 
        else if (profile == null && mess2 == "New")
        {
            response.AppendLine("Account not found!");

            await ResponseAsync(response, component);
            await UpdateProfileAsync(profile, component);
            return;
        }

        if (profile != null && mess2 == "Delete")
        {
            _db.Profile.Remove(profile);
            await _db.SaveChangesAsync();
            response.AppendLine("Account removed!");
        }

        if (profile != null && mess2 == "Me")
        {

            response.AppendLine($"This is: {profile.Name} " +
                         $"\nMoney: {profile.Money} " +
                         $"\nLevel: {profile.Level} " +
                         $"\nExperience: {profile.Experience} " +
                         $"\nSpace: {profile.Inventory.Count - 1}");
        }

        if (profile != null && mess2 == "ProfileLookup")
        {
            var other = await _db.Profile.FirstOrDefaultAsync(usr => usr.Name == nameLookup);
            
            if (other != null)
                response.AppendLine($"This is: {other.Name} " +
                           $"\nMoney: {other.Money} " +
                           $"\nLevel: {other.Level} " + 
                           $"\nExperience: {other.Experience} " + 
                           $"\nSpace: {other.Inventory.Count - 1}");
            else
            {
                response.AppendLine($"Sorry but I wasn't able to find {nameLookup}");
            }
        }

        if (profile == null && mess2 != "New")
        {
            response.AppendLine("Account not found!");
        }

        await ResponseAsync(response, component);

        await UpdateProfileAsync(profile, component);
        return;
    }
    
    // Adding / removing money, levels, etc. for testing purposes
    [Command("Test")]
    public async Task TestAsync(string mess1, string mess2, int amount, string nameLookupTest)
    {
        var user = await _db.Profile.FirstOrDefaultAsync(user => user.DiscordId == Context.User.Id);
        var other = await _db.Profile.FirstOrDefaultAsync(usr => usr.Name == nameLookupTest);
        StringBuilder response = new();

        if (user == null)
        {
            await ReplyAsync("You don't have an account! Create one with !Game account new");
        }

        

        if (mess1 == "Hp")
        {

            if (user != null && mess2 == "Add")
            {
                user.Hp += amount;
                await ReplyAsync($"Added {amount} HP");
            }
            else if (user != null && mess2 == "Remove")
            {
                user.Hp -= amount;
                await ReplyAsync($"Removed {amount} HP");
            }
        }

        if (mess1 == "Money")
        {
            if (nameLookupTest == "me")
            {
                if (user != null && mess2 == "Add")
                {
                    user.Money += amount;
                    await ReplyAsync($"Added {amount} gold");
                }
                else if (user != null && mess2 == "Remove")
                {
                    user.Money -= amount;
                    await ReplyAsync($"Removed {amount} gold");
                }
            }
            else if (other != null)
            {
                if (other != null && mess2 == "Add")
                {
                    other.Money += amount;
                    await ReplyAsync($"Added {amount} gold");
                }
                else if (other != null && mess2 == "Remove")
                {
                    other.Money -= amount;
                    await ReplyAsync($"Removed {amount} gold");
                }
            }
            else if (other == null)
            {
                await ReplyAsync($"User {nameLookupTest} doesn't exist!");
            }
        }

        if (mess1 == "Level")
        {
            if (nameLookupTest == "me")
            {
                if (user != null && mess2 == "Add")
                {
                    user.Level += amount;
                    await ReplyAsync($"Added {amount} level(s)");
                }
                else if (user != null && mess2 == "Remove")
                {
                    user.Level -= amount;
                    await ReplyAsync($"Removed {amount} level(s)");
                }
            }
            else if (other != null)
            {
                if (other != null && mess2 == "Add")
                {
                    other.Level += amount;
                    await ReplyAsync($"Added {amount} level(s)");
                }
                else if (other != null && mess2 == "Remove")
                {
                    other.Level -= amount;
                    await ReplyAsync($"Removed {amount} level(s)");
                }
            }
            else if (other == null)
            {
                await ReplyAsync($"User {nameLookupTest} doesn't exist!");
            }
        }

        if (mess1 == "Experience")
        {
            if (nameLookupTest == "me")
            {
                if (user != null && mess2 == "Add")
                {
                    user.Experience += amount;
                    await ReplyAsync($"Added {amount} experience");
                }
                else if (user != null && mess2 == "Remove")
                {
                    user.Experience -= amount;
                    await ReplyAsync($"Removed {amount} experience");
                }
            }
            else if (other != null)
            {
                if (other != null && mess2 == "Add")
                {
                    other.Experience += amount;
                    await ReplyAsync($"Added {amount} experience");
                }
                else if (other != null && mess2 == "Remove")
                {
                    other.Experience -= amount;
                    await ReplyAsync($"Removed {amount} experience");
                }
            }
            else if (other == null)
            {
                await ReplyAsync($"User {nameLookupTest} doesn't exist!");
            }
        }

        await UpdateProfileAsync(user);
        if (nameLookupTest != "me" && other != null)
        {
            await UpdateProfileAsync(other);
        } 
        return;
    }

    public async Task HandleShopAsync(string mess1, string nameLookup, Profile profile, List<Weapon> weapons, SocketMessageComponent? component = null)
    {
        Random rnd1 = new Random();

        if (profile == null)
        {
            response.AppendLine("You don't have an account! Create one with !Game account new");
            return;
        }

        if (mess1 == "Swap" && profile.Money >= 50)
        {
            switch (rnd1.Next(1, 7))
            {
                case 1:
                    profile.ShopItemsSave[0] = 1;
                    break;
                case 2:
                    profile.ShopItemsSave[0] = 2;
                    break;
                case 3:
                    profile.ShopItemsSave[0] = 3;
                    break;
                case 4:
                    profile.ShopItemsSave[0] = 4;
                    break;
                case 5:
                    profile.ShopItemsSave[0] = 5;
                    break;
                case 6:
                    profile.ShopItemsSave[0] = 6;
                    break;
                case 7:
                    profile.ShopItemsSave[0] = 7;
                    break;
            } // Item 1
            switch (rnd1.Next(1, 7))
            {
                case 1:
                    profile.ShopItemsSave[1] = 1;
                    break;
                case 2:
                    profile.ShopItemsSave[1] = 2;
                    break;
                case 3:
                    profile.ShopItemsSave[1] = 3;
                    break;
                case 4:
                    profile.ShopItemsSave[1] = 4;
                    break;
                case 5:
                    profile.ShopItemsSave[1] = 5;
                    break;
                case 6: 
                    profile.ShopItemsSave[1] = 6;
                    break;
                case 7:
                    profile.ShopItemsSave[1] = 7;
                    break;
            } // Item 2
            switch (rnd1.Next(1, 7))
            {
                case 1:
                    profile.ShopItemsSave[2] = 1;
                    break;
                case 2:
                    profile.ShopItemsSave[2] = 2;
                    break;
                case 3:
                    profile.ShopItemsSave[2] = 3;
                    break;
                case 4:
                    profile.ShopItemsSave[2] = 4;
                    break;
                case 5:
                    profile.ShopItemsSave[2] = 5;
                    break;
                case 6:
                    profile.ShopItemsSave[2] = 6;
                    break;
                case 7:
                    profile.ShopItemsSave[2] = 7;
                    break;
            } // Item 3

            profile.Money -= 50;

            response.AppendLine("You lost 50 gold for swapping the items!" +
                       $"\rYou now own {profile.Money} bucks!" +
                       $"\r\rHere is the current shop stock:" +
                       $"\rItem 1: {weapons[profile.ShopItemsSave[0]].Name}, Damage: {weapons[profile.ShopItemsSave[0]].Damage}, Costs: {weapons[profile.ShopItemsSave[0]].Value} gold." +
                       $"\rItem 2: {weapons[profile.ShopItemsSave[1]].Name}, Damage: {weapons[profile.ShopItemsSave[1]].Damage}, Costs: {weapons[profile.ShopItemsSave[1]].Value} gold." +
                       $"\rItem 3: {weapons[profile.ShopItemsSave[2]].Name}, Damage: {weapons[profile.ShopItemsSave[2]].Damage}, Costs: {weapons[profile.ShopItemsSave[2]].Value} gold.\");");
        }
        else if(profile.Money <= 49)
        {
            response.AppendLine("You ain't got enough money!" +
                       $"You currently have {profile.Money} bucks!");
        }

        if (profile != null && mess1 == "Sell")
        {
            profile.Money += profile.Value[profile.ItemSelected];
            profile.Inventory[profile.ItemSelected] = 0;
            profile.Damage[profile.ItemSelected] = 0;
            profile.Value[profile.ItemSelected] = 0;
            response.AppendLine($"You sold your weapon for {profile.Value[profile.ItemSelected]} gold!");
        }

        if (profile != null && mess1 == "View")
        {
            response.AppendLine($"\rItem 1: {weapons[profile.ShopItemsSave[0]].Name}, Damage: {weapons[profile.ShopItemsSave[0]].Damage}, Costs: {weapons[profile.ShopItemsSave[0]].Value} gold." +
                       $"\rItem 2: {weapons[profile.ShopItemsSave[1]].Name}, Damage: {weapons[profile.ShopItemsSave[1]].Damage}, Costs: {weapons[profile.ShopItemsSave[1]].Value} gold." +
                       $"\rItem 3: {weapons[profile.ShopItemsSave[2]].Name}, Damage: {weapons[profile.ShopItemsSave[2]].Damage}, Costs: {weapons[profile.ShopItemsSave[2]].Value} gold.\");");
        }
        
        if (profile != null && mess1 == "Buy")
        {
            if (profile.Money >= weapons[profile.ShopItemsSave[int.Parse(nameLookup) - 1]].Value)
            {
                profile.Money -= weapons[profile.ShopItemsSave[0]].Value;
                profile.Inventory[profile.ItemSelected] = weapons[profile.ShopItemsSave[int.Parse(nameLookup) - 1]].Id;
                profile.Damage[profile.ItemSelected] = weapons[profile.ShopItemsSave[int.Parse(nameLookup) - 1]].Damage;
                profile.Value[profile.ItemSelected] = weapons[profile.ShopItemsSave[int.Parse(nameLookup) - 1]].Value;
                response.AppendLine($"You bought the weapon {weapons[profile.ShopItemsSave[int.Parse(nameLookup) - 1]].Name} for {weapons[profile.ShopItemsSave[int.Parse(nameLookup) - 1]].Value} gold!");
            }
            else
            {
                response.AppendLine("You don't have enough money!" +
                           $"The item cost {weapons[profile.ShopItemsSave[int.Parse(nameLookup) - 1]].Value} while you only have {profile.Money} bucks!");
            }
        }

        await ResponseAsync(response, component);

        await UpdateProfileAsync(profile, component);
        return;
    }

    public async Task HelpAsync(SocketMessageComponent? component = null)
    {
        StringBuilder response = new();

        response.AppendLine("You will always put a space between your commands!" +
                   "\r\nTo use a command, try something like this! --> !Game account new" +
                   "\r\n\r\n!Test" +
                   "\r\n  Hp:" +
                   "\r\n      Add: Adds HP to your account." +
                   "\r\n      Remove: Removes HP from your account." +
                   "\r\n  Money:" +
                   "\r\n      Add: Adds money to your account." +
                   "\r\n      Remove: Removes money from your account." +
                   "\r\n  Level:" +
                   "\r\n      Add: Adds levels to your account." +
                   "\r\n      Remove: Removes levels from your account." +
                   "\r\n  Experience:" +
                   "\r\n      Add: Adds experience to your account." +
                   "\r\n      Remove: Removes experience from your account." +
                   "\r\n\r\n!Game" +
                   "\r\n  Account:" +
                   "\r\n      New: Creates an account for said profile.+" +
                   "\r\n      Me: Shows you the details of your account" +
                   "\r\n      ProfileLookup: Shows the details of others accounts that you look up." +
                   "\r\n      Delete: Deletes the profile you own." +
                   "\r\n  Dungeon:" +
                   "\r\n      Crawl: Moves you around in the dungeon." +
                   "\r\n      Fight: Fights the monster you're currently boxing." +
                   "\r\n  Inventory:" +
                   "\r\n      CheckInv: Checks the weapons you have in your Inventory." +
                   "\r\n      ItemSwap: Swaps the weapon you are using for the one you want to swap with." +
                   "\r\n  SetItem: Allows you to either set the item, or remove it." +
                   "\r\n      Remove: Removes the item you currently found after fighting ( use this if u don't want the item )." +
                   "\r\n      Replace: The item u decide to use ( Input a number from 1 - 10 )." +
                   "\r\n  Shop: Sells the weapon you are currently using." +
                   "\r\n      View: [Number]: Shows the weapons you can buy. Shop stock is randomly generated. Number can be anything" +
                   "\r\n      Buy: [Number (1-3)]: Purchases a weapon. Be sure to swap to an empty spot in your inventory first." +
                   "\r\n      Swap: Changes the shops items for 50 bucks.");

        await ResponseAsync(response, component);
    }
}