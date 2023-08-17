namespace TinyLink.Hits.Abstractions.DataTransferObjects;

public record HitsTotalDto(string ShortCode, int TotalHits, DateTimeOffset? LastUpdated);