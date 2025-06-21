namespace Banko.Models;

public class Privacy : BaseEntity
{
  public bool HideEmail { get; set; } = false;
  public bool HideBalance { get; set; } = false;
  public bool EnableTwoFactorAuth { get; set; } = false;
  public bool ReceiveMarketingEmails { get; set; } = true;
}

public class Preferences : BaseEntity
{
  public bool DarkMode { get; set; } = false;
  public string Language { get; set; } = "en-US";
  public string TimeZone { get; set; } = "UTC";
  public string DateFormat { get; set; } = "MM/dd/yyyy";
  public Currency CurrencyDisplay { get; set; } = Currency.EUR;
  public bool PushNotifications { get; set; } = true;
  public bool TransactionAlerts { get; set; } = true;
  public bool LowBalanceAlerts { get; set; } = true;
  public decimal LowBalanceThreshold { get; set; } = 100.00m;
  public Privacy Privacy { get; set; } = new Privacy();
}