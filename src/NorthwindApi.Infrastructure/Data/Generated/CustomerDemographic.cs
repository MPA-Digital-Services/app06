using System;
using System.Collections.Generic;

namespace NorthwindApi.Infrastructure.Data.Generated;

public partial class CustomerDemographic
{
    public string CustomerTypeID { get; set; } = null!;

    public string? CustomerDesc { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
