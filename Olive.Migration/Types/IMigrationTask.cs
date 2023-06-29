namespace Olive.Migration
{
	using Olive.Entities;
	using System;

	public interface IMigrationTask:IEntity
	{
		public string Name { get;set;}
		public DateTime CreateOn { get;set;}
		public DateTime LoadOn { get;set;}
		public DateTime? MigrateStartOn { get;set;}
		public DateTime? MigrateEndOn { get;set;}
		public bool Migrated { get;set; }
		public string BeforeMigrationBackupPath { get; set; }
		public string AfterMigrationBackupPath { get; set; }
		public string LastError { get; set; }
	}
}
