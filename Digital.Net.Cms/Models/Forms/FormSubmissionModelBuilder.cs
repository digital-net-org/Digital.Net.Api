using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Forms;

public static class FormSubmissionModelBuilder
{
    public static ModelBuilder BuildFormSubmission(this ModelBuilder builder)
    {
        builder.Entity<FormSubmission>()
            .HasOne(s => s.Form)
            .WithMany(fm => fm.Submissions)
            .HasForeignKey(s => s.FormId)
            .OnDelete(DeleteBehavior.Cascade);
        return builder;
    }
}