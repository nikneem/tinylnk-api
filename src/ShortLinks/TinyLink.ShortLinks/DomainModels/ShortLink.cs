using System.Text.RegularExpressions;
using HexMaster.DomainDrivenDesign;
using HexMaster.DomainDrivenDesign.ChangeTracking;
using TinyLink.Core;
using TinyLink.ShortLinks.Abstractions.DomainModels;
using TinyLink.ShortLinks.Abstractions.Exceptions;

namespace TinyLink.ShortLinks.DomainModels;

public class ShortLink : DomainModel<Guid>, IShortLink
{

    public string ShortCode { get; private set; }
    public string TargetUrl { get; private set; }
    public int? Hits { get; }
    public string OwnerId { get; }
    public DateTimeOffset CreatedOn { get; }
    public DateTimeOffset? ExpiresOn { get; private set; }

    public async Task SetShortCode(string value, Func<string, Task<bool>> isUniqueFunction)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ShortCodeNullOrEmptyException();
        }

        if (!Regex.IsMatch(value, Constants.ShortCodeRegularExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase))
        {
            throw new ShortCodeInvalidException();
        }

        if (!Equals(ShortCode, value))
        {
            var lowerCasedValue = value.ToLowerInvariant();
            var isUnique = await isUniqueFunction(lowerCasedValue);
            if (isUnique)
            {
                ShortCode = lowerCasedValue;
                SetState(TrackingState.Modified);
            }
            else
            {
                throw new ShortCodeNotUniqueException();
            }
        }
    }
    public void SetTargetUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new TargetUrlNullOrEmptyException();
        }

        if (!Regex.IsMatch(value, Constants.UrlRegularExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase))
        {
            throw new TargetUrlInvalidException();
        }

        if (!Equals(TargetUrl, value))
        {
            TargetUrl = value.ToLowerInvariant();
            SetState(TrackingState.Modified);
        }
    }
    public void SetExpiryDate(DateTimeOffset? value)
    {
        if (!Equals(ExpiresOn, value))
        {
            ExpiresOn = value;
            SetState(TrackingState.Modified);
        }
    }

    public ShortLink(
        Guid id, 
        string shortCode, 
        string targetUrl, 
        int hits,
        string ownerId, 
        DateTimeOffset createdOn, 
        DateTimeOffset? expiresOn) : base(id)
    {
        ShortCode = shortCode;
        TargetUrl = targetUrl;
        Hits = hits;
        OwnerId = ownerId;
        CreatedOn = createdOn;
        ExpiresOn = expiresOn;
    }

    private ShortLink(string targetUrl, string shortCode, string ownerId) : base(Guid.NewGuid(), TrackingState.New)
    {
        ShortCode = shortCode;
        TargetUrl = targetUrl;
        OwnerId = ownerId;
        CreatedOn = DateTimeOffset.UtcNow;
    }

    public static ShortLink Create(string targetUrl, string shortCode, string ownerId)
    {
        return new ShortLink(targetUrl, shortCode, ownerId);
    }
}
