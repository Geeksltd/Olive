namespace Olive.Entities.Data.IRepository.GenericRepository;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Enum to set record state.
/// </summary>
public enum StateFilter
{
    /// <summary>
    /// Returns only active records.
    /// </summary>
    Active,

    /// <summary>
    /// Returns only archived records.
    /// </summary>
    Archived,

    /// <summary>
    /// Returns all records.
    /// </summary>
    All,
}