using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Cms.Models.Pages;

[Table("PageOpenGraph")]
[PivotResolution("/openGraph", Ownership.Cascade)]
public class PageOpenGraph : Pivot<Page, OpenGraphEntry>;
