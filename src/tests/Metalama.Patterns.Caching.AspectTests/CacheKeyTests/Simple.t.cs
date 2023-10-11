using Flashtrace.Formatters;
using Metalama.Patterns.Caching.Aspects;
using Metalama.Patterns.Caching.Formatters;
namespace Metalama.Patterns.Caching.AspectTests.CacheKeyTests.Simple;
public class TheClass : IFormattable<CacheKeyFormatting>
{
  [CacheKey]
  public string Id { get; }
  [CacheKey]
  public int SubId { get; }
  public string? Description { get; }
  void IFormattable<CacheKeyFormatting>.Format(UnsafeStringBuilder stringBuilder, IFormatterRepository formatterRepository)
  {
    stringBuilder.Append(this.GetType().FullName);
    if (formatterRepository.Role is CacheKeyFormatting)
    {
      stringBuilder.Append(" ");
      formatterRepository.Get<string>().Format(stringBuilder, this.Id);
      stringBuilder.Append(" ");
      formatterRepository.Get<int>().Format(stringBuilder, this.SubId);
    }
  }
  protected virtual void FormatCacheKey(UnsafeStringBuilder stringBuilder, IFormatterRepository formatterRepository)
  {
    stringBuilder.Append(this.GetType().FullName);
    if (formatterRepository.Role is CacheKeyFormatting)
    {
      stringBuilder.Append(" ");
      formatterRepository.Get<string>().Format(stringBuilder, this.Id);
      stringBuilder.Append(" ");
      formatterRepository.Get<int>().Format(stringBuilder, this.SubId);
    }
  }
}