using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(1)]
    public class _1_Init : MyMigration
    {
        public override void Up()
        {
            ExecuteScriptByName("1_create_types");
            
            Create.Table("users")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_name").AsString(200).Unique()
                .WithColumn("email").AsString(200).Unique()
                .WithColumn("password_hash").AsBinary()
                .WithColumn("password_salt").AsBinary()
                .WithColumn("create_time").AsDateTime()
                .WithColumn("update_time").AsDateTime();
            
            Create.Table("applications")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64()
                .WithColumn("name").AsString(200)   
                .WithColumn("api_token").AsString(2048)
                .WithColumn("create_time").AsDateTime()
                .WithColumn("update_time").AsDateTime();

            Create.ForeignKey()
                .FromTable("applications").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");
            
            Create.Table("logs")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("application_id").AsInt64()
                .WithColumn("level").AsString(5)
                .WithColumn("message").AsString(2048)
                .WithColumn("log_time").AsDateTime()
                .WithColumn("create_time").AsDateTime();
            
            Execute.Sql("ALTER TABLE logs ADD CONSTRAINT cs_logs_check_log_level CHECK(level IN ('E', 'W', 'F', 'I', 'D'))");
            
            Create.ForeignKey()
                .FromTable("logs").ForeignColumn("application_id")
                .ToTable("applications").PrimaryColumn("id");
            
            Create.Table("log_properties")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("log_id").AsInt64()
                .WithColumn("key").AsString(200)
                .WithColumn("value").AsString(2048)
                .WithColumn("create_time").AsDateTime();
            
            Create.ForeignKey()
                .FromTable("log_properties").ForeignColumn("log_id")
                .ToTable("logs").PrimaryColumn("id");
            
            Create.Table("log_traces")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("log_id").AsInt64()
                .WithColumn("trace").AsString(2048)
                .WithColumn("create_time").AsDateTime();
            
            Create.ForeignKey()
                .FromTable("log_traces").ForeignColumn("log_id")
                .ToTable("logs").PrimaryColumn("id");
            
            Alter.Table("logs")
                .AddColumn("is_favorite").AsBoolean().WithDefaultValue(false).NotNullable();
            
            Alter.Table("logs")
                .AddColumn("token").AsString(50).Nullable().Unique();
            
            Create.Table("request_logs")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("type").AsString(4)
                .WithColumn("token").AsString(512).Indexed().Nullable()
                .WithColumn("client_ip").AsString(200).Nullable()
                .WithColumn("data").AsCustom("text").Nullable()
                .WithColumn("create_time").AsDateTime();
            
            Execute.Sql("ALTER TABLE request_logs ADD CONSTRAINT cs_request_logs_check_type CHECK(type IN ('L', 'R'))");
            
            // logs

            Create.Table("log_levels")
                .WithColumn("id").AsInt64().PrimaryKey()
                .WithColumn("name").AsString(100);

            Insert.IntoTable("log_levels")
                .Row(new { id = 1, name = "Error" })
                .Row(new { id = 2, name = "Warning" })
                .Row(new { id = 3, name = "Fatal" })
                .Row(new { id = 4, name = "Information" })
                .Row(new { id = 5, name = "Debug" });

            Alter.Table("logs")
                .AddColumn("log_level").AsInt64().WithDefaultValue(1);

            Create.ForeignKey()
                .FromTable("logs").ForeignColumn("log_level")
                .ToTable("log_levels").PrimaryColumn("id");

            // request_logs
            Create.Table("request_log_types")
                .WithColumn("id").AsInt64().PrimaryKey()
                .WithColumn("name").AsString(100);

            Insert.IntoTable("request_log_types")
                .Row(new { id = 1, name = "Log" })
                .Row(new { id = 2, name = "Request" });

            Alter.Table("request_logs")
                .AddColumn("log_type").AsInt64().WithDefaultValue(1);

            Create.ForeignKey()
                .FromTable("request_logs").ForeignColumn("log_type")
                .ToTable("request_log_types").PrimaryColumn("id");
            
            Execute.Sql("UPDATE logs SET log_level = 1 WHERE level = 'E';");
            Execute.Sql("UPDATE logs SET log_level = 2 WHERE level = 'W';");
            Execute.Sql("UPDATE logs SET log_level = 3 WHERE level = 'F';");
            Execute.Sql("UPDATE logs SET log_level = 4 WHERE level = 'I';");
            Execute.Sql("UPDATE logs SET log_level = 5 WHERE level = 'D';");

            Execute.Sql("UPDATE request_logs SET log_type = 1 WHERE type = 'L';");
            Execute.Sql("UPDATE request_logs SET log_type = 2 WHERE type = 'R';");
            
            Delete.Column("level").FromTable("logs");
            Rename.Column("log_level").OnTable("logs").To("level");
            
            Delete.Column("type").FromTable("request_logs");
            Rename.Column("log_type").OnTable("request_logs").To("type");
            
            Create.Index("ix_logs_application_id")
                .OnTable("logs")
                .OnColumn("application_id");
            
            // logs
            Create.Index("ix_log_properties_log_id")
                .OnTable("log_properties")
                .OnColumn("log_id");
            
            Create.Index("ix_log_traces_log_id")
                .OnTable("log_traces")
                .OnColumn("log_id");
            
            Create.Index("ix_logs_level")
                .OnTable("logs")
                .OnColumn("level");
            
            Create.Index("ix_logs_log_time")
                .OnTable("logs")
                .OnColumn("log_time");
            
            Create.Table("application_users")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64().NotNullable()
                .WithColumn("application_id").AsInt64().NotNullable();

            Create.ForeignKey()
                .FromTable("application_users").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("application_users").ForeignColumn("application_id")
                .ToTable("applications").PrimaryColumn("id");

            Create.UniqueConstraint()
                .OnTable("application_users")
                .Columns("application_id", "user_id");
            
            Create.Table("log_favorites")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("user_id").AsInt64().NotNullable()
                .WithColumn("log_id").AsInt64().NotNullable();

            Create.ForeignKey()
                .FromTable("log_favorites").ForeignColumn("user_id")
                .ToTable("users").PrimaryColumn("id");

            Create.ForeignKey()
                .FromTable("log_favorites").ForeignColumn("log_id")
                .ToTable("logs").PrimaryColumn("id");

            Create.UniqueConstraint()
                .OnTable("log_favorites")
                .Columns("log_id", "user_id");

            Delete.Column("is_favorite").FromTable("logs");
            
            Create.Table("log_shares")
                .WithColumn("id").AsInt64().PrimaryKey().Identity()
                .WithColumn("log_id").AsInt64().NotNullable()
                .WithColumn("token").AsString().NotNullable()
                .WithColumn("create_time").AsDateTime().NotNullable()
                .WithColumn("update_time").AsDateTime().NotNullable();

            Create.ForeignKey()
                .FromTable("log_shares").ForeignColumn("log_id")
                .ToTable("logs").PrimaryColumn("id");

            Create.UniqueConstraint()
                .OnTable("log_shares")
                .Columns("log_id");
            
            Delete.ForeignKey("FK_log_properties_log_id_logs_id").OnTable("log_properties");

            Create.ForeignKey()
                .FromTable("log_properties").ForeignColumn("log_id")
                .ToTable("logs").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);
            
            Delete.ForeignKey("FK_log_traces_log_id_logs_id").OnTable("log_traces");

            Create.ForeignKey()
                .FromTable("log_traces").ForeignColumn("log_id")
                .ToTable("logs").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);
            
            Delete.UniqueConstraint("UC_log_favorites_log_id_user_id").FromTable("log_favorites");
            Delete.ForeignKey("FK_log_favorites_log_id_logs_id").OnTable("log_favorites");

            Create.ForeignKey()
                .FromTable("log_favorites").ForeignColumn("log_id")
                .ToTable("logs").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            Create.UniqueConstraint()
                .OnTable("log_favorites")
                .Columns("user_id", "log_id");
            
            Delete.UniqueConstraint("UC_log_shares_log_id").FromTable("log_shares");
            Delete.ForeignKey("FK_log_shares_log_id_logs_id").OnTable("log_shares");

            Create.ForeignKey()
                .FromTable("log_shares").ForeignColumn("log_id")
                .ToTable("logs").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);

            Create.UniqueConstraint()
                .OnTable("log_shares")
                .Columns("log_id");
            
            Delete.ForeignKey("FK_logs_application_id_applications_id").OnTable("logs");

            Create.ForeignKey()
                .FromTable("logs").ForeignColumn("application_id")
                .ToTable("applications").PrimaryColumn("id")
                .OnDelete(System.Data.Rule.Cascade);
            
            base.Up();
        }
    }
}
