namespace Banko.Helpers
{
  public static class AccountHelper
  {
    public static string GenerateAccountNumber()
    {
      Random random = new();
      return "NKO" + random.Next(10000000, 99999999).ToString();
    }
    public static int GenerateRandomNumber()
    {
      Random random = new();
      return random.Next(1, 15);
    }
  }
}