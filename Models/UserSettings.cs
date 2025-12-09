using System;

namespace BudgetEase.Models;

public class UserSettings
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public string Currency { get; set; } = "USD";

    public string Theme { get; set; } = "system"; // light, dark, system

    public string TimeZone { get; set; } = "UTC";
}


