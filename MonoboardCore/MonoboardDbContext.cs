using Microsoft.EntityFrameworkCore;
using MonoboardCore.Model;

namespace MonoboardCore
{
	public sealed class MonoboardDbContext : DbContext
	{
		/// <summary>
		/// Доступ до даних курсів валют
		/// </summary>
		public DbSet<ExchangeRates> ExchangeRates { get; set; }

		/// <summary>
		/// Доступ до даних особистих налаштувань користувача
		/// </summary>
		public DbSet<MonoboardUser> MonoBoardUsers { get; set; }

		/// <summary>
		/// Доступ до даних особистої інформації користувача
		/// </summary>
		public DbSet<UserInfo> UserInfo { get; set; }

		/// <summary>
		/// Доступ до даних карт/рахунків користувача
		/// </summary>
		public DbSet<Account> Accounts { get; set; }

		/// <summary>
		/// Доступ до даних для перевірки завантажених виписок за казаний період
		/// </summary>
		public DbSet<StatementItemsChecker> StatementItemsCheckers { get; set; }

		/// <summary>
		/// Доступ до даних виписки по картці/рахунку користувача
		/// </summary>
		public DbSet<StatementItem> StatementItems { get; set; }

		/// <summary>
		/// Доступ до даних партнерів монобанку (Покупка частинами)
		/// </summary>
		public DbSet<Partner> Partners { get; set; }

		/// <summary>
		/// Доступ до даних категорій взаємодії з партнерами монобанку (Покупка частинами)
		/// </summary>
		public DbSet<PartnersCategory> PartnersCategories { get; set; }

		protected override void OnModelCreating(ModelBuilder builder)
		{
			builder.Entity<UserInfo>()
				.HasOne(u => u.MonoboardUser)
				.WithOne(mb => mb.UserInfo)
				.HasForeignKey<MonoboardUser>(mb => mb.MonoboardUserKey)
				.OnDelete(DeleteBehavior.Cascade);


			builder.Entity<UserInfo>()
				.HasMany(u => u.Accounts)
				.WithOne(mb => mb.UserInfo)
				.HasForeignKey(mb => mb.AccountKey)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<Account>()
				.HasMany(u => u.StatementItems)
				.WithOne(mb => mb.Account)
				.HasForeignKey(mb => mb.CardKey)
				.HasPrincipalKey(account => account.CardCode)
				.OnDelete(DeleteBehavior.Cascade);

			builder.Entity<Partner>()
				.HasMany(p => p.Categories)
				.WithOne(pc => pc.Partner)
				.HasForeignKey(mb => mb.PartnerKey)
				.OnDelete(DeleteBehavior.Cascade);

			base.OnModelCreating(builder);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
			optionsBuilder.UseSqlite("Data Source=MonoBoard.db");

		public bool CreateDatabase() => Database.EnsureCreated();
	}
}
