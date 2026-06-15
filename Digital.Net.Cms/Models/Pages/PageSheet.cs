using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Cms.Models.Pages;

[Table("PageSheet")]
[PivotResolution("/sheets", Ownership.Cascade)]
public class PageSheet : Pivot<Page, Sheet>;
