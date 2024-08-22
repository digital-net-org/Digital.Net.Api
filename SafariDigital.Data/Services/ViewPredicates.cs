using System.Linq.Expressions;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Services;

public static class ViewPredicates
{
    public static Expression<Func<ViewFrame, bool>> ByViewAndFrameIds(int viewId, int viewFrameId) =>
        x => x.ViewId == viewId && x.Id == viewFrameId;
}