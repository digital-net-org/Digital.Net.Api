using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models.Pages;

[Table("PageOpenGraph")]
[PivotResolution("/openGraph", Ownership.Cascade)]
public class PageOpenGraph : Pivot<Page, OpenGraphEntry>;
