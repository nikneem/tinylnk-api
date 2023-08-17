namespace TinyLink.ShortLinks.Abstractions.DomainModels;

public interface IShortLink
{
    string ShortCode { get;  }
    string TargetUrl { get;  }
    string OwnerId { get; }
    DateTimeOffset CreatedOn { get; }
    DateTimeOffset? ExpiresOn { get;  }

    Task SetShortCode(string value, Func<string, Task<bool>> idUniqueFunction);
    void SetTargetUrl(string value);
    void SetExpiryDate(DateTimeOffset? value);
}
