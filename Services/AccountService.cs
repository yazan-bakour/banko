namespace Banko.Services
{
  public static class AccountService
  {
    public static string GenerateAccountNumber()
    {
      Random random = new();
      return "NKO" + random.Next(10000000, 99999999).ToString();
    }
  }
}
