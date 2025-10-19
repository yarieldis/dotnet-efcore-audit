using System;

namespace Data.Audit;

public interface IAuditable
{
    int Id { get; set; }
    DateTime ModifiedDate { get; set; }
    string ModifiedUser { get; set; }
}
