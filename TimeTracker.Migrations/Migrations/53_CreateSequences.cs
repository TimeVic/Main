using FluentMigrator;
using TimeTracker.Migrations.Code;

namespace TimeTracker.Migrations.Migrations
{
    [Migration(53)]
    public class _53_CreateSequences : MyMigration
    {
        public override void Up()
        {
            Create.Table("sequences")
                .WithColumn("id").AsGuid().PrimaryKey().Unique().NotNullable().WithDefault(SystemMethods.NewGuid)
                .WithColumn("counter").AsInt64().Nullable().WithDefaultValue(1)
                .WithColumn("entity").AsString().Nullable()
                .WithColumn("entity_id").AsString().Nullable()
                .WithColumn("created_at").AsDateTime().NotNullable()
                .WithColumn("updated_at").AsDateTime().Nullable();
            
            Create.UniqueConstraint().OnTable("sequences").Columns("entity", "entity_id");
            
            base.Up();
        }
    }
}
