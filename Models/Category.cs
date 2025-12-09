using System;
using System.Collections.Generic;

namespace BudgetEase.Models;

public class Category
{
    public Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public ApplicationUser? User { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ColorHex { get; set; }

    /// <summary>
    /// Indicates that this category is a global default (available as a template for any user).
    /// </summary>
    public bool IsDefaultGlobal { get; set; }

    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}


