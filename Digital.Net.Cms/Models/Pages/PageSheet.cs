using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models.Pages;

[Table("PageSheet")]
[PivotResolution("/sheets", Ownership.Cascade)]
public class PageSheet : Pivot<Page, Sheet>;
