
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Forms;

public static class FormFieldModelBuilder
{
    public static ModelBuilder BuildFormField(this ModelBuilder builder)
    {
        builder.Entity<FormField>()
            .HasOne(f => f.Form)
            .WithMany(fm => fm.Fields)
            .HasForeignKey(f => f.FormId)
            .OnDelete(DeleteBehavior.Cascade);
        return builder;
    }
}