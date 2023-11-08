using HexMaster.DomainDrivenDesign.Abstractions;

namespace TinyLink.ShortLinks.Abstractions.DomainModels;

public interface IShortLink: IDomainModel<Guid>
{
    string ShortCode { get;  }
    string TargetUrl { get;  }
    int? Hits { get;  }
    string OwnerId { get; }
    DateTimeOffset CreatedOn { get; }
    DateTimeOffset? ExpiresOn { get;  }

    Task SetShortCode(string value, Func<string, Task<bool>> idUniqueFunction);
    void SetTargetUrl(string value);
    void SetExpiryDate(DateTimeOffset? value);
}
