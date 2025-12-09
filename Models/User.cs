using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace BudgetEase.Models;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;

    public UserSettings? Settings { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public ICollection<Category> Categories { get; set; } = new List<Category>();
}

