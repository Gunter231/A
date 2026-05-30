using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PersonalCabinetEducationProgram.Data;

#nullable disable

namespace PersonalCabinetEducationProgram.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    partial class ApplicationDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.EducationalProgram", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CodeReferral")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("code_referral");

                    b.Property<string>("EducationalLevel")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("educational_level");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.Property<DateTime?>("YearApprovals")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("year_approvals");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("educational_programs", "personal_cabinet");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.EducationalProgramElement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("EducationalProgramId")
                        .HasColumnType("int")
                        .HasColumnName("educational_program_id");

                    b.Property<string>("FileName")
                        .HasColumnType("longtext")
                        .HasColumnName("file_name");

                    b.Property<string>("FilePath")
                        .HasColumnType("longtext")
                        .HasColumnName("file_path");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<string>("StatusApprovals")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("status_approvals");

                    b.Property<string>("TypeElement")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("type_element");

                    b.Property<DateOnly?>("UploadDate")
                        .HasColumnType("date")
                        .HasColumnName("upload_date");

                    b.HasKey("Id");

                    b.HasIndex("EducationalProgramId");

                    b.ToTable("educational_program_elements", "personal_cabinet");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.EducationalProgramElementComment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("CommentContent")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("comment_content");

                    b.Property<DateTime>("DateTimeComment")
                        .HasColumnType("datetime(6)")
                        .HasColumnName("date_time_comment");

                    b.Property<int>("EducationalProgramElementId")
                        .HasColumnType("int")
                        .HasColumnName("educational_program_element_id");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.Property<int>("UserId")
                        .HasColumnType("int")
                        .HasColumnName("user_id");

                    b.HasKey("Id");

                    b.HasIndex("EducationalProgramElementId");

                    b.HasIndex("UserId");

                    b.ToTable("comments_educational_program_element", "personal_cabinet");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.User", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("full_name");

                    b.Property<string>("LinkRole")
                        .IsRequired()
                        .HasColumnType("longtext")
                        .HasColumnName("link_role");

                    b.Property<string>("Post")
                        .IsRequired()
                        .HasColumnType("longtext");

                    b.HasKey("Id");

                    b.ToTable("users", "personal_cabinet");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.EducationalProgram", b =>
                {
                    b.HasOne("PersonalCabinetEducationProgram.Models.User", "User")
                        .WithMany("EducationalPrograms")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.EducationalProgramElement", b =>
                {
                    b.HasOne("PersonalCabinetEducationProgram.Models.EducationalProgram", "EducationalProgram")
                        .WithMany("Elements")
                        .HasForeignKey("EducationalProgramId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("EducationalProgram");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.EducationalProgramElementComment", b =>
                {
                    b.HasOne("PersonalCabinetEducationProgram.Models.EducationalProgramElement", "Element")
                        .WithMany("Comments")
                        .HasForeignKey("EducationalProgramElementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PersonalCabinetEducationProgram.Models.User", "User")
                        .WithMany("Comments")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Element");

                    b.Navigation("User");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.EducationalProgram", b =>
                {
                    b.Navigation("Elements");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.EducationalProgramElement", b =>
                {
                    b.Navigation("Comments");
                });

            modelBuilder.Entity("PersonalCabinetEducationProgram.Models.User", b =>
                {
                    b.Navigation("Comments");

                    b.Navigation("EducationalPrograms");
                });
#pragma warning restore 612, 618
        }
    }
}
